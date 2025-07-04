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
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models;
using FoundationaLLM.Context.Models.Configuration;
using MathNet.Numerics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using Parquet.Serialization;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="authorizationServiceClient">The client for the FoundationaLLM Authorization API.</param>
    /// <param name="configurationResourceProvider">The FoundationaLLM.Configuration resource provider service.</param>
    /// <param name="vectorResourceProvider">The FoundationaLLM.Vector resource provider service.</param>
    /// <param name="httpClientFactory"> The factory for creating HTTP clients.</param>
    /// <param name="settings">The settings for the Knowledge Graph service.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class KnowledgeGraphService(
        IStorageService storageService,
        IAuthorizationServiceClient authorizationServiceClient,
        IResourceProviderService configurationResourceProvider,
        IResourceProviderService vectorResourceProvider,
        IHttpClientFactoryService httpClientFactory,
        KnowledgeGraphServiceSettings settings,
        ILoggerFactory loggerFactory) : IKnowledgeGraphService
    {
        private readonly IStorageService _storageService = storageService;
        private readonly IAuthorizationServiceClient _authorizationServiceClient = authorizationServiceClient;
        private readonly IResourceProviderService _configurationResourceProvider = configurationResourceProvider;
        private readonly IResourceProviderService _vectorResourceProvider = vectorResourceProvider;
        private readonly IHttpClientFactoryService _httpClientFactory = httpClientFactory;
        private readonly KnowledgeGraphServiceSettings _settings = settings;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<KnowledgeGraphService> _logger = loggerFactory.CreateLogger<KnowledgeGraphService>();

        private AzureOpenAIClient _azureOpenAIClient = null!;
        private readonly Dictionary<string, IAzureAISearchService> _azureAISearchServices = [];

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100, // Limit cache size to 100 units (knowledge graph instances).
            ExpirationScanFrequency = TimeSpan.FromSeconds(60) // How often to scan for expired items.
        });

        private const string KNOWLEDGE_GRAPH_ROOT_PATH = "knowledge-graph";
        private const string KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME = "entities.parquet";
        private const string KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME = "relationships.parquet";
        private const string KEY_FIELD_NAME = "Id";

        /// <inheritdoc />
        public async Task UpdateKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            ContextKnowledgeGraphUpdateRequest updateRequest,
            UnifiedUserIdentity userIdentity)
        {
            await _storageService.CopyFileAsync(
                instanceId,
                updateRequest.EntitiesSourceFilePath,
                string.Join('/',
                    [
                        KNOWLEDGE_GRAPH_ROOT_PATH,
                        knowledgeGraphId,
                        KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                    ]));

            await _storageService.CopyFileAsync(
                instanceId,
                updateRequest.RelationshipsSourceFilePath,
                string.Join('/',
                    [
                        KNOWLEDGE_GRAPH_ROOT_PATH,
                        knowledgeGraphId,
                        KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                    ]));

            await UpdateKnowledgeGraphProperties(
                instanceId,
                knowledgeGraphId,
                updateRequest,
                userIdentity);

            // If the knowledge graph is cached, remove it from the cache.
            var cacheKey = GetCacheKey(instanceId, knowledgeGraphId);
            if (_cache.TryGetValue(cacheKey, out _))
                _cache.Remove(cacheKey);
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeGraphQueryResponse> QueryKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            ContextKnowledgeGraphQueryRequest queryRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var graph = await GetKnowledgeGraphFromCache(
                    instanceId,
                    knowledgeGraphId);

                var userPromptEmbedding = await graph.EmbeddingClient.GenerateEmbeddingAsync(
                    queryRequest.UserPrompt,
                    new EmbeddingGenerationOptions
                    {
                        Dimensions = graph.KnowledgeGraph.EmbeddingDimensions
                    });

                var matchingEntities = GetMatchingKnowledgeGraphEntities(
                    graph,
                    userPromptEmbedding.Value.ToFloats(),
                    queryRequest.MappedEntitiesSimilarityThreshold,
                    queryRequest.MappedEntitiesMaxCount,
                    queryRequest.RelationshipsMaxDepth,
                    queryRequest.AllEntitiesMaxCount);

                var matchingDocumentsFilter = string.Join(" and ",
                    [
                        $"{graph.VectorDatabase.VectorStoreIdPropertyName} eq '{graph.KnowledgeGraph.VectorStoreId}'",
                        $"search.in({KEY_FIELD_NAME}, '{string.Join(',', matchingEntities.SelectMany(me => me.Entity.ChunkIds))}')"
                    ]);

                var matchingDocuments = await graph.SearchService.SearchDocuments(
                    graph.VectorDatabase.DatabaseName,
                    [
                        KEY_FIELD_NAME,
                        graph.VectorDatabase.ContentPropertyName,
                        graph.VectorDatabase.MetadataPropertyName
                    ],
                    matchingDocumentsFilter,
                    queryRequest.UserPrompt!,
                    userPromptEmbedding.Value.ToFloats(),
                    graph.VectorDatabase.EmbeddingPropertyName,
                    queryRequest.TextChunksSimilarityThreshold,
                    queryRequest.TextChunksMaxCount,
                    queryRequest.UseSemanticRanking);

                return new ContextKnowledgeGraphQueryResponse
                {
                    Success = true,
                    Entities = [.. matchingEntities
                        .Select(me => new KnowledgeEntity
                        {
                            Type = me.Entity.Type,
                            Name = me.Entity.Name,
                            SummaryDescription = me.Entity.SummaryDescription
                        })
                    ],
                    TextChunks = [.. matchingDocuments
                        .Select(md => new ContextTextChunk
                        {
                            Content = md.GetString(graph.VectorDatabase.ContentPropertyName),
                            Metadata = md.GetObject(graph.VectorDatabase.MetadataPropertyName).ToDictionary()
                        })
                    ]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge graph {KnowledgeGraphId} for instance {InstanceId}.",
                    knowledgeGraphId, instanceId);
                return new ContextKnowledgeGraphQueryResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while querying the knowledge graph '{knowledgeGraphId}' for instance '{instanceId}'."
                };
            }
        }

        private async Task UpdateKnowledgeGraphProperties(
            string instanceId,
            string knowledgeGraphId,
            ContextKnowledgeGraphUpdateRequest updateRequest,
            UnifiedUserIdentity userIdentity)
        {
            var knowledgeGraph = default(KnowledgeGraph);
            var knowledgeGraphFilePath = string.Join('/',
                [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    $"{knowledgeGraphId}.json"
                ]);

            if (!await _storageService.FileExistsAsync(
                instanceId,
                knowledgeGraphFilePath,
                default))
            {
                // If the knowledge graph properties file does not exist, create it with default values.
                knowledgeGraph = new KnowledgeGraph
                {
                    Type = "knowledge-graph",
                    Name = knowledgeGraphId,
                    ObjectId = $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_ContextAPI}/knowledgeGraphs/{knowledgeGraphId}",
                    EmbeddingModel = updateRequest.EmbeddingModel,
                    EmbeddingDimensions = updateRequest.EmbeddingDimensions,
                    VectorDatabaseObjectId = updateRequest.VectorDatabaseObjectId,
                    VectorStoreId = updateRequest.VectorStoreId,
                    CreatedBy = userIdentity.UPN,
                    CreatedOn = DateTimeOffset.UtcNow
                };
            }
            else
            {
                var knowledgeGraphContent = await _storageService.ReadFileAsync(
                    instanceId,
                    knowledgeGraphFilePath,
                    default);
                knowledgeGraph = JsonSerializer.Deserialize<KnowledgeGraph>(knowledgeGraphContent);

                knowledgeGraph!.EmbeddingModel = updateRequest.EmbeddingModel;
                knowledgeGraph.EmbeddingDimensions = updateRequest.EmbeddingDimensions;
                knowledgeGraph.VectorDatabaseObjectId = updateRequest.VectorDatabaseObjectId;
                knowledgeGraph.VectorStoreId = updateRequest.VectorStoreId;
                knowledgeGraph.UpdatedBy = userIdentity.UPN;
                knowledgeGraph.UpdatedOn = DateTimeOffset.UtcNow;
            }

            await _storageService.WriteFileAsync(
                    instanceId,
                    knowledgeGraphFilePath,
                    JsonSerializer.Serialize(knowledgeGraph),
                    "application/json",
                    default);
        }

        private async Task<CachedKnowledgeGraph> GetKnowledgeGraphFromCache(
            string instanceId,
            string knowledgeGraphId)
        {
            var cacheKey = GetCacheKey(instanceId, knowledgeGraphId);

            if (!_cache.TryGetValue(cacheKey, out CachedKnowledgeGraph? knowledgeGraph))
            {
                var knowledgeGraphPropertiesFilePath = string.Join('/',
                [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    $"{knowledgeGraphId}.json"
                ]);

                var knowledgeGraphBinaryContent = await _storageService.ReadFileAsync(
                    instanceId,
                    knowledgeGraphPropertiesFilePath,
                    default);
                var knowledgeGraphProperties =
                    JsonSerializer.Deserialize<KnowledgeGraph>(knowledgeGraphBinaryContent);

                var entitiesBinaryContent = await _storageService.ReadFileAsync(
                    instanceId,
                    string.Join('/',
                        [
                            KNOWLEDGE_GRAPH_ROOT_PATH,
                            knowledgeGraphId,
                            KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                        ]),
                    default);

                var entities = await ParquetSerializer.DeserializeAsync<KnowledgeEntity>(
                    entitiesBinaryContent.ToStream());

                var relationshipsBinaryContent = await _storageService.ReadFileAsync(
                    instanceId,
                    string.Join('/',
                        [
                            KNOWLEDGE_GRAPH_ROOT_PATH,
                        knowledgeGraphId,
                        KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                        ]),
                    default);

                var relationships = await ParquetSerializer.DeserializeAsync<KnowledgeRelationship>(
                    relationshipsBinaryContent.ToStream());

                var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                    knowledgeGraphProperties!.VectorDatabaseObjectId,
                    ServiceContext.ServiceIdentity!);

                knowledgeGraph = new CachedKnowledgeGraph
                {
                    Entities = [.. entities],
                    Relationships = [.. relationships],
                    KnowledgeGraph = knowledgeGraphProperties!,
                    VectorDatabase = vectorDatabase,
                    SearchService = await GetAzureAISearchService(
                        vectorDatabase),
                    EmbeddingClient = await GetEmbeddingClient(
                        _settings.Embedding.ModelDeployments[knowledgeGraphProperties.EmbeddingModel]),
                    Index = KnowledgeGraphIndex.Create(
                        entities,
                        relationships)
                };

                // Store the knowledge graph in the cache.
                _cache.Set(
                    cacheKey,
                    knowledgeGraph,
                    GetMemoryCacheEntryOptions());
            }

            return knowledgeGraph!;
        }

        private List<(KnowledgeEntity Entity, float SimilarityScore)> GetMatchingKnowledgeGraphEntities(
            CachedKnowledgeGraph knowledgeGraph,
            ReadOnlyMemory<float> userPromptEmbedding,
            float similarityThreshold,
            int matchedEntitiesMaxCount,
            int relationshipsMaxDepth,
            int allEntitiesMaxCount)
        {
            var matchingEntities = new List<(KnowledgeEntity Entity, float SimilarityScore)>();
            var maxSimilarity = 0f;
            var minSimilarity = 1f;
            foreach (var entity in knowledgeGraph.Entities)
            {
                var similarity = 1.0f - Distance.Cosine(
                    userPromptEmbedding.ToArray(),
                    entity.SummaryDescriptionEmbedding);
                if (similarity >= similarityThreshold)
                {
                    matchingEntities.Add((entity, similarity));
                }

                if (similarity > maxSimilarity)
                    maxSimilarity = similarity;
                if (similarity < minSimilarity)
                    minSimilarity = similarity;
            }

            _logger.LogInformation("The minimum and maximum similarity values are {MinSimilarity} and {MaxSimilarity}", minSimilarity, maxSimilarity);

            matchingEntities = [.. matchingEntities
                .OrderByDescending(e => e.SimilarityScore)
                .Take(matchedEntitiesMaxCount)];

            var remainingEntitiesCount = allEntitiesMaxCount >= matchedEntitiesMaxCount
                ? allEntitiesMaxCount - matchedEntitiesMaxCount
                : 0;
            var relationships = new List<KnowledgeRelationship>();
            var relationShipEntities = new List<KnowledgeEntity>();



            return [.. matchingEntities
                .OrderByDescending(e => e.SimilarityScore)
                .Take(matchedEntitiesMaxCount)];
        }

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // Cache entries are valid for 60 minutes.
           .SetSlidingExpiration(TimeSpan.FromSeconds(1800)) // Reset expiration time if accessed within 30 minutes.
           .SetSize(1); // Each cache entry is a single knowledge graph instance.

        private string GetCacheKey(string instanceId, string knowledgeGraphId) =>
            $"{instanceId}:{knowledgeGraphId}";

        private async Task<EmbeddingClient> GetEmbeddingClient(
            string modelDeploymentName)
        {
            _azureOpenAIClient ??= await GetAzureOpenAIClient();
            return _azureOpenAIClient.GetEmbeddingClient(modelDeploymentName);
        }

        private async Task<IAzureAISearchService> GetAzureAISearchService(
            VectorDatabase vectorDatabase)
        {
            if (!_azureAISearchServices.TryGetValue(vectorDatabase.APIEndpointConfigurationObjectId, out var azureAISearchService))
            {
                var searchIndexClient = await _httpClientFactory.CreateClient<SearchIndexClient>(
                    ResourcePath.GetResourcePath(
                        vectorDatabase.APIEndpointConfigurationObjectId)
                        .ResourceId!,
                    ServiceContext.ServiceIdentity!,
                    AzureAISearchService.CreateSearchIndexClient);
                azureAISearchService = new AzureAISearchService(
                    searchIndexClient,
                    _loggerFactory.CreateLogger<AzureAISearchService>());
                _azureAISearchServices[vectorDatabase.ObjectId!] = azureAISearchService;
            }

            return azureAISearchService;
        }

        private async Task<AzureOpenAIClient> GetAzureOpenAIClient() =>
            await _httpClientFactory.CreateClient<AzureOpenAIClient>(
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
    }
}
