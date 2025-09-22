using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Context.Exceptions;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using System.ClientModel.Primitives;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="authorizationServiceClient">The client for the FoundationaLLM Authorization API.</param>
    /// <param name="agentResourceProvider">The FoundationaLLM.Agent resource provider.</param>
    /// <param name="contextResourceProvider"> The FoundationaLLM.Context resource provider service.</param>
    /// <param name="configurationResourceProvider">The FoundationaLLM.Configuration resource provider service.</param>
    /// <param name="vectorResourceProvider">The FoundationaLLM.Vector resource provider service.</param>
    /// <param name="httpClientFactory"> The factory for creating HTTP clients.</param>
    /// <param name="settings">The settings for the Knowledge Graph service.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class KnowledgeService(
        IStorageService storageService,
        IAuthorizationServiceClient authorizationServiceClient,
        IResourceProviderService agentResourceProvider,
        IResourceProviderService contextResourceProvider,
        IResourceProviderService configurationResourceProvider,
        IResourceProviderService vectorResourceProvider,
        IHttpClientFactoryService httpClientFactory,
        KnowledgeServiceSettings settings,
        ILoggerFactory loggerFactory) : IKnowledgeService
    {
        private readonly IStorageService _storageService = storageService;
        private readonly IAuthorizationServiceClient _authorizationServiceClient = authorizationServiceClient;
        private readonly IResourceProviderService _agentResourceProvider = agentResourceProvider;
        private readonly IResourceProviderService _contextResourceProvider = contextResourceProvider;
        private readonly IResourceProviderService _configurationResourceProvider = configurationResourceProvider;
        private readonly IResourceProviderService _vectorResourceProvider = vectorResourceProvider;
        private readonly IHttpClientFactoryService _httpClientFactory = httpClientFactory;
        private readonly KnowledgeServiceSettings _settings = settings;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<KnowledgeService> _logger = loggerFactory.CreateLogger<KnowledgeService>();

        private AzureOpenAIClient _azureOpenAIClient = null!;
        private readonly Dictionary<string, IAzureAISearchService> _azureAISearchServices = [];

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100, // Limit cache size to 100 units (knowledge graph instances).
            ExpirationScanFrequency = TimeSpan.FromSeconds(60) // How often to scan for expired items.
        });
        private readonly SemaphoreSlim _cacheLock = new(1, 1);

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName,
            UnifiedUserIdentity userIdentity)
        {
            // If the query is submitted on behalf of an agent, retrieve the agent resource.
            AgentBase agent = null!;
            if (agentName is not null)
                agent = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                    instanceId,
                    agentName,
                    userIdentity);

            var knowledgeUnit = await _contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                instanceId,
                knowledgeUnitId,
                userIdentity,
                parentResourceInstance: agent);
            return new ContextServiceResponse<ResourceProviderGetResult<KnowledgeUnit>>
            {
                Success = true,
                Result = new ResourceProviderGetResult<KnowledgeUnit>
                {
                    Resource = knowledgeUnit,
                    Actions = [],
                    Roles = []
                }
            };
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName,
            UnifiedUserIdentity userIdentity)
        {
            // If the query is submitted on behalf of an agent, retrieve the agent resource.
            AgentBase agent = null!;
            if (agentName is not null)
                agent = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                    instanceId,
                    agentName,
                    userIdentity);

            var knowledgeSource = await _contextResourceProvider.GetResourceAsync<KnowledgeSource>(
                instanceId,
                knowledgeSourceId,
                userIdentity,
                parentResourceInstance: agent);
            return new ContextServiceResponse<ResourceProviderGetResult<KnowledgeSource>>
            {
                Success = true,
                Result = new ResourceProviderGetResult<KnowledgeSource>
                {
                    Resource = knowledgeSource,
                    Actions = [],
                    Roles = []
                }
            };
        }

        public async Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>> GetKnowledgeUnits(
            string instanceId,
            ContextKnowledgeResourceListRequest listRequest,
            UnifiedUserIdentity userIdentity)
        {
            var knowledgeUnitResults = await _contextResourceProvider.GetResourcesAsync<KnowledgeUnit>(
                instanceId,
                userIdentity);
            return new ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>
            {
                Success = true,
                Result = listRequest.KnowledgeResourceNames is null
                    ? knowledgeUnitResults
                        .OrderBy(r => r.Resource.Name)
                    : knowledgeUnitResults
                        .Where(r => listRequest.KnowledgeResourceNames.Contains(r.Resource.Name))
                        .OrderBy(r => r.Resource.Name)
            };
        }

        /// <inheritdoc />
        public async Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>> GetKnowledgeSources(
            string instanceId,
            ContextKnowledgeResourceListRequest listRequest,
            UnifiedUserIdentity userIdentity)
        {
            var knowledgeSourceResults = await _contextResourceProvider.GetResourcesAsync<KnowledgeSource>(
                instanceId,
                userIdentity);
            return new ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>
            {
                Success = true,
                Result = listRequest.KnowledgeResourceNames is null
                    ? knowledgeSourceResults
                        .OrderBy(r => r.Resource.Name)
                    : knowledgeSourceResults
                        .Where(r => listRequest.KnowledgeResourceNames.Contains(r.Resource.Name))
                        .OrderBy(r => r.Resource.Name)
            };
        }

        /// <inheritdoc />
        public async Task<ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeUnit>>> UpsertKnowledgeUnit(
            string instanceId,
            KnowledgeUnit knowledgeUnit,
            UnifiedUserIdentity userIdentity)
        {
            var upsertResult =
                await _contextResourceProvider.UpsertResourceAsync<KnowledgeUnit, ResourceProviderUpsertResult<KnowledgeUnit>>(
                    instanceId,
                    knowledgeUnit,
                    userIdentity);
            return new ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeUnit>>
            {
                Success = true,
                Result = upsertResult
            };
        }

        /// <inheritdoc />
        public async Task<ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeSource>>> UpsertKnowledgeSource(
            string instanceId,
            KnowledgeSource knowledgeSource,
            UnifiedUserIdentity userIdentity)
        {
            var upsertResult =
                await _contextResourceProvider.UpsertResourceAsync<KnowledgeSource, ResourceProviderUpsertResult<KnowledgeSource>>(
                    instanceId,
                    knowledgeSource,
                    userIdentity);
            return new ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeSource>>
            {
                Success = true,
                Result = upsertResult
            };
        }

        /// <inheritdoc />
        public async Task<ContextServiceResponse<ResourceProviderActionResult>> SetKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeUnitSetGraphRequest setGraphRequest,
            UnifiedUserIdentity userIdentity)
        {
            var result = await _contextResourceProvider.ExecuteResourceActionAsync<
                KnowledgeUnit,
                ContextKnowledgeUnitSetGraphRequest,
                ResourceProviderActionResult>(
                instanceId,
                knowledgeUnitId,
                ResourceProviderActions.SetGraph,
                setGraphRequest,
                userIdentity);

            // If the knowledge source is cached, remove it from the cache.
            RemoveKnowledgeUnitFromCache(
                instanceId,
                knowledgeUnitId);

            return new ContextServiceResponse<ResourceProviderActionResult>
            {
                Success = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                Result = result
            };
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeSourceQueryResponse> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest queryRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                // If the query is submitted on behalf of an agent, retrieve the agent resource.
                AgentBase agent = null!;
                if (queryRequest.AgentObjectId is not null)
                    agent = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                        queryRequest.AgentObjectId,
                        userIdentity);

                var knowledgeSource = await _contextResourceProvider.GetResourceAsync<KnowledgeSource>(
                    instanceId,
                    knowledgeSourceId,
                    userIdentity,
                    parentResourceInstance: agent);

                var knowledgeUnitTasks = knowledgeSource.KnowledgeUnitObjectIds
                    .Select(knowledgeUnitObjectId =>
                        _contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                            knowledgeUnitObjectId,
                            userIdentity,
                            parentResourceInstance: agent))
                    .ToList();
                var knowledgeUnits = await Task.WhenAll(knowledgeUnitTasks);

                var vectorDatabaseTasks = knowledgeUnits
                    .Select(knowledgeUnit => _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                        knowledgeUnit.VectorDatabaseObjectId,
                        userIdentity,
                        parentResourceInstance: agent))
                    .ToList();
                var vectorDatabases = await Task.WhenAll(vectorDatabaseTasks);

                var knowledgeGraphVectorDatabaseTasks = knowledgeUnits
                    .Select(async knowledgeUnit =>
                        knowledgeUnit.HasKnowledgeGraph
                        ? await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                                    knowledgeUnit.KnowledgeGraphVectorDatabaseObjectId!,
                                    userIdentity,
                                    parentResourceInstance: agent)
                        : null)
                    .ToList();
                var knowledgeGraphVectorDatabases = await Task.WhenAll(knowledgeGraphVectorDatabaseTasks);

                var cachedKnowledgeUnitTasks = Enumerable.Range(0, knowledgeUnits.Length)
                    .Select(i => GetCachedKnowledgeUnit(
                        instanceId,
                        agent,
                        knowledgeUnits[i],
                        vectorDatabases[i],
                        knowledgeGraphVectorDatabases[i],
                        userIdentity))
                    .ToList();
                var cachedKnowledgeUnits = await Task.WhenAll(cachedKnowledgeUnitTasks);

                var knowledgeUnitQueryEngines = Enumerable.Range(0, knowledgeUnits.Length)
                    .Select(i => new KnowledgeUnitQueryEngine(
                        knowledgeUnits[i],
                        cachedKnowledgeUnits[i],
                        vectorDatabases[i],
                        knowledgeGraphVectorDatabases[i],
                        _loggerFactory.CreateLogger<KnowledgeUnitQueryEngine>()))
                    .ToList();

                var queryEngine = new KnowledgeSourceQueryEngine(
                    knowledgeSourceId,
                    knowledgeUnitQueryEngines,
                    _loggerFactory.CreateLogger<KnowledgeSourceQueryEngine>());

                var queryResponse = await queryEngine.QueryAsync(
                    queryRequest);

                return queryResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge source {KnowledgeSourceId} from instance {InstanceId}.",
                    knowledgeSourceId, instanceId);
                return new ContextKnowledgeSourceQueryResponse
                {
                    Source = knowledgeSourceId,
                    Success = false,
                    ErrorMessage = $"An error occurred while querying the knowledge source {knowledgeSourceId} from instance {instanceId}."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeUnitRenderGraphResponse> RenderKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeSourceQueryRequest? queryRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var knowledgeUnit = await _contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                    instanceId,
                    knowledgeUnitId,
                    userIdentity);

                var vectorStoreFilter = queryRequest?.KnowledgeUnitVectorStoreFilters
                    .SingleOrDefault(f => f.KnowledgeUnitId == knowledgeUnitId);

                if (!knowledgeUnit.HasKnowledgeGraph)
                    return new ContextKnowledgeUnitRenderGraphResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge unit {knowledgeUnitId} from instance {instanceId} does not contain a knowledge graph."
                    };

                var vectorStoreId = string.IsNullOrWhiteSpace(knowledgeUnit.VectorStoreId)
                    ? vectorStoreFilter?.VectorStoreId
                    : knowledgeUnit.VectorStoreId;
                if (string.IsNullOrWhiteSpace(vectorStoreId))
                    return new ContextKnowledgeUnitRenderGraphResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge unit {knowledgeUnitId} from instance {instanceId} does not have a vector store identifier specified and none was provided in the rendering request."
                    };

                var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                    knowledgeUnit.VectorDatabaseObjectId,
                    userIdentity);

                var knowledgeGraphVectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                    knowledgeUnit.KnowledgeGraphVectorDatabaseObjectId!,
                    userIdentity);

                var cachedKnowledgeUnit = await GetCachedKnowledgeUnit(
                    instanceId,
                    null,
                    knowledgeUnit,
                    vectorDatabase,
                    knowledgeGraphVectorDatabase,
                    userIdentity);

                var renderResponse = new ContextKnowledgeUnitRenderGraphResponse
                {
                    Success = true
                };

                if (queryRequest is null)
                {
                    renderResponse.Nodes = [.. cachedKnowledgeUnit.KnowledgeGraph!.Graph.Entities
                        .Select(e => new KnowledgeGraphRenderingNode
                        {
                            Id = e.UniqueId,
                            Label = e.Name,
                        })];
                    renderResponse.Edges = [.. cachedKnowledgeUnit.KnowledgeGraph!.Graph.Relationships
                        .Where(r => cachedKnowledgeUnit.KnowledgeGraph.Index.Nodes.ContainsKey(r.SourceUniqueId)
                                    && cachedKnowledgeUnit.KnowledgeGraph.Index.Nodes.ContainsKey(r.TargetUniqueId))
                        .Select(r => new List<string> { r.SourceUniqueId, r.TargetUniqueId })];
                }
                else
                {
                    // TODO: Execute the query against the knowledge graph and render the result.
                }

                return renderResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering the knowledge graph for knowledge unit {KnowledgeUnitId} from instance {InstanceId}.",
                    knowledgeUnitId, instanceId);
                return new ContextKnowledgeUnitRenderGraphResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while rendering the knowledge graph for knowledge unit {knowledgeUnitId} from instance {instanceId}."
                };
            }
        }

        #region Utils

        private async Task<CachedKnowledgeUnit> GetCachedKnowledgeUnit(
            string instanceId,
            AgentBase? agent,
            KnowledgeUnit knowledgeUnit,
            VectorDatabase vectorDatabase,
            VectorDatabase? knowledgeGraphVectorDatabase,
            UnifiedUserIdentity userIdentity)
        {
            var cachedKnowledgeUnit = GetKnowledgeUnitFromCache(
                instanceId,
                knowledgeUnit.Name);
            if (cachedKnowledgeUnit is not null)
                return cachedKnowledgeUnit;

            // Add a new cached knowledge unit to the cache.

            cachedKnowledgeUnit = new CachedKnowledgeUnit
            {
                SearchService = await GetAzureAISearchService(
                    instanceId,
                    vectorDatabase),
                EmbeddingClient = await GetEmbeddingClient(
                    instanceId,
                    _settings.Embedding.ModelDeployments[vectorDatabase.EmbeddingModel]),
            };

            if (knowledgeUnit.HasKnowledgeGraph)
            {
                if (knowledgeGraphVectorDatabase is not null)
                {
                    cachedKnowledgeUnit.KnowledgeGraphSearchService = await GetAzureAISearchService(
                        instanceId,
                        knowledgeGraphVectorDatabase);
                    cachedKnowledgeUnit.KnowledgeGraphEmbeddingClient = await GetEmbeddingClient(
                        instanceId,
                        _settings.Embedding.ModelDeployments[knowledgeGraphVectorDatabase.EmbeddingModel]);
                }

                var actionResult = await _contextResourceProvider.ExecuteResourceActionAsync<
                    KnowledgeUnit,
                    object,
                    ResourceProviderActionResult<KnowledgeGraph>>(
                    instanceId,
                    knowledgeUnit.Name,
                    ResourceProviderActions.LoadGraph,
                    null!,
                    userIdentity,
                    parentResourceInstance: agent);

                if (!actionResult.IsSuccess)
                {
                    _logger.LogError(
                        "Failed to load knowledge graph for knowledge unit {KnowledgeUnitId}: {ErrorMessage}",
                        knowledgeUnit.Name, actionResult.ErrorMessage ?? "N/A");
                    throw new ContextServiceException(
                        $"Failed to load knowledge graph for knowledge unit {knowledgeUnit.Name}: {actionResult.ErrorMessage ?? "N/A"}");
                }

                cachedKnowledgeUnit.KnowledgeGraph = new IndexedKnowledgeGraph
                {
                    Graph = actionResult.Resource!,
                    Index = KnowledgeGraphIndex.Create(
                        actionResult.Resource!.Entities,
                        actionResult.Resource!.Relationships)
                };
            }

            SetKnowledgeUnitInCache(
                instanceId,
                knowledgeUnit.Name,
                cachedKnowledgeUnit);

            return cachedKnowledgeUnit;
        }

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // Cache entries are valid for 60 minutes.
           .SetSlidingExpiration(TimeSpan.FromSeconds(1800)) // Reset expiration time if accessed within 30 minutes.
           .SetSize(1); // Each cache entry is a single knowledge unit instance.

        private string GetCacheKey(string instanceId, string knowledgeUnitId) =>
            $"{instanceId}:{knowledgeUnitId}";

        private void SetKnowledgeUnitInCache(
            string instanceId,
            string knowledgeUnitId,
            CachedKnowledgeUnit cachedKnowledgeUnit)
        {
            var cacheKey = GetCacheKey(instanceId, knowledgeUnitId);
            _cache.Set(
                cacheKey,
                cachedKnowledgeUnit,
                GetMemoryCacheEntryOptions());
        }

        private void RemoveKnowledgeUnitFromCache(
            string instanceId,
            string knowledgeUnitId)
        {
            var cacheKey = GetCacheKey(instanceId, knowledgeUnitId);
            if (_cache.TryGetValue(cacheKey, out _))
                _cache.Remove(cacheKey);
        }

        private CachedKnowledgeUnit? GetKnowledgeUnitFromCache(
            string instanceId,
            string knowledgeUnitId)
        {
            var cacheKey = GetCacheKey(instanceId, knowledgeUnitId);
            if (_cache.TryGetValue(cacheKey, out CachedKnowledgeUnit? cachedKnowledgeUnit))
                return cachedKnowledgeUnit;
            return null;
        }

        private async Task<EmbeddingClient> GetEmbeddingClient(
            string instanceId,
            string modelDeploymentName)
        {
            _azureOpenAIClient ??= await GetAzureOpenAIClient(instanceId);
            return _azureOpenAIClient.GetEmbeddingClient(modelDeploymentName);
        }

        private async Task<IAzureAISearchService> GetAzureAISearchService(
            string instanceId,
            VectorDatabase vectorDatabase)
        {
            if (!_azureAISearchServices.TryGetValue(vectorDatabase.APIEndpointConfigurationObjectId, out var azureAISearchService))
            {
                var searchIndexClient = await _httpClientFactory.CreateClient<SearchIndexClient>(
                    instanceId,
                    ResourcePath.GetResourcePath(
                        vectorDatabase.APIEndpointConfigurationObjectId)
                        .ResourceId!,
                    ServiceContext.ServiceIdentity!,
                    AzureAISearchService.CreateSearchIndexClient);
                    //new Dictionary<string, object>()
                    //{
                    //    { HttpClientFactoryServiceKeyNames.EnableDiagnostics, true }
                    //});
                azureAISearchService = new AzureAISearchService(
                    searchIndexClient,
                    _loggerFactory.CreateLogger<AzureAISearchService>());
                _azureAISearchServices[vectorDatabase.ObjectId!] = azureAISearchService;
            }

            return azureAISearchService;
        }

        private async Task<AzureOpenAIClient> GetAzureOpenAIClient(
            string instanceId) =>
            await _httpClientFactory.CreateClient<AzureOpenAIClient>(
                instanceId,
                ResourcePath.GetResourcePath(
                    _settings.Embedding.EmbeddingAPIEndpointConfigurationObjectId)
                    .ResourceId!,
                ServiceContext.ServiceIdentity!,
                BuildAzureOpenAIClient);

        private AzureOpenAIClient BuildAzureOpenAIClient(Dictionary<string, object> parameters)
        {
            AzureOpenAIClient client = null!;
            try
            {
                var endpoint = parameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString();
                var authenticationType = (AuthenticationTypes)parameters[HttpClientFactoryServiceKeyNames.AuthenticationType];
                switch (authenticationType)
                {
                    case AuthenticationTypes.AzureIdentity:
                        client = new AzureOpenAIClient(
                            new Uri(endpoint!),
                            ServiceContext.AzureCredential,
                            new AzureOpenAIClientOptions
                            {
                                NetworkTimeout = TimeSpan.FromSeconds(120),
                                RetryPolicy = new ClientRetryPolicy(0)
                            });
                        break;
                    case AuthenticationTypes.APIKey:
                        var apiKey = parameters[HttpClientFactoryServiceKeyNames.APIKey].ToString();
                        client = new AzureOpenAIClient(
                            new Uri(endpoint!),
                            new AzureKeyCredential(apiKey!),
                            new AzureOpenAIClientOptions
                            {
                                NetworkTimeout = TimeSpan.FromSeconds(120),
                                RetryPolicy = new ClientRetryPolicy(0)
                            });
                        break;
                    default:
                        throw new ConfigurationValueException($"The {authenticationType} authentication type is not supported by the Azure Event Grid events service.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error creating the Azure OpenAI client.");
            }

            return client;
        }

        #endregion
    }
}
