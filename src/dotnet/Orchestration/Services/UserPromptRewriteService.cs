using Azure;
using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Constants.Telemetry;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Telemetry;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Diagnostics;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// Provides a service for managing the semantic cache.
    /// </summary>
    public class UserPromptRewriteService : IUserPromptRewriteService
    {
        private readonly Dictionary<string, AgentUserPromptRewriter> _agentRewriters = [];
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        private readonly IResourceProviderService _aiModelResourceProviderService;
        private readonly IResourceProviderService _configurationResourceProviderService;
        private readonly IResourceProviderService _promptResourceProviderService;
        private readonly ITemplatingService _templatingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserPromptRewriteService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPromptRewriteService"/> class.
        /// </summary>
        /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="templatingService">The templating service used for replacing variables in prompts.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="logger">The logger used for logging..</param>
        public UserPromptRewriteService(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            ITemplatingService templatingService,
            IConfiguration configuration,
            ILogger<UserPromptRewriteService> logger)
        {
            _aiModelResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_AIModel);
            _configurationResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_Configuration);
            _promptResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_Prompt);
            _templatingService = templatingService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public bool HasUserPromptRewriterForAgent(string instanceId, string agentName) =>
            _agentRewriters.ContainsKey($"{instanceId}|{agentName}");

        /// <inheritdoc/>
        public async Task InitializeUserPromptRewriterForAgent(
            string instanceId,
            string agentName,
            AgentUserPromptRewriteSettings agentSettings)
        {
            try
            {
                await _syncLock.WaitAsync();

                if (HasUserPromptRewriterForAgent(instanceId, agentName))
                {
                    _logger.LogWarning("A user prompt rewriter for agent {AgentName} in instance {InstanceId} already exists.", agentName, instanceId);
                    return;
                }

                var userPromptRewriteAIModel = await _aiModelResourceProviderService.GetResourceAsync<AIModelBase>(
                    agentSettings.UserPromptRewriteAIModelObjectId,
                    ServiceContext.ServiceIdentity!);
                var userPromptRewriteAPIEndpointConfiguration = await _configurationResourceProviderService.GetResourceAsync<APIEndpointConfiguration>(
                    userPromptRewriteAIModel.EndpointObjectId!,
                    ServiceContext.ServiceIdentity!);
                var userPromptRewritePrompt = await _promptResourceProviderService.GetResourceAsync<PromptBase>(
                    agentSettings.UserPromptRewritePromptObjectId,
                    ServiceContext.ServiceIdentity!);

                _agentRewriters[$"{instanceId}|{agentName}"] = new AgentUserPromptRewriter
                {
                    Settings = agentSettings,
                    RewriterSystemPrompt = (userPromptRewritePrompt as MultipartPrompt)!.Prefix!,
                    ChatClient = GetChatClient(
                        userPromptRewriteAIModel.DeploymentName!,
                        userPromptRewriteAPIEndpointConfiguration)
                };
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task RewriteUserPrompt(
            string instanceId,
            string agentName,
            CompletionRequest completionRequest)
        {
            if (!_agentRewriters.TryGetValue($"{instanceId}|{agentName}", out AgentUserPromptRewriter? agentRewriter)
                || agentRewriter == null)
                throw new UserPromptRewriteException($"The user prompt rewriter is not initialized for agent {agentName} in instance {instanceId}.");

            //No need to rewrite a single message.
            if (completionRequest.MessageHistory?.Count == 0)
            {
                completionRequest.UserPromptRewrite = completionRequest.UserPrompt;
                return;
            }

            try
            {
                var messages = completionRequest.MessageHistory?
                    .TakeLast(agentRewriter.Settings.UserPromptsWindowSize * 2)
                    .Select<MessageHistoryItem, ChatMessage>(m => m.Sender switch
                    {
                        nameof(Participants.User) => new UserChatMessage(m.TextRewrite ?? m.Text),
                        nameof(Participants.Agent) => new AssistantChatMessage(m.Text),
                        _ => throw new OrchestrationException($"Unknown message sender {m.Sender}.")
                    })
                    .ToList()
                    ?? [];
                messages.Insert(0, new SystemChatMessage(
                    _templatingService.Transform(agentRewriter.RewriterSystemPrompt)));
                messages.Add(new UserChatMessage(completionRequest.UserPrompt));

                using var telemetryActivity = TelemetryActivitySources.OrchestrationAPIActivitySource.StartActivity(
                    TelemetryActivityNames.OrchestrationAPI_AgentOrchestration_UserPromptRewrite_LLM,
                    ActivityKind.Internal,
                    parentContext: default,
                    tags: new Dictionary<string, object?>
                    {
                        { TelemetryActivityTagNames.InstanceId, instanceId },
                        { TelemetryActivityTagNames.OperationId, completionRequest.OperationId },
                        { TelemetryActivityTagNames.ConversationId, completionRequest.SessionId ?? "N/A" }
                    });

                var completionResult = await agentRewriter.ChatClient.CompleteChatAsync(
                    messages,
                    new ChatCompletionOptions
                    {
                        Temperature = 0,
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                        Seed = 42
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    });

                completionRequest.UserPromptRewrite = completionResult.Value.Content[0].Text;

            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while rewriting the user prompt {UserPrompt} for agent {AgentName} in instance {InstanceId}.",
                    completionRequest.UserPrompt,
                    agentName,
                    instanceId);

                completionRequest.UserPromptRewrite = completionRequest.UserPrompt;
            }
        }

        private ChatClient GetChatClient(string deploymentName, APIEndpointConfiguration apiEndpointConfiguration) =>
            apiEndpointConfiguration.AuthenticationType switch
            {
                AuthenticationTypes.AzureIdentity => (new AzureOpenAIClient(
                    new Uri(apiEndpointConfiguration.Url),
                    ServiceContext.AzureCredential))
                    .GetChatClient(deploymentName),
                AuthenticationTypes.APIKey => (new AzureOpenAIClient(
                    new Uri(apiEndpointConfiguration.Url),
                    new AzureKeyCredential(GetAPIKey(apiEndpointConfiguration))))
                    .GetChatClient(deploymentName),
                _ => throw new NotImplementedException($"API endpoint authentication type {apiEndpointConfiguration.AuthenticationType} is not supported.")
            };

        private string GetAPIKey(APIEndpointConfiguration apiEndpointConfiguration)
        {
            if (!apiEndpointConfiguration.AuthenticationParameters.TryGetValue(
                       AuthenticationParametersKeys.APIKeyConfigurationName, out var apiKeyConfigurationNameObj))
                throw new SemanticCacheException($"The {AuthenticationParametersKeys.APIKeyConfigurationName} key is missing from the endpoint's authentication parameters dictionary.");

            var apiKey = _configuration.GetValue<string>(apiKeyConfigurationNameObj?.ToString()!)!;

            return apiKey;
        }
    }
}
