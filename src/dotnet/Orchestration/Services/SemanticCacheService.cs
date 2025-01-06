using Azure;
using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models;
using FoundationaLLM.Orchestration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// Provides a service for managing the semantic cache.
    /// </summary>
    public class SemanticCacheService : ISemanticCacheService
    {
        private readonly Dictionary<string, AgentSemanticCache> _agentCaches = [];
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        private readonly IAzureCosmosDBService _cosmosDBService;
        private readonly IResourceProviderService _aiModelResourceProviderService;
        private readonly IResourceProviderService _configurationResourceProviderService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SemanticCacheService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticCacheService"/> class.
        /// </summary>
        /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> service providing access to the Cosmos DB vector store.</param>
        /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="logger">The logger used for logging..</param>
        public SemanticCacheService(
            IAzureCosmosDBService cosmosDBService,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IConfiguration configuration,
            ILogger<SemanticCacheService> logger)
        {
            _cosmosDBService = cosmosDBService;
            _aiModelResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_AIModel);
            _configurationResourceProviderService = resourceProviderServices
                .Single(x => x.Name == ResourceProviderNames.FoundationaLLM_Configuration);
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public bool HasCacheForAgent(string instanceId, string agentName) =>
            _agentCaches.ContainsKey($"{instanceId}|{agentName}");

        /// <inheritdoc/>
        public async Task InitializeCacheForAgent(
            string instanceId,
            string agentName,
            AgentSemanticCacheSettings agentSettings,
            UnifiedUserIdentity currentUserIdentity)
        {
            try
            {
                await _syncLock.WaitAsync();

                if (_agentCaches.Count == 0)
                {
                    // This is the first time an agent attempts to initialize the cache.
                    // TODO: Ensure the necessary Cosmos DB collections are created.
                }

                if (HasCacheForAgent(instanceId, agentName))
                {
                    _logger.LogWarning("Semantic cache for agent {AgentName} in instance {InstanceId} already exists.", agentName, instanceId);
                    return;
                }

                var embeddingAIModel = await _aiModelResourceProviderService.GetResourceAsync<AIModelBase>(
                                        agentSettings.EmbeddingAIModelObjectId,
                                        currentUserIdentity);
                var embeddingAPIEndpointConfiguration = await _configurationResourceProviderService.GetResourceAsync<APIEndpointConfiguration>(
                                        embeddingAIModel.EndpointObjectId!,
                                        currentUserIdentity);

                _agentCaches[$"{instanceId}|{agentName}"] = new AgentSemanticCache
                {
                    Settings = agentSettings,
                    EmbeddingClient = GetEmbeddingClient(embeddingAIModel.DeploymentName!, embeddingAPIEndpointConfiguration)
                };
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <inheritdoc/>
        public Task ResetCacheForAgent(string instanceId, string agentName) =>
            Task.CompletedTask;

        /// <inheritdoc/>
        public Task SetCacheItem(string instanceId, string agentName, SemanticCacheItem cacheItem) =>
            Task.CompletedTask;

        /// <inheritdoc/>
        public async Task<SemanticCacheItem?> GetCacheItem(
            string instanceId,
            string agentName,
            string userPrompt,
            List<string> messageHistory)
        {
            if (!_agentCaches.TryGetValue($"{instanceId}-{agentName}", out AgentSemanticCache? agentCache)
                || agentCache == null)
                throw new SemanticCacheException($"The semantic cache is not initialized for agent {agentName} in instance {instanceId}.");

            var userPromptEmbedding = await agentCache.EmbeddingClient.GenerateEmbeddingAsync(
                userPrompt,
                new EmbeddingGenerationOptions
                {
                    Dimensions = agentCache.Settings.EmbeddingDimensions
                });

            return null;
        }

        private EmbeddingClient GetEmbeddingClient(string deploymentName, APIEndpointConfiguration apiEndpointConfiguration) =>
            apiEndpointConfiguration.AuthenticationType switch
            {
                AuthenticationTypes.AzureIdentity => (new AzureOpenAIClient(
                    new Uri(apiEndpointConfiguration.Url),
                    DefaultAuthentication.AzureCredential))
                    .GetEmbeddingClient(deploymentName),
                AuthenticationTypes.APIKey => (new AzureOpenAIClient(
                    new Uri(apiEndpointConfiguration.Url),
                    new AzureKeyCredential(GetAPIKey(apiEndpointConfiguration))))
                    .GetEmbeddingClient(deploymentName),
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
