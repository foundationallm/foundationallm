using Azure;
using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models;
using FoundationaLLM.Orchestration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// Provides a service for managing the semantic cache.
    /// </summary>
    public class SemanticCacheService : ISemanticCacheService
    {
        private const string SEMANTIC_CACHE_CONTAINER_NAME = "CompletionsCache";

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
            AgentSemanticCacheSettings agentSettings)
        {
            try
            {
                await _syncLock.WaitAsync();

                if (_agentCaches.Count == 0)
                {
                    // This is the first time an agent attempts to initialize the cache.
                    // Ensure the proper container exists in Cosmos DB.

                    // For now we are skipping the dynamic creation of the container as it looks like
                    // there is a bug with the Cosmos DB client when working with RBAC.

                    //await _cosmosDBService.CreateVectorSearchContainerAsync(
                    //    SEMANTIC_CACHE_CONTAINER_NAME,
                    //    "/partitionKey",
                    //    "/userPromptEmbedding",
                    //    agentSettings.EmbeddingDimensions);
                }

                if (HasCacheForAgent(instanceId, agentName))
                {
                    _logger.LogWarning("Semantic cache for agent {AgentName} in instance {InstanceId} already exists.", agentName, instanceId);
                    return;
                }

                var embeddingAIModel = await _aiModelResourceProviderService.GetResourceAsync<AIModelBase>(
                                        agentSettings.EmbeddingAIModelObjectId,
                                        ServiceContext.ServiceIdentity!);
                var embeddingAPIEndpointConfiguration = await _configurationResourceProviderService.GetResourceAsync<APIEndpointConfiguration>(
                                        embeddingAIModel.EndpointObjectId!,
                                        ServiceContext.ServiceIdentity!);

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
        public async Task ResetCacheForAgent(string instanceId, string agentName) =>
            await Task.CompletedTask;

        /// <inheritdoc/>
        public async Task SetCompletionResponseInCache(string instanceId, string agentName, CompletionResponse completionResponse)
        {
            if (!_agentCaches.TryGetValue($"{instanceId}|{agentName}", out AgentSemanticCache? agentCache)
                || agentCache == null)
                throw new SemanticCacheException($"The semantic cache is not initialized for agent {agentName} in instance {instanceId}.");

            var cacheItem = new SemanticCacheItem
            {
                Id = Guid.NewGuid().ToString().ToLower(),
                OperationId = completionResponse.OperationId!,
                UserPrompt = completionResponse.UserPromptRewrite!,
                SerializedItem = JsonSerializer.Serialize(completionResponse),
            };

            var embeddingResponse = await agentCache.EmbeddingClient.GenerateEmbeddingAsync(
                cacheItem.UserPrompt,
                new EmbeddingGenerationOptions
                {
                    Dimensions = agentCache.Settings.EmbeddingDimensions
                });
            cacheItem.UserPromptEmbedding = embeddingResponse.Value.ToFloats().ToArray();

            await _cosmosDBService.UpsertItemAsync<SemanticCacheItem>(
                SEMANTIC_CACHE_CONTAINER_NAME,
                cacheItem.OperationId,
                cacheItem);
        }

        /// <inheritdoc/>
        public async Task<CompletionResponse?> GetCompletionResponseFromCache(
            string instanceId,
            string agentName,
            CompletionRequest completionRequest)
        {
            if (!_agentCaches.TryGetValue($"{instanceId}|{agentName}", out AgentSemanticCache? agentCache)
                || agentCache == null)
                throw new SemanticCacheException($"The semantic cache is not initialized for agent {agentName} in instance {instanceId}.");

            if (string.IsNullOrEmpty(completionRequest.UserPromptRewrite))
                return null;

            var embeddingResult = await agentCache.EmbeddingClient.GenerateEmbeddingAsync(
                completionRequest.UserPromptRewrite,
                new EmbeddingGenerationOptions
                {
                    Dimensions = agentCache.Settings.EmbeddingDimensions
                });
            var userPromptEmbedding = embeddingResult.Value.ToFloats();

            var cachedCompletionResponse = await _cosmosDBService.GetCompletionResponseAsync(
                SEMANTIC_CACHE_CONTAINER_NAME,
                userPromptEmbedding,
                agentCache.Settings.MinimumSimilarityThreshold);

            return cachedCompletionResponse;
        }

        private EmbeddingClient GetEmbeddingClient(string deploymentName, APIEndpointConfiguration apiEndpointConfiguration) =>
            apiEndpointConfiguration.AuthenticationType switch
            {
                AuthenticationTypes.AzureIdentity => (new AzureOpenAIClient(
                    new Uri(apiEndpointConfiguration.Url),
                    ServiceContext.AzureCredential))
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
