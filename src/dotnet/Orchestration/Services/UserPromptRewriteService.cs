using Azure;
using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Embeddings;

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
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserPromptRewriteService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticCacheService"/> class.
        /// </summary>
        /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="logger">The logger used for logging..</param>
        public UserPromptRewriteService(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IConfiguration configuration,
            ILogger<UserPromptRewriteService> logger)
        {
            _aiModelResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_AIModel);
            _configurationResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_Configuration);
            _promptResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_Prompt);
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
                    DefaultAuthentication.ServiceIdentity!);
                var userPromptRewriteAPIEndpointConfiguration = await _configurationResourceProviderService.GetResourceAsync<APIEndpointConfiguration>(
                    userPromptRewriteAIModel.EndpointObjectId!,
                    DefaultAuthentication.ServiceIdentity!);
                var userPromptRewritePrompt = await _promptResourceProviderService.GetResourceAsync<PromptBase>(
                    agentSettings.UserPromptRewritePromptObjectId,
                    DefaultAuthentication.ServiceIdentity!);

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

            try
            {
                var userPromptsHistory = completionRequest.MessageHistory?
                    .Where(x => StringComparer.Ordinal.Equals(x.Sender, nameof(Participants.User)))
                    .Select(x => x.Text)
                    .TakeLast(agentRewriter.Settings.UserPromptsWindowSize)
                    .ToList()
                    ?? [];
                userPromptsHistory.Add(completionRequest.UserPrompt);
                var completionResult = await agentRewriter.ChatClient.CompleteChatAsync(
                    [
                        new SystemChatMessage(agentRewriter.RewriterSystemPrompt),
                        new UserChatMessage($"QUESTIONS:{Environment.NewLine}{string.Join(Environment.NewLine, [.. userPromptsHistory])}")
                    ]);

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
                    DefaultAuthentication.AzureCredential))
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
