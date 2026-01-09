using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Orchestration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Azure.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureAI;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Utils;
using Google.Api.Gax;
using Google.Cloud.AIPlatform.V1;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using Conversation = FoundationaLLM.Common.Models.Conversation.Conversation;
using LongRunningOperation = FoundationaLLM.Common.Models.Orchestration.LongRunningOperation;
using Message = FoundationaLLM.Common.Models.Conversation.Message;

namespace FoundationaLLM.Core.Services;

/// <ineritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="CoreService"/> class.
/// </summary>
/// <param name="cosmosDBService">The Azure Cosmos DB service that contains
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
/// <param name="configuration">The <see cref="IConfiguration"/> service providing configuration settings.</param>
/// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to build HTTP clients.</param>
/// <param name="contextServiceClient">The <see cref="IContextServiceClient"/> used to interact with the Context API.</param>
/// <param name="userProfileService">The <see cref="IUserProfileService"/> used to manage user profiles.</param>
public partial class CoreService(
    IAzureCosmosDBService cosmosDBService,
    IEnumerable<IDownstreamAPIService> downstreamAPIServices,
    ILogger<CoreService> logger,
    IOptions<ClientBrandingConfiguration> brandingSettings,
    IOptions<CoreServiceSettings> settings,
    IOrchestrationContext callContext,
    IEnumerable<IResourceProviderService> resourceProviderServices,
    IConfiguration configuration,
    IHttpClientFactoryService httpClientFactory,
    IContextServiceClient contextServiceClient,
    IUserProfileService userProfileService) : ICoreService
{
    private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
    private readonly IDownstreamAPIService _gatekeeperAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.GatekeeperAPI);
    private readonly IDownstreamAPIService _orchestrationAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.OrchestrationAPI);
    private readonly ILogger<CoreService> _logger = logger;
    private readonly UnifiedUserIdentity _userIdentity = ValidateUserIdentity(callContext.CurrentUserIdentity);
    private readonly string _sessionType = brandingSettings.Value.KioskMode ? ConversationTypes.KioskSession : ConversationTypes.Session;
    private readonly CoreServiceSettings _settings = settings.Value;
    private readonly IContextServiceClient _contextServiceClient = contextServiceClient;
    private readonly IUserProfileService _userProfileService = userProfileService;
    private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    private readonly string _baseUrl = GetBaseUrl(configuration, httpClientFactory, callContext).GetAwaiter().GetResult();

    private readonly IResourceProviderService _attachmentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
    private readonly IResourceProviderService _agentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
    private readonly IResourceProviderService _azureAIResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AzureAI);
    private readonly IResourceProviderService _azureOpenAIResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AzureOpenAI);
    private readonly IResourceProviderService _aiModelResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AIModel);
    private readonly IResourceProviderService _configurationResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration);
    private readonly IResourceProviderService _conversationResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Conversation);

    private readonly HashSet<string> _azureOpenAIFileSearchFileExtensions =
        [.. settings.Value.AzureOpenAIAssistantsFileSearchFileExtensions
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.ToLowerInvariant())];

    private readonly HashSet<string> _azureAIAgentServiceFileSearchFileExtensions =
        [.. settings.Value.AzureAIAgentsFileSearchFileExtensions
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.ToLowerInvariant())];

    #region Agent

    /// <inheritdoc/>
    public async Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync(
        string instanceId)
    {
        var agentResults = await _agentResourceProvider.GetResourcesAsync<AgentBase>(
                instanceId,
                _userIdentity,
                new ResourceProviderGetOptions
                {
                    IncludeRoles = true,
                    IncludeActions = false,
                    LoadContent = false
                });

        var userProfile = await _userProfileService.GetUserProfileAsync(instanceId, agentResults);
        var enabledAgents = userProfile!.Agents.ToHashSet();

        foreach (var agentResult in agentResults)
        {
            agentResult.Properties ??= [];

            agentResult.Properties.Add(
                AgentPropertyNames.Enabled,
                enabledAgents.Contains(agentResult.Resource.ObjectId!));
        }

        return agentResults;
    }

    # endregion

    #region Conversation management - FoundationaLLM.Conversation resource provider

    /// <inheritdoc/>
    public async Task<List<Conversation>> GetAllConversationsAsync(string instanceId)
    {
        var result = await _conversationResourceProvider.GetResourcesAsync<Conversation>(
            instanceId,
            _userIdentity,
            new()
            {
                Parameters = new()
                {
                    { ConversationResourceProviderGetParameterNames.ConversationType, _sessionType }
                }
            });

        return result.Select(r => r.Resource).ToList();
    }

    /// <inheritdoc/>
    public async Task<Conversation> CreateConversationAsync(string instanceId, ConversationProperties chatSessionProperties)
    {
        ArgumentException.ThrowIfNullOrEmpty(chatSessionProperties.Name);

        var newConversationId = $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
        Conversation newConversation = new()
        {
            Id = newConversationId,
            SessionId = newConversationId,
            Name = newConversationId,
            DisplayName = chatSessionProperties.Name,
            Metadata = chatSessionProperties.Metadata,
            Type = _sessionType,
            UPN = _userIdentity.UPN!
        };

        _ = await _conversationResourceProvider.UpsertResourceAsync<Conversation, ResourceProviderUpsertResult<Conversation>>(
            instanceId,
            newConversation,
            _userIdentity);

        return newConversation;
    }

    /// <inheritdoc/>
    public async Task<Conversation> UpdateConversationAsync(string instanceId, string conversationId, ConversationProperties conversationProperties)
    {
        ArgumentNullException.ThrowIfNull(conversationId);

        if (string.IsNullOrWhiteSpace(conversationProperties.Name)
            && string.IsNullOrWhiteSpace(conversationProperties.Metadata))
            throw new CoreServiceException("The conversation name and metadata fields cannot be all empty.");

        var propertyValues = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(conversationProperties.Name))
            propertyValues.Add("/displayName", conversationProperties.Name);
        if (!string.IsNullOrWhiteSpace(conversationProperties.Metadata))
            propertyValues.Add("/metadata", conversationProperties.Metadata);

        var result = await _conversationResourceProvider.UpdateResourcePropertiesAsync<Conversation, ResourceProviderUpsertResult<Conversation>>(
            instanceId,
            conversationId,
            propertyValues,
            _userIdentity);

        return result.Resource!;
    }

    /// <inheritdoc/>
    public async Task DeleteConversationAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        await _conversationResourceProvider.DeleteResourceAsync<Conversation>(
            instanceId,
            sessionId,
            _userIdentity);
    }

    /// <inheritdoc/>
    public async Task<ConversationSummaryResponse> SummarizeConversationAsync(
        string instanceId,
        string sessionId,
        ConversationSummaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(instanceId);
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AgentName);

        try
        {
            // Verify that the conversation exists and that the user has access to it.
            var conversation = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

            // Get the agent to find its underlying AI model
            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                request.AgentName,
                _userIdentity);

            // Get the AI model object ID from the agent's workflow
            var aiModelObjectId = agentBase.Workflow?.MainAIModelObjectId;
            if (string.IsNullOrWhiteSpace(aiModelObjectId))
            {
                _logger.LogWarning(
                    "Agent {AgentName} does not have a main AI model configured. Skipping LLM summarization for conversation {SessionId}.",
                    request.AgentName, sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            // Extract the AI model name from the full resource path
            // The object ID format is: /instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/{modelName}
            var aiModelName = aiModelObjectId.Split('/').LastOrDefault();
            if (string.IsNullOrWhiteSpace(aiModelName))
            {
                _logger.LogWarning(
                    "Could not extract AI model name from object ID {AIModelObjectId}. Skipping LLM summarization for conversation {SessionId}.",
                    aiModelObjectId, sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            // Get the AI model resource
            var aiModel = await _aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                instanceId,
                aiModelName,
                _userIdentity);

            if (string.IsNullOrWhiteSpace(aiModel.DeploymentName))
            {
                _logger.LogWarning(
                    "AI model {AIModelName} does not have a deployment name. Skipping LLM summarization for conversation {SessionId}.",
                    aiModelName, sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            // Get the API endpoint configuration for the AI model
            var endpointName = aiModel.EndpointObjectId.Split('/').LastOrDefault();
            if (string.IsNullOrWhiteSpace(endpointName))
            {
                _logger.LogWarning(
                    "Could not extract endpoint name from {EndpointObjectId}. Skipping LLM summarization for conversation {SessionId}.",
                    aiModel.EndpointObjectId, sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            var endpointConfig = await _configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                instanceId,
                endpointName,
                _userIdentity);

            // Get the messages from the conversation
            var messages = await _cosmosDBService.GetConversationMessagesAsync(sessionId, _userIdentity.UPN ??
                throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));

            // Find the first user message and first agent response
            var firstUserMessage = messages.FirstOrDefault(m => m.Sender == nameof(Participants.User));
            var firstAgentMessage = messages.FirstOrDefault(m => m.Sender == nameof(Participants.Agent));

            if (firstUserMessage == null || firstAgentMessage == null)
            {
                _logger.LogWarning("Cannot summarize conversation {SessionId}: missing user or agent message.", sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            // Get the text content from messages
            var userText = GetMessageText(firstUserMessage);
            var agentText = GetMessageText(firstAgentMessage);

            if (string.IsNullOrWhiteSpace(userText) || string.IsNullOrWhiteSpace(agentText))
            {
                _logger.LogWarning("Cannot summarize conversation {SessionId}: empty user or agent message text.", sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            // Build the summarization prompt
            var summarizationPrompt = $"""
                Generate a brief title (3-6 words) that summarizes the main topic of this conversation. Return only the title, no quotes or punctuation.

                User: {userText}
                Assistant: {agentText}

                Title:
                """;

            // Route to the appropriate provider based on endpoint configuration
            var summary = await GetSummarizationFromProvider(
                endpointConfig,
                aiModel.DeploymentName,
                summarizationPrompt,
                sessionId);

            if (string.IsNullOrWhiteSpace(summary))
            {
                _logger.LogWarning("Azure OpenAI returned empty summary for conversation {SessionId}.", sessionId);
                return new ConversationSummaryResponse { Summary = conversation.DisplayName ?? sessionId };
            }

            // Limit the summary length to a reasonable size
            if (summary.Length > 100)
            {
                summary = summary[..97] + "...";
            }

            // Persist the new name to Cosmos DB
            await UpdateConversationAsync(instanceId, sessionId, new ConversationProperties
            {
                Name = summary,
                Metadata = conversation.Metadata // preserve existing metadata
            });

            _logger.LogInformation("Successfully summarized conversation {SessionId} with title: {Summary} using deployment {DeploymentName}.",
                sessionId, summary, aiModel.DeploymentName);

            return new ConversationSummaryResponse { Summary = summary };
        }
        catch (ClientResultException crex) when (crex.Status == 429)
        {
            _logger.LogWarning(crex, "Rate limit exceeded while summarizing conversation {SessionId}. Keeping default name.", sessionId);
            return new ConversationSummaryResponse { Summary = "" };
        }
        catch (Amazon.Runtime.AmazonClientException awsEx) when (awsEx.Message.Contains("Failed to resolve AWS credentials"))
        {
            // AWS credentials not configured - silently keep the timestamp name
            _logger.LogWarning("AWS credentials not configured for Bedrock. Keeping default conversation name for {SessionId}.", sessionId);
            return new ConversationSummaryResponse { Summary = "" };
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error summarizing conversation {SessionId}.", sessionId);
            throw;
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the conversation - just keep the default name
            _logger.LogWarning(ex, "Error summarizing conversation {SessionId}. Keeping default name.", sessionId);
            return new ConversationSummaryResponse { Summary = "" };
        }
    }

    /// <summary>
    /// Extracts the text content from a message.
    /// </summary>
    private static string GetMessageText(Message message)
    {
        // First try to get text from Content array (newer format)
        if (message.Content is { Count: > 0 })
        {
            var textContent = message.Content
                .Where(c => c.Type == MessageContentItemTypes.Text)
                .Select(c => c.Value)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(textContent))
                return textContent;
        }

        // Fall back to Text property (older format)
        return message.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets a summarization from the appropriate provider based on the endpoint configuration.
    /// </summary>
    private async Task<string> GetSummarizationFromProvider(
        APIEndpointConfiguration endpointConfig,
        string deploymentName,
        string prompt,
        string sessionId)
    {
        var provider = ResolveProvider(endpointConfig);

        return provider switch
        {
            APIEndpointProviders.MICROSOFT or APIEndpointProviders.OPENAI or APIEndpointProviders.AZUREAI =>
                await GetAzureOpenAISummarization(endpointConfig, deploymentName, prompt, sessionId),
            APIEndpointProviders.BEDROCK =>
                await GetBedrockSummarization(endpointConfig, deploymentName, prompt, sessionId),
            APIEndpointProviders.VERTEXAI =>
                await GetVertexAISummarization(endpointConfig, deploymentName, prompt, sessionId),
            _ => throw new CoreServiceException($"Unsupported provider '{provider}' for conversation summarization.")
        };
    }

    private string ResolveProvider(APIEndpointConfiguration endpointConfig)
    {
        // First try the configured provider.
        var normalizedProvider = NormalizeProvider(endpointConfig.Provider);

        // If it isn't recognized, attempt URL-based detection.
        if (!IsSupportedProvider(normalizedProvider))
        {
            normalizedProvider = NormalizeProvider(DetectProviderFromUrl(endpointConfig.Url));
        }

        if (!IsSupportedProvider(normalizedProvider))
        {
            var configured = string.IsNullOrWhiteSpace(endpointConfig.Provider) ? "unspecified" : endpointConfig.Provider;
            throw new CoreServiceException($"Unsupported provider '{configured}' for endpoint '{endpointConfig.Name}' with URL '{endpointConfig.Url}'.");
        }

        return normalizedProvider;
    }

    private static string NormalizeProvider(string? provider) =>
        (provider ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "" => string.Empty,
            "azure" or "azureopenai" or "aoai" => APIEndpointProviders.MICROSOFT,
            APIEndpointProviders.MICROSOFT => APIEndpointProviders.MICROSOFT,
            APIEndpointProviders.AZUREAI => APIEndpointProviders.AZUREAI,
            APIEndpointProviders.OPENAI => APIEndpointProviders.OPENAI,
            "amazon" or "aws" or "amazonbedrock" or "anthropic" => APIEndpointProviders.BEDROCK,
            APIEndpointProviders.BEDROCK => APIEndpointProviders.BEDROCK,
            "google" or "googleai" or "vertex" or "vertexai" or "gemini" => APIEndpointProviders.VERTEXAI,
            _ => provider?.Trim().ToLowerInvariant() ?? string.Empty
        };

    private static string DetectProviderFromUrl(string url)
    {
        if (url.Contains(".openai.azure.com", StringComparison.OrdinalIgnoreCase) ||
            url.Contains(".cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase))
            return APIEndpointProviders.MICROSOFT;

        if (url.Contains("openai.com", StringComparison.OrdinalIgnoreCase))
            return APIEndpointProviders.OPENAI;

        if (url.Contains("bedrock", StringComparison.OrdinalIgnoreCase) ||
            (url.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase) && url.Contains("bedrock", StringComparison.OrdinalIgnoreCase)))
            return APIEndpointProviders.BEDROCK;

        if (url.Contains("googleapis.com", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("vertexai", StringComparison.OrdinalIgnoreCase))
            return APIEndpointProviders.VERTEXAI;

        return string.Empty;
    }

    private static bool IsSupportedProvider(string provider) =>
        APIEndpointProviders.All.Contains(provider);

    /// <summary>
    /// Gets a summarization using Azure OpenAI.
    /// </summary>
    private async Task<string> GetAzureOpenAISummarization(
        APIEndpointConfiguration endpointConfig,
        string deploymentName,
        string prompt,
        string sessionId)
    {
        _logger.LogInformation(
            "Calling Azure OpenAI directly to summarize conversation {SessionId} using deployment {DeploymentName} at {Endpoint}.",
            sessionId, deploymentName, endpointConfig.Url);

        var azureOpenAIClient = new AzureOpenAIClient(
            new Uri(endpointConfig.Url),
            ServiceContext.AzureCredential,
            new AzureOpenAIClientOptions
            {
                NetworkTimeout = TimeSpan.FromSeconds(30),
                RetryPolicy = new ClientRetryPolicy(1)
            });

        var chatClient = azureOpenAIClient.GetChatClient(deploymentName);

        var chatCompletionOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 50,
            Temperature = 0.3f
        };

        var result = await chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            chatCompletionOptions);

        return result.Value.Content[0].Text?.Trim().Trim('"', '\'') ?? string.Empty;
    }

    /// <summary>
    /// Gets a summarization using AWS Bedrock (for Claude models).
    /// </summary>
    private async Task<string> GetBedrockSummarization(
        APIEndpointConfiguration endpointConfig,
        string deploymentName,
        string prompt,
        string sessionId)
    {
        _logger.LogInformation(
            "Calling AWS Bedrock directly to summarize conversation {SessionId} using model {ModelId} at {Endpoint}.",
            sessionId, deploymentName, endpointConfig.Url);

        // Extract region from endpoint URL (e.g., https://bedrock-runtime.us-east-1.amazonaws.com/)
        var region = ExtractAwsRegionFromUrl(endpointConfig.Url);

        var bedrockConfig = new AmazonBedrockRuntimeConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region),
            Timeout = TimeSpan.FromSeconds(30),
            ServiceURL = endpointConfig.Url
        };

        // Log all available authentication parameters
        var authParamsLog = string.Join(", ", endpointConfig.AuthenticationParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        _logger.LogInformation(
            "Bedrock endpoint auth config - Type: {AuthType}, Params: [{AuthParams}], URL: {Url}",
            endpointConfig.AuthenticationType, authParamsLog, endpointConfig.Url);

        AWSCredentials awsCredentials;

        if (endpointConfig.AuthenticationType == AuthenticationTypes.AzureIdentity)
        {
            // Use Azure Workload Identity Federation with AWS IAM role assumption
            awsCredentials = await GetAwsCredentialsFromAzureIdentity(endpointConfig, region);
        }
        else
        {
            // Fall back to explicit credentials or default credential chain
            var accessKeyIdConfigName = GetAuthenticationParameter(endpointConfig, "aws_access_key_id_configuration_name");
            var secretAccessKeyConfigName = GetAuthenticationParameter(endpointConfig, "aws_secret_access_key_configuration_name");

            if (!string.IsNullOrWhiteSpace(accessKeyIdConfigName) && !string.IsNullOrWhiteSpace(secretAccessKeyConfigName))
            {
                var accessKeyId = _configuration.GetValue<string>(accessKeyIdConfigName);
                var secretAccessKey = _configuration.GetValue<string>(secretAccessKeyConfigName);

                if (!string.IsNullOrWhiteSpace(accessKeyId) && !string.IsNullOrWhiteSpace(secretAccessKey))
                {
                    awsCredentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
                }
                else
                {
                    _logger.LogWarning("AWS credentials not found in configuration. Using default AWS credential chain.");
                    awsCredentials = FallbackCredentialsFactory.GetCredentials();
                }
            }
            else
            {
                awsCredentials = FallbackCredentialsFactory.GetCredentials();
            }
        }

        using var bedrockClient = new AmazonBedrockRuntimeClient(awsCredentials, bedrockConfig);

        // Use the Converse API for Claude models
        var converseRequest = new ConverseRequest
        {
            ModelId = deploymentName,
            Messages =
            [
                new Amazon.BedrockRuntime.Model.Message
                {
                    Role = ConversationRole.User,
                    Content = [new ContentBlock { Text = prompt }]
                }
            ],
            InferenceConfig = new InferenceConfiguration
            {
                MaxTokens = 50,
                Temperature = 0.3f
            }
        };

        var response = await bedrockClient.ConverseAsync(converseRequest);

        var outputText = response.Output?.Message?.Content?
            .FirstOrDefault()?.Text?.Trim().Trim('"', '\'') ?? string.Empty;

        _logger.LogInformation("Successfully summarized conversation {SessionId} using AWS Bedrock.", sessionId);

        return outputText;
    }

    /// <summary>
    /// Gets AWS credentials by exchanging an Azure AD token for AWS temporary credentials using STS AssumeRoleWithWebIdentity.
    /// </summary>
    private async Task<AWSCredentials> GetAwsCredentialsFromAzureIdentity(
        APIEndpointConfiguration endpointConfig,
        string region)
    {
        // Get the scope and role ARN from the authentication parameters
        var scopeConfigKey = GetAuthenticationParameter(endpointConfig, "scope");
        var roleArnConfigKey = GetAuthenticationParameter(endpointConfig, "role_arn");

        if (string.IsNullOrWhiteSpace(scopeConfigKey) || string.IsNullOrWhiteSpace(roleArnConfigKey))
        {
            throw new CoreServiceException(
                "AWS Bedrock endpoint with AzureIdentity authentication requires 'scope' and 'role_arn' parameters.");
        }

        // Get the actual values from configuration (these config keys point to the actual values)
        var scope = _configuration.GetValue<string>(scopeConfigKey);
        var roleArn = _configuration.GetValue<string>(roleArnConfigKey);

        _logger.LogDebug(
            "AWS config lookup - ScopeKey: {ScopeKey}, ScopeValue: {Scope}, RoleArnKey: {RoleArnKey}, RoleArnValue: {RoleArn}",
            scopeConfigKey, scope ?? "(null)", roleArnConfigKey, roleArn ?? "(null)");

        if (string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(roleArn))
        {
            throw new CoreServiceException(
                $"AWS Bedrock configuration values not found for scope key '{scopeConfigKey}' or role ARN key '{roleArnConfigKey}'.");
        }

        _logger.LogDebug("Getting Azure AD token with scope {Scope} for AWS role {RoleArn}", scope, roleArn);

        // Get Azure AD token with the specified scope
        var tokenRequestContext = new Azure.Core.TokenRequestContext([scope]);
        var azureToken = await ServiceContext.AzureCredential!.GetTokenAsync(tokenRequestContext, default);

        _logger.LogDebug("Successfully obtained Azure AD token, exchanging for AWS credentials...");

        // Exchange the Azure AD token for AWS temporary credentials
        var stsConfig = new AmazonSecurityTokenServiceConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        };

        using var stsClient = new AmazonSecurityTokenServiceClient(new AnonymousAWSCredentials(), stsConfig);

        var assumeRoleRequest = new AssumeRoleWithWebIdentityRequest
        {
            RoleArn = roleArn,
            RoleSessionName = $"foundationallm-core-{Guid.NewGuid():N}",
            WebIdentityToken = azureToken.Token,
            DurationSeconds = 900 // 15 minutes
        };

        var assumeRoleResponse = await stsClient.AssumeRoleWithWebIdentityAsync(assumeRoleRequest);

        _logger.LogInformation(
            "Successfully assumed AWS role {RoleArn} using Azure AD token. Credentials expire at {Expiration}.",
            roleArn, assumeRoleResponse.Credentials.Expiration);

        return new SessionAWSCredentials(
            assumeRoleResponse.Credentials.AccessKeyId,
            assumeRoleResponse.Credentials.SecretAccessKey,
            assumeRoleResponse.Credentials.SessionToken);
    }

    /// <summary>
    /// Gets an authentication parameter value from the endpoint configuration.
    /// </summary>
    private static string? GetAuthenticationParameter(APIEndpointConfiguration endpointConfig, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (endpointConfig.AuthenticationParameters.TryGetValue(key, out var value))
            {
                return value?.ToString();
            }

            var match = endpointConfig.AuthenticationParameters
                .FirstOrDefault(kvp => string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(match.Key))
            {
                return match.Value?.ToString();
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the AWS region from a Bedrock endpoint URL.
    /// </summary>
    private static string ExtractAwsRegionFromUrl(string url)
    {
        // URL format: https://bedrock-runtime.{region}.amazonaws.com/
        var uri = new Uri(url);
        var parts = uri.Host.Split('.');
        if (parts.Length >= 3 && parts[0] == "bedrock-runtime")
        {
            return parts[1];
        }
        // Default to us-east-1 if we can't extract the region
        return "us-east-1";
    }

    /// <summary>
    /// Gets a summarization using Google Vertex AI (for Gemini models).
    /// </summary>
    private async Task<string> GetVertexAISummarization(
        APIEndpointConfiguration endpointConfig,
        string deploymentName,
        string prompt,
        string sessionId)
    {
        _logger.LogInformation(
            "Calling Google Vertex AI directly to summarize conversation {SessionId} using model {ModelId} at {Endpoint}.",
            sessionId, deploymentName, endpointConfig.Url);

        // Extract project and location from endpoint URL
        // URL format: https://{location}-aiplatform.googleapis.com/
        var (projectId, location) = ExtractVertexAIProjectAndLocation(endpointConfig);

        // Build the model resource name
        var modelResourceName = $"projects/{projectId}/locations/{location}/publishers/google/models/{deploymentName}";

        // Create the prediction service client
        var predictionServiceClient = await new PredictionServiceClientBuilder
        {
            Endpoint = $"{location}-aiplatform.googleapis.com"
        }.BuildAsync();

        // Create the request using GenerateContent
        var generateContentRequest = new GenerateContentRequest
        {
            Model = modelResourceName,
            Contents =
            {
                new Content
                {
                    Role = "user",
                    Parts = { new Part { Text = prompt } }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                MaxOutputTokens = 50,
                Temperature = 0.3f
            }
        };

        var response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

        var outputText = response.Candidates?
            .FirstOrDefault()?.Content?.Parts?
            .FirstOrDefault()?.Text?.Trim().Trim('"', '\'') ?? string.Empty;

        return outputText;
    }

    /// <summary>
    /// Extracts the project ID and location from a Vertex AI endpoint configuration.
    /// </summary>
    private static (string projectId, string location) ExtractVertexAIProjectAndLocation(APIEndpointConfiguration endpointConfig)
    {
        // Try to get project ID from authentication parameters or environment.
        var projectId = GetAuthenticationParameter(endpointConfig, "project_id", "projectId", "project")
                         ?? Platform.Instance()?.ProjectId
                         ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
                         ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID")
                         ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_NUMBER")
                         ?? Environment.GetEnvironmentVariable("GCLOUD_PROJECT")
                         ?? string.Empty;

        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new CoreServiceException($"Google Vertex AI project id not configured for endpoint '{endpointConfig.Name}'.");
        }

        // Extract location from authentication parameters or URL (e.g., https://us-central1-aiplatform.googleapis.com/)
        var location = GetAuthenticationParameter(endpointConfig, "location", "region");
        if (string.IsNullOrWhiteSpace(location))
        {
            location = "us-central1"; // Default
            var uri = new Uri(endpointConfig.Url);
            var hostParts = uri.Host.Split('-');
            if (hostParts.Length >= 2 && hostParts[^1].StartsWith("aiplatform", StringComparison.OrdinalIgnoreCase))
            {
                // Reconstruct location from parts before "aiplatform"
                location = string.Join("-", hostParts.Take(hostParts.Length - 1));
            }
        }

        return (projectId, location);
    }

    #endregion

    #region Asynchronous completion operations

    /// <inheritdoc/>
    public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(completionRequest.SessionId);
            var operationStartTime = DateTime.UtcNow;

            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                completionRequest.AgentName!,
                _userIdentity);

            if (AgentExpired(agentBase, out var message))
            {
                return new LongRunningOperation
                {
                    OperationId = completionRequest.OperationId,
                    Status = OperationStatus.Failed,
                    StatusMessage = message
                };
            }

            _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, completionRequest.SessionId, _userIdentity);

            completionRequest = await PrepareCompletionRequest(instanceId, completionRequest, agentBase, true);

            var conversationItems = await CreateConversationItemsAsync(completionRequest, _userIdentity);

            var agentOption = GetGatekeeperOption(agentBase, completionRequest);

            var operationContext = new LongRunningOperationContext
            {
                InstanceId = instanceId,
                OperationId = completionRequest.OperationId!,
                AgentName = completionRequest.AgentName!,
                SessionId = completionRequest.SessionId!,
                UserMessageId = conversationItems.UserMessage.Id,
                AgentMessageId = conversationItems.AgentMessage.Id,
                CompletionPromptId = conversationItems.CompletionPrompt.Id,
                GatekeeperOverride = agentOption,
                SemanticCacheSettings = (agentBase.CacheSettings?.SemanticCacheEnabled ?? false)
                    ? agentBase.CacheSettings?.SemanticCacheSettings
                    : null,
                StartTime = operationStartTime,
                UPN = _userIdentity.UPN!
            };
            await _cosmosDBService.UpsertLongRunningOperationContextAsync(operationContext);

            // Start the completion operation.
            var operation = await GetDownstreamAPIService(agentOption).StartCompletionOperation(instanceId, completionRequest);

            switch (operation.Status)
            {
                case OperationStatus.Failed:
                    {
                        // In case the completion operation fails to start properly, we need to update the user and agent messages accordingly.

                        var patchOperations = new List<IPatchOperationItem>
                        {
                            new PatchOperationItem<Message>
                            {
                                ItemId = conversationItems.UserMessage.Id,
                                PropertyValues = new Dictionary<string, object?>
                                {
                                    { "/status", OperationStatus.Failed }
                                }
                            },
                            new PatchOperationItem<Message>
                            {
                                ItemId = conversationItems.AgentMessage.Id,
                                PropertyValues = new Dictionary<string, object?>
                                {
                                    { "/status", OperationStatus.Failed },
                                    { "/text", operation.StatusMessage }
                                }
                            }
                        };

                        var patchedItems = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
                            completionRequest.SessionId!,
                            patchOperations
                        );

                        operation.Result = new Message
                        {
                            OperationId = operation.OperationId,
                            Status = operation.Status,
                            Text = operation.StatusMessage!,
                            TimeStamp = DateTime.UtcNow,
                            SenderDisplayName = operationContext.AgentName
                        };

                        break;
                    }
                case OperationStatus.Completed:
                    {
                        // If the completion operation completes immediately, we need to process the completion response and update the user and agent messages accordingly.

                        var processedOperation = await ProcessLongRunningOperation(operationContext, operation);
                        return processedOperation;
                    }
                default:
                    break;
            }

            return operation;
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error starting completion operation in conversation {SessionId}.",
                completionRequest.SessionId);

            return new LongRunningOperation
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                StatusMessage = rpex.StatusCode == StatusCodes.Status403Forbidden
                              ? "Could not start completion operation because access is forbidden."
                              : "Could not start completion operation due to an internal error."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting completion operation in conversation {SessionId}.",
                completionRequest.SessionId);

            // TODO: Depending on the type of failure, we should update the agent message to reflect the failure.

            return new LongRunningOperation
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                StatusMessage = "Could not start completion operation due to an internal error."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
    {
        try
        {
            var operationContext = await _cosmosDBService.GetLongRunningOperationContextAsync(operationId);

            _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, operationContext.SessionId, _userIdentity);

            var operation = await GetDownstreamAPIService(operationContext.GatekeeperOverride).GetCompletionOperationStatus(instanceId, operationId);

            var processedOperation = await ProcessLongRunningOperation(operationContext, operation);
            return processedOperation;
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error retrieving the status for the operation with id {OperationId}.",
                operationId);

            return new LongRunningOperation
            {
                OperationId = operationId,
                Status = OperationStatus.Failed,
                StatusMessage = rpex.StatusCode == StatusCodes.Status403Forbidden
                              ? "Could not retrieve the status of the operation because access is forbidden."
                              : "Could not retrieve the status of the operation due to an internal error."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving the status for the operation with id {OperationId}.",
                operationId);
            return new LongRunningOperation
            {
                OperationId = operationId,
                StatusMessage = "Could not retrieve the status of the operation due to an internal error.",
                Result = new Message
                {
                    OperationId = operationId,
                    Status = OperationStatus.Failed,
                    Text = "Could not retrieve the status of the operation due to an internal error.",
                    TimeStamp = DateTime.UtcNow
                },
                Status = OperationStatus.Failed
            };
        }
    }

    private async Task<LongRunningOperation> ProcessLongRunningOperation(
        LongRunningOperationContext operationContext,
        LongRunningOperation operation)
    {
        if ((DateTime.UtcNow - operationContext.StartTime).TotalMinutes > 30
                && (operation.Status == OperationStatus.InProgress))
        {
            // We've hit the hard stop time for the operation.

            var patchOperations = new List<IPatchOperationItem>
                {
                    new PatchOperationItem<Message>
                    {
                        ItemId = operationContext.UserMessageId,
                        PropertyValues = new Dictionary<string, object?>
                        {
                            { "/status", OperationStatus.Failed },
                            { "/text", "The completion operation has exceeded the maximum time allowed." }
                        }
                    },
                    new PatchOperationItem<Message>
                    {
                        ItemId = operationContext.AgentMessageId,
                        PropertyValues = new Dictionary<string, object?>
                        {
                            { "/status", OperationStatus.Failed },
                            { "/text", "The completion operation has exceeded the maximum time allowed." },
                            { "/timeStamp", DateTime.UtcNow }
                        }
                    }
                };

            _ = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
                operationContext.SessionId,
                patchOperations
            );

            return new LongRunningOperation
            {
                OperationId = operation.OperationId,
                StatusMessage = "The completion operation has exceeded the maximum time allowed.",
                Result = new Message
                {
                    OperationId = operation.OperationId,
                    Status = OperationStatus.Failed,
                    Text = "The completion operation has exceeded the maximum time allowed.",
                    TimeStamp = DateTime.UtcNow,
                    SenderDisplayName = operationContext.AgentName
                },
                Status = OperationStatus.Failed
            };
        }

        if (operation.Result is JsonElement jsonElement)
        {
            var completionResponse = jsonElement.Deserialize<CompletionResponse>();

            var agentMessage = await ProcessCompletionResponse(
                operationContext,
                completionResponse!,
                operation.Status);

            if (agentMessage.Content is { Count: > 0 })
            {
                foreach (var content in agentMessage.Content)
                {
                    content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                }
            }

            operation.Result = agentMessage;
            if (completionResponse != null)
            {
                operation.PromptTokens = completionResponse.PromptTokens;
            }

            return operation;
        }

        operation.Result = new Message
        {
            OperationId = operation.OperationId,
            Status = operation.Status,
            Text = operation.StatusMessage ?? "The completion operation is in progress.",
            TimeStamp = DateTime.UtcNow,
            SenderDisplayName = operationContext.AgentName
        };

        return operation;
    }

    #endregion

    #region Synchronous completion operations

    /// <inheritdoc/>
    public async Task<Message> GetChatCompletionAsync(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(completionRequest.SessionId);
            var operationStartTime = DateTime.UtcNow;

            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                completionRequest.AgentName!,
                _userIdentity);

            if (AgentExpired(agentBase, out var message))
            {
                return new Message
                {
                    OperationId = completionRequest.OperationId,
                    Status = OperationStatus.Failed,
                    Text = message
                };
            }

            _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, completionRequest.SessionId, _userIdentity);

            completionRequest = await PrepareCompletionRequest(instanceId, completionRequest, agentBase);

            var conversationItems = await CreateConversationItemsAsync(completionRequest, _userIdentity);

            var agentOption = GetGatekeeperOption(agentBase, completionRequest);

            // Generate the completion to return to the user.
            var completionResponse = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, completionRequest);

            var agentMessage = await ProcessCompletionResponse(
                new LongRunningOperationContext
                {
                    InstanceId = instanceId,
                    AgentName = completionRequest.AgentName!,
                    SessionId = completionRequest.SessionId!,
                    OperationId = completionRequest.OperationId!,
                    UserMessageId = conversationItems.UserMessage.Id,
                    AgentMessageId = conversationItems.AgentMessage.Id,
                    CompletionPromptId = conversationItems.CompletionPrompt.Id,
                    GatekeeperOverride = agentOption,
                    SemanticCacheSettings = (agentBase.CacheSettings?.SemanticCacheEnabled ?? false)
                        ? agentBase.CacheSettings?.SemanticCacheSettings
                        : null,
                    StartTime = operationStartTime,
                    UPN = _userIdentity.UPN!
                },
                completionResponse,
                OperationStatus.Completed);

            if (agentMessage.Content is { Count: > 0 })
            {
                foreach (var content in agentMessage.Content)
                {
                    content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                }
            }

            return agentMessage;
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error getting completion in conversation {SessionId} for user prompt [{UserPrompt}].",
                 completionRequest.SessionId, completionRequest.UserPrompt);

            return new Message
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = rpex.StatusCode == StatusCodes.Status403Forbidden
                     ? "Could not generate a completion because access is forbidden."
                     : "Could not generate a completion due to an internal error."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion in conversation {SessionId} for user prompt [{UserPrompt}].",
                completionRequest.SessionId, completionRequest.UserPrompt);
            return new Message
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = "Could not generate a completion due to an internal error."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<Message> GetCompletionAsync(string instanceId, CompletionRequest directCompletionRequest)
    {
        try
        {
            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                directCompletionRequest.AgentName!,
                _userIdentity);

            if (AgentExpired(agentBase, out var message))
            {
                return new Message
                {
                    OperationId = directCompletionRequest.OperationId,
                    Status = OperationStatus.Failed,
                    Text = message
                };
            }

            directCompletionRequest = await PrepareCompletionRequest(instanceId, directCompletionRequest, agentBase);

            var agentOption = GetGatekeeperOption(agentBase, directCompletionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, directCompletionRequest);

            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Completed,
                Text = result.Completion
                    ?? (result.Content?.Where(c => c.Type == MessageContentItemTypes.Text).FirstOrDefault() as TextMessageContentItem)?.Value
                        ?? "Could not generate a completion due to an internal error."
            };
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, $"Error getting completion for user prompt [{{UserPrompt}}].", directCompletionRequest.UserPrompt);

            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = rpex.StatusCode == StatusCodes.Status403Forbidden
                     ? "Could not generate a completion because access is forbidden."
                     : "Could not generate a completion due to an internal error."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting completion for user prompt [{{UserPrompt}}].", directCompletionRequest.UserPrompt);

            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = "Could not generate a completion due to an internal error."
            };
        }
    }

    #endregion

    #region Attachments

    /// <inheritdoc/>
    public async Task<ResourceProviderUpsertResult<AttachmentFile>> UploadAttachment(
        string instanceId, string sessionId, AttachmentFile attachmentFile, string agentName)
    {
        var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(instanceId, agentName, _userIdentity);

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        var agentRequiresOpenAIAssistants = agentBase.HasAzureOpenAIAssistantsWorkflow();
        var agentRequiresAzureAIAgentService = agentBase.HasAzureAIAgentServiceWorkflow();

        if (agentRequiresOpenAIAssistants)
        {
            var aiModelBase = await _aiModelResourceProvider.GetResourceAsync<AIModelBase>(agentBase.Workflow!.MainAIModelObjectId!, _userIdentity);

            attachmentFile.SecondaryProvider = agentRequiresOpenAIAssistants ? ResourceProviderNames.FoundationaLLM_AzureOpenAI : ResourceProviderNames.FoundationaLLM_AzureAI;

            var attachmentUpsertResult = await _attachmentResourceProvider.UpsertResourceAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile,
                _userIdentity);

            var apiEndpointConfiguration = await _configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(aiModelBase.EndpointObjectId!, _userIdentity);

            var fileMapping = new AzureOpenAIFileMapping
            {
                Name = string.Empty,
                Id = string.Empty,
                UPN = _userIdentity.UPN!,
                InstanceId = instanceId,
                FileObjectId = attachmentUpsertResult.ObjectId!,
                OriginalFileName = attachmentFile.OriginalFileName,
                FileContentType = attachmentFile.ContentType!,
                OpenAIEndpoint = apiEndpointConfiguration.Url,
                OpenAIFileId = string.Empty
            };

            var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
            {
                Parameters = new()
                {
                    { AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId, agentBase.ObjectId! },
                    { AzureOpenAIResourceProviderUpsertParameterNames.ConversationId, sessionId },
                    { AzureOpenAIResourceProviderUpsertParameterNames.AttachmentObjectId, attachmentUpsertResult.ObjectId },
                    { AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIFile, true }
                }
            };

            var extension = Path.GetExtension(attachmentFile.OriginalFileName).ToLowerInvariant().Replace(".", string.Empty);
            if (_azureOpenAIFileSearchFileExtensions.Contains(extension))
            {
                // The file also needs to be vectorized for the OpenAI assistant.
                fileMapping.FileRequiresVectorization = true;
            }

            var fileMappingUpsertResult = await _azureOpenAIResourceProvider.UpsertResourceAsync<AzureOpenAIFileMapping, ResourceProviderUpsertResult<AzureOpenAIFileMapping>>(
                instanceId,
                fileMapping,
                _userIdentity,
                resourceProviderUpsertOptions);

            var newFileId = fileMappingUpsertResult.Resource!.OpenAIFileId;            
            
            await _attachmentResourceProvider.UpdateResourcePropertiesAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile.Name!,
                new Dictionary<string, object?>
                {
                    { "/secondaryProviderObjectId", newFileId }
                },
                _userIdentity);
            attachmentUpsertResult.Resource!.SecondaryProviderObjectId = newFileId;

            return attachmentUpsertResult;
        }

        if (agentRequiresAzureAIAgentService)
        {
            attachmentFile.SecondaryProvider = ResourceProviderNames.FoundationaLLM_AzureAI;
            var attachmentUpsertResult = await _attachmentResourceProvider.UpsertResourceAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile,
                _userIdentity);

            var workflow = agentBase.Workflow as AzureAIAgentServiceAgentWorkflow;
            var fileMapping = new AzureAIAgentFileMapping
            {
                Name = string.Empty,
                Id = string.Empty,
                UPN = _userIdentity.UPN!,
                InstanceId = instanceId,
                FileObjectId = attachmentUpsertResult.ObjectId!,
                OriginalFileName = attachmentFile.OriginalFileName,
                FileContentType = attachmentFile.ContentType!,
                ProjectConnectionString = workflow!.ProjectConnectionString!,
                AzureAIAgentFileId = string.Empty
            };

            var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
            {
                Parameters = new()
                {
                    { AzureAIResourceProviderUpsertParameterNames.AgentObjectId, agentBase.ObjectId! },
                    { AzureAIResourceProviderUpsertParameterNames.ConversationId, sessionId },
                    { AzureAIResourceProviderUpsertParameterNames.AttachmentObjectId, attachmentUpsertResult.ObjectId },
                    { AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentFile, true }
                }
            };

            var extension = Path.GetExtension(attachmentFile.OriginalFileName).ToLowerInvariant().Replace(".", string.Empty);
            if (_azureAIAgentServiceFileSearchFileExtensions.Contains(extension))
            {
                // The file also needs to be vectorized for the Azure AI Agent Service agent.
                fileMapping.FileRequiresVectorization = true;
            }

            var fileMappingUpsertResult = await _azureAIResourceProvider.UpsertResourceAsync<AzureAIAgentFileMapping, ResourceProviderUpsertResult<AzureAIAgentFileMapping>>(
                instanceId,
                fileMapping,
                _userIdentity,
                resourceProviderUpsertOptions);

            var newFileId = fileMappingUpsertResult.Resource!.AzureAIAgentFileId;
            

            await _attachmentResourceProvider.UpdateResourcePropertiesAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile.Name!,
                new Dictionary<string, object?>
                {
                    { "/secondaryProviderObjectId", newFileId }
                },
                _userIdentity);
            attachmentUpsertResult.Resource!.SecondaryProviderObjectId = newFileId;

            return attachmentUpsertResult;
        }
       
        var serviceResult = await _contextServiceClient.CreateFileForConversation(
            instanceId,
            agentName,
            sessionId,
            attachmentFile.OriginalFileName,
            attachmentFile.ContentType!,
            attachmentFile.Content!);

            if (serviceResult.TryGetValue(out var fileRecord))
            {
                return new ResourceProviderUpsertResult<AttachmentFile>
                {
                    ObjectId = fileRecord.FileObjectId,
                    ResourceExists = false,
                    Resource = new AttachmentFile
                    {
                        Name = fileRecord.Id,
                        ObjectId = fileRecord.FileObjectId,
                        DisplayName = fileRecord.FileName,
                        CreatedBy = fileRecord.UPN,
                        CreatedOn = fileRecord.CreatedAt,

                        ContentType = fileRecord.ContentType,
                        Path = fileRecord.FilePath,
                        OriginalFileName = fileRecord.FileName,
                    }
                };
            }
            else
                throw new CoreServiceException(
                    serviceResult.Error?.Detail,
                    serviceResult.Error?.Status ?? StatusCodes.Status500InternalServerError);
        
    }

    /// <inheritdoc/>
    public async Task<AttachmentFile?> DownloadAttachment(string instanceId, string fileProvider, string fileId)
    {
        try
        {
            switch (fileProvider)
            {

                case ResourceProviderNames.FoundationaLLM_AzureOpenAI:

                    var result = await _azureOpenAIResourceProvider.ExecuteResourceActionAsync<AzureOpenAIFileMapping, object?, ResourceProviderActionResult<FileContent>>(
                        instanceId,
                        fileId,
                        ResourceProviderActions.LoadFileContent,
                        null,
                        _userIdentity);

                    return new AttachmentFile
                    {
                        Name = result.Resource!.Name,
                        OriginalFileName = result.Resource!.OriginalFileName,
                        ContentType = result.Resource!.ContentType,
                        Content = result.Resource!.BinaryContent
                    };
                case ResourceProviderNames.FoundationaLLM_AzureAI:
                    var azureAIResponse = await _azureAIResourceProvider.ExecuteResourceActionAsync<AzureAIAgentFileMapping, object?, ResourceProviderActionResult<FileContent>>(
                        instanceId,
                        fileId,
                        ResourceProviderActions.LoadFileContent,
                        null,
                        _userIdentity);

                    return new AttachmentFile
                    {
                        Name = azureAIResponse.Resource!.Name,
                        OriginalFileName = azureAIResponse.Resource!.OriginalFileName,
                        ContentType = azureAIResponse.Resource!.ContentType,
                        Content = azureAIResponse.Resource!.BinaryContent
                    };
                case ResourceProviderNames.FoundationaLLM_Context:

                    var serviceResult = await _contextServiceClient.GetFileContent(instanceId, fileId);

                    if (serviceResult.TryGetValue(out var fileContent))
                    {
                        return new AttachmentFile
                        {
                            Name = fileContent.FileName,
                            OriginalFileName = fileContent.FileName,
                            ContentType = fileContent.ContentType,
                            Content = fileContent.FileContent
                        };
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {FileId} from {FileProvider}.", fileId, fileProvider);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ResourceProviderDeleteResult?>> DeleteAttachments(
        string instanceId, List<string> resourcePaths)
    {
        var results = resourcePaths.ToDictionary(key => key, value => (ResourceProviderDeleteResult?)null);

        foreach (var rawResourcePath in resourcePaths)
        {
            try
            {
                var resourcePath = ResourcePath.GetResourcePath(rawResourcePath);
                if (string.IsNullOrWhiteSpace(resourcePath.ResourceProvider))
                    throw new ResourceProviderException(
                        $"Invalid resource provider for resource path [{rawResourcePath}].");

                switch (resourcePath.ResourceProvider)
                {
                    case ResourceProviderNames.FoundationaLLM_Attachment:
                        await _attachmentResourceProvider.HandleDeleteAsync(rawResourcePath, _userIdentity);
                        results[rawResourcePath] = new ResourceProviderDeleteResult()
                        {
                            Deleted = true
                        };
                        break;
                    case ResourceProviderNames.FoundationaLLM_Context:
                        var contextServiceResponse = await _contextServiceClient.DeleteFileRecord(resourcePath.InstanceId!, resourcePath.MainResourceId!);
                        results[rawResourcePath] = new ResourceProviderDeleteResult()
                        {
                            Deleted = contextServiceResponse.IsSuccess
                        };
                        break;
                    default:
                        throw new ResourceProviderException(
                            $"The resource provider [{resourcePath.ResourceProvider}] is not supported by the delete attachments endpoint.");
                }
            }
            catch (ResourceProviderException rpex)
            {
                _logger.LogError(rpex, "{Message}", rpex.Message);

                results[rawResourcePath] = new ResourceProviderDeleteResult()
                {
                    Deleted = false,
                    Reason = rpex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error when handling the deletion for resource path [{ResourcePath}].", rawResourcePath);

                results[rawResourcePath] = new ResourceProviderDeleteResult()
                {
                    Deleted = false,
                    Reason = $"There was an error when handling the deletion for resource path [{rawResourcePath}]."
                };
            }
        }

        return results;
    }

    #endregion

    #region Conversation messages

    /// <inheritdoc/>
    public async Task<List<Message>> GetConversationMessagesAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(instanceId);
        ArgumentNullException.ThrowIfNull(sessionId);

        // Verify that the conversation exists and that the user has access to it.
        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        var messages = await _cosmosDBService.GetConversationMessagesAsync(sessionId, _userIdentity.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));

        // Get a list of all attachment IDs in the messages.
        var attachmentIds = messages.SelectMany(m => m.Attachments ?? Enumerable.Empty<string>()).Distinct().ToList();
        if (attachmentIds.Count > 0)
        {
            var contextAttachmentIds = new List<string>();
            var legacyAttachmentIds = new List<string>();
            var attachmentReferences = new List<AttachmentDetail>();

            foreach (var attachmentObjectId in attachmentIds)
            {
                var resourcePath = ResourcePath.GetResourcePath(attachmentObjectId);
                switch (resourcePath.ResourceProvider)
                {
                    case ResourceProviderNames.FoundationaLLM_Attachment:

                        legacyAttachmentIds.Add(attachmentObjectId);
                        break;

                    case ResourceProviderNames.FoundationaLLM_Context:

                        contextAttachmentIds.Add(resourcePath.MainResourceId!);
                        break;

                    default:
                        throw new CoreServiceException(
                            $"The resource provider [{resourcePath.ResourceProvider}] is not supported for attachments.");
                }
            }

            if (legacyAttachmentIds.Count > 0)
            {
                var filter = new ResourceFilter
                {
                    ObjectIDs = legacyAttachmentIds
                };
                // Get the attachment details from the attachment resource provider.
                var result = await _attachmentResourceProvider!.HandlePostAsync(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Attachment}/{AttachmentResourceTypeNames.Attachments}/{ResourceProviderActions.Filter}",
                    JsonSerializer.Serialize(filter),
                    null,
                    _userIdentity);
                attachmentReferences.AddRange(from attachment in (IEnumerable<AttachmentFile>)result select AttachmentDetail.FromAttachmentFile(attachment));
            }
            else
            {
                var contextAttachmentResults =
                    contextAttachmentIds
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => await _contextServiceClient.GetFileRecord(instanceId, x));
                await foreach (var attachmentResult in contextAttachmentResults)
                {
                    if (attachmentResult.TryGetValue(out var fileRecord))
                        attachmentReferences.Add(AttachmentDetail.FromContextFileRecord(fileRecord));
                    else
                        _logger.LogWarning("Could not retrieve context file record for attachment with id {AttachmentId} in conversation {SessionId}. The error was: {ErrorDetail}",
                            attachmentResult.Error?.Instance ?? "N/A", sessionId, attachmentResult.Error?.Detail ?? "N/A");
                }
            }

            if (attachmentReferences.Count > 0)
            {
                // Add the attachment details to the messages.
                foreach (var message in messages)
                {
                    if (message.Attachments is { Count: > 0 })
                    {
                        var messageAttachmentDetails = new List<AttachmentDetail>();
                        foreach (var attachment in message.Attachments)
                        {
                            var attachmentDetail = attachmentReferences.FirstOrDefault(ad => ad.ObjectId == attachment);
                            if (attachmentDetail != null)
                            {
                                messageAttachmentDetails.Add(attachmentDetail);
                            }
                        }
                        message.AttachmentDetails = messageAttachmentDetails;
                    }
                }
            }
        }

        foreach (var message in messages)
        {
            if (message.Content is { Count: > 0 })
            {
                foreach (var content in message.Content)
                {
                    content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                }
            }
        }

        return [.. messages];
    }

    /// <inheritdoc/>
    public async Task<int> GetConversationMessagesCountAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(instanceId);
        ArgumentNullException.ThrowIfNull(sessionId);

        // Verify that the conversation exists and that the user has access to it.
        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        var messagesCount = await _cosmosDBService.GetConversationMessagesCountAsync(sessionId, _userIdentity.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));

        return messagesCount;
    }

    /// <inheritdoc/>
    public async Task RateMessageAsync(string instanceId, string id, string sessionId, MessageRatingRequest rating)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        await _cosmosDBService.PatchSessionsItemPropertiesAsync<Message>(
            id,
            sessionId,
            new Dictionary<string, object?>
            {
                { "/rating", rating.Rating },
                { "/ratingComments", rating.Comments }
            });
    }

    /// <inheritdoc/>
    public async Task<CompletionPrompt> GetCompletionPrompt(string instanceId, string sessionId, string completionPromptId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNullOrEmpty(completionPromptId);

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        return await _cosmosDBService.GetCompletionPromptAsync(sessionId, completionPromptId);
    }

    #endregion

    #region Configuration

    /// <inheritdoc/>
    public async Task<CoreConfiguration> GetCoreConfiguration(string instanceId)
    {
        var configuration = new CoreConfiguration
        {
            FileStoreConnectors =
                await GetFileStoreConnectors(instanceId, _userIdentity),
            MaxUploadsPerMessage =
                _settings.MaxUploadsPerMessage.GetValueForUser(_userIdentity.UPN!),
            CompletionResponsePollingIntervalMilliseconds =
                _settings.CompletionResponsePollingIntervalMilliseconds.GetValueForUser(_userIdentity.UPN!)
        };

        return configuration;
    }

    #endregion

    #region Private helper methods

    /// <inheritdoc/>
    private async Task<IEnumerable<APIEndpointConfiguration>> GetFileStoreConnectors(string instanceId, UnifiedUserIdentity userIdentity)
    {
        var apiEndpointConfigurations = await _configurationResourceProvider.GetResourcesAsync<APIEndpointConfiguration>(instanceId, userIdentity);
        var resources = apiEndpointConfigurations.Select(c => c.Resource).ToList();
        return resources.Where(c => c.Category == APIEndpointCategory.FileStoreConnector);
    }

    private IDownstreamAPIService GetDownstreamAPIService(AgentGatekeeperOverrideOption agentOption) =>
        ((agentOption == AgentGatekeeperOverrideOption.UseSystemOption) && _settings.BypassGatekeeper)
        || (agentOption == AgentGatekeeperOverrideOption.MustBypass)
            ? _orchestrationAPIService
            : _gatekeeperAPIService;

    private static AgentGatekeeperOverrideOption GetGatekeeperOption(
        AgentBase agent, CompletionRequest completionRequest)
    {
        if (agent.GatekeeperSettings?.UseSystemSetting == false)
        {
            // Agent does not want to use system settings, however it does not have any Gatekeeper options either
            // Consequently, a request to bypass Gatekeeper will be returned.
            if (agent.GatekeeperSettings!.Options == null || agent.GatekeeperSettings.Options.Length == 0)
                return AgentGatekeeperOverrideOption.MustBypass;

            completionRequest.GatekeeperOptions = agent.GatekeeperSettings.Options;
            return AgentGatekeeperOverrideOption.MustCall;
        }

        return AgentGatekeeperOverrideOption.UseSystemOption;
    }

    /// <summary>
    /// Add session message
    /// </summary>
    private async Task<(Message UserMessage, Message AgentMessage, CompletionPrompt CompletionPrompt)> CreateConversationItemsAsync(
        CompletionRequest request, UnifiedUserIdentity userIdentity)
    {
        var userMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId!,
            Sender = nameof(Participants.User),
            Text = request.UserPrompt,
            UPN = userIdentity.UPN!,
            SenderDisplayName = userIdentity.Name,
            Attachments = request.Attachments,
            Status = OperationStatus.InProgress,
            OperationId = request.OperationId
        };

        var agentMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId!,
            Sender = nameof(Participants.Agent),
            UPN = userIdentity.UPN!,
            SenderDisplayName = request.AgentName,
            Status = OperationStatus.InProgress,
            OperationId = request.OperationId
        };

        var completionPrompt = new CompletionPrompt
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId!,
            MessageId = agentMessage.Id,
            Prompt = string.Empty
        };

        agentMessage.CompletionPromptId = completionPrompt.Id;

        // Adds the incoming message to the session and updates the session with token usage.
        await _cosmosDBService.UpsertSessionBatchAsync(userMessage, agentMessage, completionPrompt);

        return (userMessage, agentMessage, completionPrompt);
    }

    private async Task<Message> ProcessCompletionResponse(
        LongRunningOperationContext operationContext, CompletionResponse completionResponse, OperationStatus operationStatus)
    {
        #region Process content

        var newContent = new List<MessageContent>();

        if (completionResponse.Content is { Count: > 0 })
        {
            foreach (var content in completionResponse.Content)
            {
                switch (content)
                {
                    case TextMessageContentItem textMessageContent:
                        if (textMessageContent.Annotations.Count > 0)
                        {
                            foreach (var annotation in textMessageContent.Annotations)
                            {
                                newContent.Add(new MessageContent
                                {
                                    Type = FileUtils.GetMessageContentFileType(annotation.Text, annotation.Type),
                                    FileName = annotation.Text,
                                    Value = annotation.FileUrl
                                });
                            }
                        }
                        newContent.Add(new MessageContent
                        {
                            Type = textMessageContent.Type,
                            Value = textMessageContent.Value
                        });
                        break;
                    case ImageFileMessageContentItem imageFileMessageContent:
                        newContent.Add(new MessageContent
                        {
                            Type = imageFileMessageContent.Type,
                            Value = imageFileMessageContent.FileUrl
                        });
                        break;
                }
            }
        }

        #endregion

        var completionPromptText =
            $"User prompt: {completionResponse.UserPrompt}{Environment.NewLine}Agent: {completionResponse.AgentName}{Environment.NewLine}Prompt template: {(!string.IsNullOrWhiteSpace(completionResponse.FullPrompt) ? completionResponse.FullPrompt : completionResponse.PromptTemplate)}";

        var patchOperations = new List<IPatchOperationItem>
        {
            // TODO: This update only needs to be done the first time a completion is processed (i.e. when status is retrieved).
            new PatchOperationItem<Message>
            {
                ItemId = operationContext.UserMessageId,
                PropertyValues = new Dictionary<string, object?>
                {
                    { "/tokens", completionResponse.PromptTokens },
                    { "/status", operationStatus },
                    { "/textRewrite", completionResponse.UserPromptRewrite }
                }
            },
            new PatchOperationItem<Message>
            {
                ItemId = operationContext.AgentMessageId,
                PropertyValues = new Dictionary<string, object?>
                {
                    { "/tokens", completionResponse.CompletionTokens },
                    { "/text", completionResponse.Completion },
                    { "/contentArtifacts", completionResponse.ContentArtifacts },
                    { "/content", newContent },
                    { "/analysisResults", completionResponse.AnalysisResults },
                    { "/status", operationStatus },
                    { "/timeStamp", DateTime.UtcNow }
                }
            },
            new PatchOperationItem<CompletionPrompt>
            {
                ItemId = operationContext.CompletionPromptId,
                PropertyValues = new Dictionary<string, object?> { { "/prompt", completionPromptText } }
            },
        };

        var patchedItems = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
            operationContext.SessionId,
            patchOperations
        );

        var agentMessage = patchedItems[operationContext.AgentMessageId] as Message;

        return agentMessage ?? new Message
        {
            OperationId = operationContext.OperationId,
            Status = OperationStatus.Failed,
            Text = "Could not retrieve the status of the operation due to an internal error."
        };
    }

    /// <summary>
    /// Pre-processing of incoming completion request.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance ID.</param>
    /// <param name="request">The completion request.</param>
    /// <param name="agent">The <see cref="AgentBase"/> resource object.</param>
    /// <param name="longRunningOperation">Indicates whether this is a long-running operation.</param>
    /// <returns>The updated completion request with pre-processing applied.</returns>
    private async Task<CompletionRequest> PrepareCompletionRequest(
        string instanceId,
        CompletionRequest request,
        AgentBase agent,
        bool longRunningOperation = false)
    {
        request.LongRunningOperation = longRunningOperation;

        if (request.Attachments is { Count: > 0 })
        {
            if (agent.ShowFileUpload is null
                || !agent.ShowFileUpload.Value)
                throw new CoreServiceException(
                    $"File uploads are not enabled for the {request.AgentName} agent.",
                    StatusCodes.Status400BadRequest);

            if (request.Attachments.Count > _settings.MaxUploadsPerMessage.GetValueForUser(_userIdentity.UPN!))
                throw new CoreServiceException(
                    $"The maximum number of attachments per message is {_settings.MaxUploadsPerMessage.Value}.",
                    StatusCodes.Status400BadRequest);
        }

        List<MessageHistoryItem> messageHistoryList = [];
        List<string> contentArtifactTypes = (agent.ConversationHistorySettings?.Enabled ?? false)
            ? [.. (agent.ConversationHistorySettings.HistoryContentArtifactTypes ?? string.Empty).Split(",", StringSplitOptions.RemoveEmptyEntries)]
            : [];

        // Retrieve the complete conversation.
        var messages = await _cosmosDBService.GetConversationMessagesAsync(request.SessionId!, _userIdentity.UPN!);
        var fileHistory = new List<FileHistoryItem>();
        int attachmentOrder = 0;
        foreach (var message in messages)
        {
            var messageText = message.Text;
            var messageTextRewrite = message.TextRewrite;
            if (message.Content is { Count: > 0 })
            {
                StringBuilder text = new();
                foreach (var content in message.Content.Where(content => content.Type == MessageContentItemTypes.Text))
                {
                    text.Append(content.Value);
                }
                messageText = text.ToString();
            }

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                var messageHistoryItem = new MessageHistoryItem(message.Sender, messageText, messageTextRewrite)
                {
                    ContentArtifacts = message.ContentArtifacts?.Where(ca => contentArtifactTypes.Contains(ca.Type ?? string.Empty)).ToList()
                };
                if (message.Attachments is { Count: > 0 })
                {
                    foreach (var attachmentObjectId in message.Attachments)
                    {
                        var resourcePath = ResourcePath.GetResourcePath(attachmentObjectId);
                        switch (resourcePath.ResourceProvider)
                        {
                            case ResourceProviderNames.FoundationaLLM_Attachment:

                                var file = await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(
                                    instanceId,
                                    resourcePath.MainResourceId!,
                                    _userIdentity);
                                fileHistory.Add(FileHistoryItem.FromAttachmentFile(file, ++attachmentOrder, false));

                                break;

                            case ResourceProviderNames.FoundationaLLM_Context:

                                var fileResponse = await _contextServiceClient.GetFileRecord(
                                    instanceId,
                                    resourcePath.MainResourceId!);
                                if (fileResponse.TryGetValue(out var fileRecord))
                                {
                                    if (fileRecord.AgentName != request.AgentName!)
                                    {
                                        _logger.LogError("Attachment agent name {AttachmentAgentName} does not match the current agent {CurrentAgentName} for attachment {AttachmentObjectId}.",
                                            fileRecord.AgentName, request.AgentName, attachmentObjectId);
                                        throw new CoreServiceException(
                                            $"Attachment agent name does not match the current agent for attachment {attachmentObjectId}.",
                                            StatusCodes.Status400BadRequest);
                                    }

                                    fileHistory.Add(FileHistoryItem.FromContextFileRecord(
                                        fileRecord,
                                        ++attachmentOrder,
                                        false,
                                        false));
                                }
                                else
                                {
                                    _logger.LogError("Failed to retrieve file record for attachment {AttachmentObjectId}.", attachmentObjectId);
                                }

                                break;
                            default:
                                throw new CoreServiceException(
                                    $"The resource provider [{resourcePath.ResourceProvider}] is not supported for attachments in the completion request.");
                        }
                    }
                }
                messageHistoryList.Add(messageHistoryItem);
            }
        }

        // If there is an attachment in the current message, add it to the file history.
        if (request.Attachments is { Count: > 0 })
        {
            foreach (var attachmentObjectId in request.Attachments)
            {
                var resourcePath = ResourcePath.GetResourcePath(attachmentObjectId);
                switch (resourcePath.ResourceProvider)
                {
                    case ResourceProviderNames.FoundationaLLM_Attachment:

                        var file = await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(
                            instanceId,
                            resourcePath.MainResourceId!,
                            _userIdentity);
                        fileHistory.Add(FileHistoryItem.FromAttachmentFile(file, ++attachmentOrder, true));

                        break;

                    case ResourceProviderNames.FoundationaLLM_Context:

                        var fileResponse = await _contextServiceClient.GetFileRecord(
                            instanceId,
                            resourcePath.MainResourceId!);
                        if (fileResponse.TryGetValue(out var fileRecord))
                        {
                            fileHistory.Add(FileHistoryItem.FromContextFileRecord(
                                fileRecord,
                                ++attachmentOrder,
                                true,
                                true));
                        }
                        else
                        {
                            _logger.LogError("Failed to retrieve file record for attachment {AttachmentObjectId}.", attachmentObjectId);
                        }

                        break;
                    default:
                        throw new CoreServiceException(
                            $"The resource provider [{resourcePath.ResourceProvider}] is not supported for attachments in the completion request.");
                }
            }
        }

        // Include conversation file history regardless of the conversation history settings.
        if (fileHistory.Count > 0)
        {
            request.FileHistory = fileHistory;
        }

        // Only include message history if the conversation history settings are enabled.
        if (string.IsNullOrWhiteSpace(request.SessionId) ||
            !(agent.ConversationHistorySettings?.Enabled ?? false))
            return request;

        // Truncate the message history based on configuration.
        var max = agent.ConversationHistorySettings?.MaxHistory * 2 ?? null;
        if (max.HasValue && messageHistoryList.Count > max)
        {
            // Remove messages from the beginning of the list.
            messageHistoryList.RemoveRange(0, messageHistoryList.Count - max.Value);
        }

        request.MessageHistory = messageHistoryList;
        return request;
    }

    private bool AgentExpired(AgentBase agentBase, out string message)
    {
        // Check if the agent is expired.
        if (agentBase.ExpirationDate != null && agentBase.ExpirationDate < DateTime.UtcNow)
        {
            _logger.LogWarning("User has attempted to access an expired agent: {AgentName}.",
                agentBase.Name);
            {
                message = "Could not complete your request because the agent has expired.";
                return true;
            }
        }
        message = string.Empty;
        return false;
    }

    private static async Task<string> GetBaseUrl(
        IConfiguration configuration,
        IHttpClientFactoryService httpClientFactory,
        IOrchestrationContext callContext)
    {
        var baseUrl = configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl]!;
        try
        {
            if (callContext.CurrentUserIdentity is { AssociatedWithAccessToken: false })
            {
                var baseUrlOverride = await httpClientFactory.CreateClient<string?>(
                    callContext.InstanceId,
                    HttpClientNames.CoreAPI,
                    callContext.CurrentUserIdentity!,
                    BuildClient);
                if (!string.IsNullOrWhiteSpace(baseUrlOverride))
                {
                    baseUrl = baseUrlOverride;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // Ignore the exception since we should always fall back to the configured value.
        }

        return baseUrl;
    }

    private static string? BuildClient(Dictionary<string, object> parameters) =>
        parameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString();

    private static string? ResolveContentDeepLinks(string? text, string rootUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }
        const string token = "{{fllm_base_url}}";
        // If rootUrl ends with a slash, remove it.
        if (rootUrl.EndsWith('/'))
        {
            rootUrl = rootUrl[..^1];
        }
        return text.Replace(token, rootUrl);
    }

    private static UnifiedUserIdentity ValidateUserIdentity(UnifiedUserIdentity? userIdentity)
    {
        if (userIdentity == null)
            throw new InvalidOperationException("The call context does not contain a valid user identity.");

        if (string.IsNullOrWhiteSpace(userIdentity.UPN))
            throw new InvalidOperationException("The user identity provided by the call context does not contain a valid UPN.");

        return userIdentity;
    }

    #endregion
}
