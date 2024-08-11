using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models;
using FoundationaLLM.Core.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Core.Services;

/// <ineritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="CoreService"/> class.
/// </summary>
/// <param name="cosmosDbService">The Azure Cosmos DB service that contains
/// chat sessions and messages.</param>
/// <param name="downstreamAPIServices">The services used to make calls to
/// the downstream APIs.</param>
/// <param name="logger">The logging interface used to log under the
/// <see cref="CoreService"/> type name.</param>
/// <param name="brandingSettings">The <see cref="ClientBrandingConfiguration"/>
/// settings retrieved by the injected <see cref="IOptions{TOptions}"/>.</param>
/// <param name="settings">The <see cref="CoreServiceSettings"/> settings for the service.</param>
/// <param name="callContext">Contains contextual data for the calling service.</param>
/// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
public partial class CoreService(
    ICosmosDbService cosmosDbService,
    IEnumerable<IDownstreamAPIService> downstreamAPIServices,
    ILogger<CoreService> logger,
    IOptions<ClientBrandingConfiguration> brandingSettings,
    IOptions<CoreServiceSettings> settings,
    ICallContext callContext,
    IEnumerable<IResourceProviderService> resourceProviderServices) : ICoreService
{
    private readonly ICosmosDbService _cosmosDbService = cosmosDbService;
    private readonly IDownstreamAPIService _gatekeeperAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.GatekeeperAPI);
    private readonly IDownstreamAPIService _orchestrationAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.OrchestrationAPI);
    private readonly ILogger<CoreService> _logger = logger;
    private readonly ICallContext _callContext = callContext;
    private readonly string _sessionType = brandingSettings.Value.KioskMode ? SessionTypes.KioskSession : SessionTypes.Session;
    private readonly CoreServiceSettings _settings = settings.Value;

    private readonly IResourceProviderService _attachmentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
    private readonly IResourceProviderService _agentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
    private readonly IResourceProviderService _azureOpenAIResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AzureOpenAI);
    private readonly IResourceProviderService _aiModelResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AIModel);
    private readonly IResourceProviderService _configurationResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration);

    /// <inheritdoc/>
    public async Task<List<Session>> GetAllChatSessionsAsync(string instanceId) =>
        await _cosmosDbService.GetSessionsAsync(_sessionType, _callContext.CurrentUserIdentity?.UPN ?? 
                                                              throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat sessions."));

    /// <inheritdoc/>
    public async Task<List<Message>> GetChatSessionMessagesAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        return await _cosmosDbService.GetSessionMessagesAsync(sessionId, _callContext.CurrentUserIdentity?.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));
    }

    /// <inheritdoc/>
    public async Task<Session> CreateNewChatSessionAsync(string instanceId, ChatSessionProperties chatSessionProperties)
    {
        ArgumentException.ThrowIfNullOrEmpty(chatSessionProperties.Name);

        Session session = new()
        {
            Name = chatSessionProperties.Name,
            Type = _sessionType,
            UPN = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when creating a new chat session.")
        };
        return await _cosmosDbService.InsertSessionAsync(session);
    }

    /// <inheritdoc/>
    public async Task<Session> RenameChatSessionAsync(string instanceId, string sessionId, ChatSessionProperties chatSessionProperties)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(chatSessionProperties.Name);

        return await _cosmosDbService.UpdateSessionNameAsync(sessionId, chatSessionProperties.Name);
    }

    /// <inheritdoc/>
    public async Task DeleteChatSessionAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        await _cosmosDbService.DeleteSessionAndMessagesAsync(sessionId);
    }

    /// <inheritdoc/>
    public async Task<Completion> GetChatCompletionAsync(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(completionRequest.SessionId);

            completionRequest = PrepareCompletionRequest(completionRequest);

            // Retrieve conversation, including latest prompt.
            var messages = await _cosmosDbService.GetSessionMessagesAsync(completionRequest.SessionId, _callContext.CurrentUserIdentity?.UPN ??
                throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat completions."));
            var messageHistoryList = messages
                .Select(message => new MessageHistoryItem(message.Sender, string.IsNullOrWhiteSpace(message.Text) ? "" : message.Text))
                .ToList();

            completionRequest.MessageHistory = messageHistoryList;

            // Add the user's UPN to the messages.
            var upn = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when adding prompt and completion messages.");
            // Create prompt message, then persist in Cosmos as transaction with the Session details.
            var promptMessage = new Message(completionRequest.SessionId, nameof(Participants.User), null, completionRequest.UserPrompt, null, null, upn, _callContext.CurrentUserIdentity?.Name);
            await AddSessionMessageAsync(completionRequest.SessionId, promptMessage);

            var agentOption = await ProcessGatekeeperOptions(completionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, completionRequest);

            // Update prompt tokens and add completion, then persist in Cosmos as transaction.
            // Add the user's UPN to the messages.
            promptMessage.Tokens = result.PromptTokens;
            promptMessage.Vector = result.UserPromptEmbedding;
            var completionMessage = new Message(completionRequest.SessionId, nameof(Participants.Assistant), result.CompletionTokens, result.Completion, null, null, upn, result.AgentName, result.Citations);
            var completionPromptText =
                $"User prompt: {result.UserPrompt}{Environment.NewLine}Agent: {result.AgentName}{Environment.NewLine}Prompt template: {(!string.IsNullOrWhiteSpace(result.FullPrompt) ? result.FullPrompt : result.PromptTemplate)}";
            var completionPrompt = new CompletionPrompt(completionRequest.SessionId, completionMessage.Id, completionPromptText);
            completionMessage.CompletionPromptId = completionPrompt.Id;

            await AddPromptCompletionMessagesAsync(completionRequest.SessionId, promptMessage, completionMessage, completionPrompt);

            return new Completion { Text = result.Completion };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion in session {SessionId} for user prompt [{UserPrompt}].",
                completionRequest.SessionId, completionRequest.UserPrompt);
            return new Completion { Text = "Could not generate a completion due to an internal error." };
        }
    }

    /// <inheritdoc/>
    public async Task<Completion> GetCompletionAsync(string instanceId, CompletionRequest directCompletionRequest)
    {
        try
        {
            directCompletionRequest = PrepareCompletionRequest(directCompletionRequest);

            var agentOption = await ProcessGatekeeperOptions(directCompletionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, directCompletionRequest);

            return new Completion { Text = result.Completion };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting completion for user prompt [{directCompletionRequest.UserPrompt}].");
            return new Completion { Text = "Could not generate a completion due to an internal error." };
        }
    }

    /// <inheritdoc/>
    public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest)
    {
        completionRequest = PrepareCompletionRequest(completionRequest);
        throw new NotImplementedException();
    }        

    /// <inheritdoc/>
    public Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<CompletionResponse> GetCompletionOperationResult(string instanceId, string operationId) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<ResourceProviderUpsertResult> UploadAttachment(string instanceId, AttachmentFile attachmentFile, string agentName, UnifiedUserIdentity userIdentity)
    {
        var agentBase = await _agentResourceProvider.GetResource<AgentBase>(
            $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Agent}/{AgentResourceTypeNames.Agents}/{agentName}",
            userIdentity);
        var aiModelBase = await _agentResourceProvider.GetResource<AIModelBase>(
            agentBase.AIModelObjectId!,
            userIdentity);
        var apiEndpointConfiguration = await _configurationResourceProvider.GetResource<APIEndpointConfiguration>(
            aiModelBase.EndpointObjectId!,
            userIdentity);

        var agentRequiresOpenAIAssistants = agentBase.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants);

        attachmentFile.SecondaryProvider = agentRequiresOpenAIAssistants
            ? ResourceProviderNames.FoundationaLLM_AzureOpenAI
            : null;
        var result = await _attachmentResourceProvider.UpsertResourceAsync<AttachmentFile, ResourceProviderUpsertResult>(
                $"/instance/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Attachment}/attachments/{attachmentFile.Name}",
                attachmentFile,
                _callContext.CurrentUserIdentity!);

        if (agentRequiresOpenAIAssistants)
        {
            var userName = userIdentity.UPN?.NormalizeUserPrincipalName() ?? userIdentity.UserId;
            var assistantUserContextName = $"{userName}-assistant-{instanceId.ToLower()}";
            var fileUserContextName = $"{userName}-file-{instanceId.ToLower()}";

            var fileUserContext = new FileUserContext
            {
                Name = fileUserContextName,
                UserPrincipalName = userName!,
                Endpoint = apiEndpointConfiguration.Url,
                AssistantUserContextName = assistantUserContextName,
                Files = new()
                {
                    {
                        result.ObjectId!,
                        new FileMapping
                        {
                            FoundationaLLMAttachmentObjectId = result.ObjectId!,
                            OriginalFileName = attachmentFile.OriginalFileName,
                            ContentType = attachmentFile.ContentType!
                        }
                    }
                }
            };

            _ = await _azureOpenAIResourceProvider.UpsertResourceAsync<FileUserContext, ResourceProviderUpsertResult>(
                $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{AzureOpenAIResourceTypeNames.FileUserContexts}/{fileUserContextName}",
                fileUserContext,
                userIdentity);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<AttachmentFile?> DownloadAttachment(string instanceId, string fileProvider, string fileId, UnifiedUserIdentity userIdentity)
    {
        try
        {
            if (fileProvider == ResourceProviderNames.FoundationaLLM_AzureOpenAI)
            {
                var userName = userIdentity.UPN?.NormalizeUserPrincipalName() ?? userIdentity.UserId;
                var fileUserContextName = $"{userName}-file-{instanceId.ToLower()}";

                var result = await _azureOpenAIResourceProvider.GetResource<FileContent>(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{AzureOpenAIResourceTypeNames.FileUserContexts}/{fileUserContextName}/{AzureOpenAIResourceTypeNames.FilesContent}/{fileId}",
                    userIdentity);

                return new AttachmentFile
                {
                    Name = result.Name,
                    OriginalFileName = result.OriginalFileName,
                    ContentType = result.ContentType,
                    Content = new MemoryStream(result.BinaryContent!.Value.ToArray())
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {FileId} from {FileProvider}.", fileId, fileProvider);
        }

        return null;
    }

    private IDownstreamAPIService GetDownstreamAPIService(AgentGatekeeperOverrideOption agentOption) =>
        ((agentOption == AgentGatekeeperOverrideOption.UseSystemOption) && _settings.BypassGatekeeper)
        || (agentOption == AgentGatekeeperOverrideOption.MustBypass)
            ? _orchestrationAPIService
            : _gatekeeperAPIService;

    private async Task<AgentGatekeeperOverrideOption> ProcessGatekeeperOptions(CompletionRequest completionRequest)
    {
        var agentBase = await _agentResourceProvider.GetResource<AgentBase>($"/{AgentResourceTypeNames.Agents}/{completionRequest.AgentName}", _callContext.CurrentUserIdentity ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving the agent settings."));

        if (agentBase?.GatekeeperSettings?.UseSystemSetting == false)
        {
            // Agent does not want to use system settings, however it does not have any Gatekeeper options either
            // Consequently, a request to bypass Gatekeeper will be returned.
            if (agentBase!.GatekeeperSettings!.Options == null || agentBase.GatekeeperSettings.Options.Length == 0)
                return AgentGatekeeperOverrideOption.MustBypass;

            completionRequest.GatekeeperOptions = agentBase.GatekeeperSettings.Options;
            return AgentGatekeeperOverrideOption.MustCall;
        }

        return AgentGatekeeperOverrideOption.UseSystemOption;
    }

    /// <summary>
    /// Add session message
    /// </summary>
    private async Task AddSessionMessageAsync(string sessionId, Message message)
    {
        var session = await _cosmosDbService.GetSessionAsync(sessionId);

        // Update session cache with tokens used.
        session.TokensUsed += message.Tokens;
       
        // Add the user's UPN to the messages.
        var upn = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when adding prompt and completion messages.");
        message.UPN = upn;

        // Adds the incoming message to the session and updates the session with token usage.
        await _cosmosDbService.UpsertSessionBatchAsync(message, session);
    }

    /// <summary>
    /// Add user prompt and AI assistance response to the chat session message list object and insert into the data service as a transaction.
    /// </summary>
    private async Task AddPromptCompletionMessagesAsync(string sessionId, Message promptMessage, Message completionMessage, CompletionPrompt completionPrompt)
    {
        var session = await _cosmosDbService.GetSessionAsync(sessionId);

        // Update session cache with tokens used.
        session.TokensUsed += promptMessage.Tokens;
        session.TokensUsed += completionMessage.Tokens;
        // Add the user's UPN to the messages.
        var upn = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when adding prompt and completion messages.");
        promptMessage.UPN = upn;
        completionMessage.UPN = upn;

        await _cosmosDbService.UpsertSessionBatchAsync(promptMessage, completionMessage, completionPrompt, session);
    }

    /// <inheritdoc/>
    public async Task<Message> RateMessageAsync(string instanceId, string id, string sessionId, bool? rating)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(sessionId);

        return await _cosmosDbService.UpdateMessageRatingAsync(id, sessionId, rating);
    }

    /// <inheritdoc/>
    public async Task<CompletionPrompt> GetCompletionPrompt(string instanceId, string sessionId, string completionPromptId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNullOrEmpty(completionPromptId);

        return await _cosmosDbService.GetCompletionPrompt(sessionId, completionPromptId);
    }

    /// <summary>
    /// Pre-processing of incoming completion request.
    /// </summary>
    /// <param name="request">The completion request.</param>
    /// <returns>The updated completion request with pre-processing applied.</returns>
    private CompletionRequest PrepareCompletionRequest(CompletionRequest request)
    {
        request.OperationId = Guid.NewGuid().ToString();
        return request;
    }

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex ChatSessionNameReplacementRegex();
}
