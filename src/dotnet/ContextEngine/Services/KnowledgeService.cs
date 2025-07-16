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
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
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
    public class KnowledgeService(
        IStorageService storageService,
        IAuthorizationServiceClient authorizationServiceClient,
        IResourceProviderService configurationResourceProvider,
        IResourceProviderService vectorResourceProvider,
        IHttpClientFactoryService httpClientFactory,
        KnowledgeServiceSettings settings,
        ILoggerFactory loggerFactory) : IKnowledgeService
    {
        private readonly IStorageService _storageService = storageService;
        private readonly IAuthorizationServiceClient _authorizationServiceClient = authorizationServiceClient;
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

        private const string KNOWLEDGE_SOURCE_ROOT_PATH = "knowledge-source";
        private const string KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME = "entities.parquet";
        private const string KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME = "relationships.parquet";
        private const string KEY_FIELD_NAME = "Id";

        /// <inheritdoc />
        public async Task<IEnumerable<KnowledgeSource>> GetKnowledgeSources(
            string instanceId,
            ContextKnowledgeSourceListRequest listRequest,
            UnifiedUserIdentity userIdentity)
        {
            var filePaths = await _storageService.GetMatchingFilePathsAsync(
               instanceId,
               $"{KNOWLEDGE_SOURCE_ROOT_PATH}/");

            filePaths = [.. filePaths
                .Where(fp =>
                    {
                        var tokens = fp.Split('/');

                        return
                            fp.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                            && tokens.Length == 2
                            && (listRequest.KnowledgeSourceNames is null
                                || listRequest.KnowledgeSourceNames.Contains(
                                    Path.GetFileNameWithoutExtension(tokens[1])));
                    })
            ];

            var result = await filePaths
                .ToAsyncEnumerable()
                .SelectAwait(async path =>
                {
                    var fileContent = await _storageService.ReadFileAsync(
                        instanceId,
                        path,
                        default);

                    return
                        JsonSerializer.Deserialize<KnowledgeSource>(fileContent);
                })
                .ToListAsync();

            return result
                .Where(ks => ks != null)
                .Select(ks => ks!)
                .OrderBy(ks => ks.Name);
        }

        /// <inheritdoc />
        public async Task UpdateKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceUpdateRequest updateRequest,
            UnifiedUserIdentity userIdentity)
        {
            if (!string.IsNullOrWhiteSpace(updateRequest.EntitiesSourceFilePath))
            {
                // The knowledge source also contains a graph of entities and relationships.

                await _storageService.CopyFileAsync(
                    instanceId,
                    updateRequest.EntitiesSourceFilePath,
                    string.Join('/',
                        [
                            KNOWLEDGE_SOURCE_ROOT_PATH,
                        knowledgeSourceId,
                        KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                        ]));

                await _storageService.CopyFileAsync(
                    instanceId,
                    updateRequest.RelationshipsSourceFilePath!,
                    string.Join('/',
                        [
                            KNOWLEDGE_SOURCE_ROOT_PATH,
                        knowledgeSourceId,
                        KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                        ]));
            }

            await UpdateKnowledgeSourceProperties(
                instanceId,
                knowledgeSourceId,
                updateRequest,
                userIdentity);

            // If the knowledge graph is cached, remove it from the cache.
            var cacheKey = GetCacheKey(instanceId, knowledgeSourceId);
            if (_cache.TryGetValue(cacheKey, out _))
                _cache.Remove(cacheKey);
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
                var knowledgeSourcePropertiesFilePath = string.Join('/',
                [
                    KNOWLEDGE_SOURCE_ROOT_PATH,
                    $"{knowledgeSourceId}.json"
                ]);

                var knowledgeSourceBinaryContent = await _storageService.ReadFileAsync(
                    instanceId,
                    knowledgeSourcePropertiesFilePath,
                    default);
                var knowledgeSource =
                    JsonSerializer.Deserialize<KnowledgeSource>(knowledgeSourceBinaryContent)
                    ?? throw new ResourceProviderException(
                        $"The knowledge source properties for {knowledgeSourceId} could not be deserialized.");

                if (!knowledgeSource.HasKnowledgeGraph
                    && queryRequest.KnowledgeGraphQuery is not null)
                    return new ContextKnowledgeSourceQueryResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge source '{knowledgeSourceId}' for instance '{instanceId}' does not contain a knowledge graph."
                    };

                var vectorStoreId = string.IsNullOrWhiteSpace(knowledgeSource.VectorStoreId)
                    ? queryRequest.VectorStoreId
                    : knowledgeSource.VectorStoreId;
                if (string.IsNullOrWhiteSpace(vectorStoreId))
                    return new ContextKnowledgeSourceQueryResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge source '{knowledgeSourceId}' for instance '{instanceId}' does not have a vector store identifier specified and none was provided in the query request."
                    };

                var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                    knowledgeSource!.VectorDatabaseObjectId,
                    ServiceContext.ServiceIdentity!);

                var cachedKnowledgeSource = await GetKnowledgeSourceFromCache(
                    instanceId,
                    knowledgeSource,
                    vectorDatabase);

                var userPromptEmbedding = await cachedKnowledgeSource.EmbeddingClient.GenerateEmbeddingAsync(
                    queryRequest.UserPrompt,
                    new EmbeddingGenerationOptions
                    {
                        Dimensions = knowledgeSource.EmbeddingDimensions
                    });

                var queryResponse = new ContextKnowledgeSourceQueryResponse
                {
                    Success = true
                };
                var matchingDocumentsFilter = string.Empty;

                if (queryRequest.KnowledgeGraphQuery is not null)
                {
                    var matchingEntities = GetMatchingKnowledgeGraphEntities(
                        cachedKnowledgeSource.KnowledgeGraph!,
                        userPromptEmbedding.Value.ToFloats(),
                        queryRequest.KnowledgeGraphQuery.MappedEntitiesSimilarityThreshold,
                        queryRequest.KnowledgeGraphQuery.MappedEntitiesMaxCount,
                        queryRequest.KnowledgeGraphQuery.RelationshipsMaxDepth,
                        queryRequest.KnowledgeGraphQuery.AllEntitiesMaxCount);

                    queryResponse.KnowledgeGraphResponse = new ContextKnowledgeGraphResponse
                    {
                        Entities = [.. matchingEntities.Entities
                            .Select(e => new KnowledgeEntity
                            {
                                Type = e.Type,
                                Name = e.Name,
                                SummaryDescription = e.SummaryDescription
                            })
                        ],
                        RelatedEntities = [.. matchingEntities.RelatedEntities
                            .Select(re => new KnowledgeEntity
                            {
                                Type = re.Type,
                                Name = re.Name,
                                SummaryDescription = re.SummaryDescription
                            })
                        ],
                        Relationships = [.. matchingEntities.Relationships
                            .Select(r => new KnowledgeRelationship
                            {
                                SourceType = r.SourceType,
                                Source = r.Source,
                                TargetType = r.TargetType,
                                Target = r.Target,
                                SummaryDescription = r.SummaryDescription
                            })
                        ]
                    };

                    matchingDocumentsFilter = string.Join(" and ",
                    [
                        $"{vectorDatabase.VectorStoreIdPropertyName} eq '{vectorStoreId}'",
                        $"search.in({KEY_FIELD_NAME}, '{string.Join(',', matchingEntities.Entities.SelectMany(e => e.ChunkIds))}')"
                    ]);
                }
                else
                {
                    matchingDocumentsFilter = $"{vectorDatabase.VectorStoreIdPropertyName} eq '{vectorStoreId}'";
                }

                if (queryRequest.MetadataFilter is not null
                    && queryRequest.MetadataFilter.Count > 0)
                {
                    matchingDocumentsFilter += " and " + string.Join(" and ",
                        queryRequest.MetadataFilter
                            .Select(kvp => $"{vectorDatabase.MetadataPropertyName}/{kvp.Key} eq '{kvp.Value.Replace("'", "''")}'"));
                }

                var matchingDocuments = await cachedKnowledgeSource.SearchService.SearchDocuments(
                    vectorDatabase.DatabaseName,
                    [
                        KEY_FIELD_NAME,
                        vectorDatabase.ContentPropertyName,
                        vectorDatabase.MetadataPropertyName
                    ],
                    matchingDocumentsFilter,
                    queryRequest.UserPrompt!,
                    userPromptEmbedding.Value.ToFloats(),
                    vectorDatabase.EmbeddingPropertyName,
                    queryRequest.TextChunksSimilarityThreshold,
                    queryRequest.TextChunksMaxCount,
                    queryRequest.UseSemanticRanking);

                queryResponse.TextChunks = [.. matchingDocuments
                        .Select(md => new ContextTextChunk
                        {
                            Content = md.GetString(vectorDatabase.ContentPropertyName),
                            Metadata = md.GetObject(vectorDatabase.MetadataPropertyName).ToDictionary()
                        })
                    ];

                return queryResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge graph {KnowledgeGraphId} for instance {InstanceId}.",
                    knowledgeSourceId, instanceId);
                return new ContextKnowledgeSourceQueryResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while querying the knowledge graph '{knowledgeSourceId}' for instance '{instanceId}'."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeSourceRenderGraphResponse> RenderKnowledgeSourceGraph(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest? queryRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var knowledgeSourcePropertiesFilePath = string.Join('/',
                [
                    KNOWLEDGE_SOURCE_ROOT_PATH,
                    $"{knowledgeSourceId}.json"
                ]);

                var knowledgeSourceBinaryContent = await _storageService.ReadFileAsync(
                    instanceId,
                    knowledgeSourcePropertiesFilePath,
                    default);
                var knowledgeSource =
                    JsonSerializer.Deserialize<KnowledgeSource>(knowledgeSourceBinaryContent)
                    ?? throw new ResourceProviderException(
                        $"The knowledge source properties for {knowledgeSourceId} could not be deserialized.");

                if (!knowledgeSource.HasKnowledgeGraph)
                    return new ContextKnowledgeSourceRenderGraphResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge source '{knowledgeSourceId}' for instance '{instanceId}' does not contain a knowledge graph."
                    };

                var vectorStoreId = string.IsNullOrWhiteSpace(knowledgeSource.VectorStoreId)
                    ? queryRequest!.VectorStoreId
                    : knowledgeSource.VectorStoreId;
                if (string.IsNullOrWhiteSpace(vectorStoreId))
                    return new ContextKnowledgeSourceRenderGraphResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge source '{knowledgeSourceId}' for instance '{instanceId}' does not have a vector store identifier specified and none was provided in the rendering request."
                    };

                var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                    knowledgeSource!.VectorDatabaseObjectId,
                    ServiceContext.ServiceIdentity!);

                var cachedKnowledgeSource = await GetKnowledgeSourceFromCache(
                    instanceId,
                    knowledgeSource,
                    vectorDatabase);

                var renderResponse = new ContextKnowledgeSourceRenderGraphResponse
                {
                    Success = true
                };

                if (queryRequest is null)
                {
                    renderResponse.Nodes = [.. cachedKnowledgeSource.KnowledgeGraph!.Entities
                        .Select(e => new KnowledgeGraphRenderingNode
                        {
                            Id = e.UniqueId,
                            Label = e.Name,
                        })];
                    renderResponse.Edges = [.. cachedKnowledgeSource.KnowledgeGraph!.Relationships
                        .Where(r => cachedKnowledgeSource.KnowledgeGraph.Index.Nodes.ContainsKey(r.SourceUniqueId)
                                    && cachedKnowledgeSource.KnowledgeGraph.Index.Nodes.ContainsKey(r.TargetUniqueId))
                        .Select(r => new List<string> { r.SourceUniqueId, r.TargetUniqueId })];
                }

                return renderResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering the knowledge graph {KnowledgeGraphId} for instance {InstanceId}.",
                    knowledgeSourceId, instanceId);
                return new ContextKnowledgeSourceRenderGraphResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while rendering the knowledge graph '{knowledgeSourceId}' for instance '{instanceId}'."
                };
            }
        }

        private async Task UpdateKnowledgeSourceProperties(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceUpdateRequest updateRequest,
            UnifiedUserIdentity userIdentity)
        {
            var knowledgeSource = default(KnowledgeSource);
            var knowledgeSourceFilePath = string.Join('/',
                [
                    KNOWLEDGE_SOURCE_ROOT_PATH,
                    $"{knowledgeSourceId}.json"
                ]);

            if (!await _storageService.FileExistsAsync(
                instanceId,
                knowledgeSourceFilePath,
                default))
            {
                // If the knowledge source properties file does not exist, create it with default values.
                knowledgeSource = new KnowledgeSource
                {
                    Type = "knowledge-source",
                    Name = knowledgeSourceId,
                    ObjectId = $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Context}/knowledgeSources/{knowledgeSourceId}",
                    EmbeddingModel = updateRequest.EmbeddingModel,
                    EmbeddingDimensions = updateRequest.EmbeddingDimensions,
                    VectorDatabaseObjectId = updateRequest.VectorDatabaseObjectId,
                    VectorStoreId = updateRequest.VectorStoreId,
                    HasKnowledgeGraph = !string.IsNullOrWhiteSpace(updateRequest.EntitiesSourceFilePath),
                    CreatedBy = userIdentity.UPN,
                    CreatedOn = DateTimeOffset.UtcNow
                };
            }
            else
            {
                var knowledgeSourceContent = await _storageService.ReadFileAsync(
                    instanceId,
                    knowledgeSourceFilePath,
                    default);
                knowledgeSource = JsonSerializer.Deserialize<KnowledgeSource>(knowledgeSourceContent);

                knowledgeSource!.EmbeddingModel = updateRequest.EmbeddingModel;
                knowledgeSource.EmbeddingDimensions = updateRequest.EmbeddingDimensions;
                knowledgeSource.VectorDatabaseObjectId = updateRequest.VectorDatabaseObjectId;
                knowledgeSource.VectorStoreId = updateRequest.VectorStoreId;
                knowledgeSource.HasKnowledgeGraph = !string.IsNullOrWhiteSpace(updateRequest.EntitiesSourceFilePath);
                knowledgeSource.UpdatedBy = userIdentity.UPN;
                knowledgeSource.UpdatedOn = DateTimeOffset.UtcNow;
            }

            await _storageService.WriteFileAsync(
                    instanceId,
                    knowledgeSourceFilePath,
                    JsonSerializer.Serialize(knowledgeSource),
                    "application/json",
                    default);
        }

        private async Task<CachedKnowledgeSource> GetKnowledgeSourceFromCache(
            string instanceId,
            KnowledgeSource knowledgeSource,
            VectorDatabase vectorDatabase)
        {
            var cacheKey = GetCacheKey(instanceId, knowledgeSource.Name);

            if (!_cache.TryGetValue(cacheKey, out CachedKnowledgeSource? cachedKnowledgeSource))
            {
                cachedKnowledgeSource = new CachedKnowledgeSource
                {
                    SearchService = await GetAzureAISearchService(
                        vectorDatabase),
                    EmbeddingClient = await GetEmbeddingClient(
                        _settings.Embedding.ModelDeployments[knowledgeSource.EmbeddingModel]),
                };

                if (knowledgeSource.HasKnowledgeGraph)
                {
                    var entitiesBinaryContent = await _storageService.ReadFileAsync(
                        instanceId,
                        string.Join('/',
                            [
                                KNOWLEDGE_SOURCE_ROOT_PATH,
                            knowledgeSource.Name,
                            KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                            ]),
                        default);

                    var entities = await ParquetSerializer.DeserializeAsync<KnowledgeEntity>(
                        entitiesBinaryContent.ToStream());

                    var relationshipsBinaryContent = await _storageService.ReadFileAsync(
                        instanceId,
                        string.Join('/',
                            [
                                KNOWLEDGE_SOURCE_ROOT_PATH,
                            knowledgeSource.Name,
                            KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                            ]),
                        default);

                    var relationships = await ParquetSerializer.DeserializeAsync<KnowledgeRelationship>(
                        relationshipsBinaryContent.ToStream());

                    cachedKnowledgeSource.KnowledgeGraph = new CachedKnowledgeGraph
                    {
                        Entities = [.. entities],
                        Relationships = [.. relationships],
                        Index = KnowledgeGraphIndex.Create(
                            entities,
                            relationships)
                    };
                }
                
                // Store the knowledge source in the cache.
                _cache.Set(
                    cacheKey,
                    cachedKnowledgeSource,
                    GetMemoryCacheEntryOptions());
            }

            return cachedKnowledgeSource!;
        }


        private (List<KnowledgeEntity> Entities, List<KnowledgeEntity> RelatedEntities, List<KnowledgeRelationship> Relationships)
            GetMatchingKnowledgeGraphEntities(
                CachedKnowledgeGraph knowledgeGraph,
                ReadOnlyMemory<float> userPromptEmbedding,
                float similarityThreshold,
                int matchedEntitiesMaxCount,
                int relationshipsMaxDepth,
                int allEntitiesMaxCount)
        {
            var similarEntities = new List<(KnowledgeEntity Entity, float SimilarityScore)>();
            var maxSimilarity = 0f;
            var minSimilarity = 1f;
            foreach (var entity in knowledgeGraph.Entities)
            {
                var similarity = 1.0f - Distance.Cosine(
                    userPromptEmbedding.ToArray(),
                    entity.SummaryDescriptionEmbedding);
                if (similarity >= similarityThreshold)
                {
                    similarEntities.Add((entity, similarity));
                }

                if (similarity > maxSimilarity)
                    maxSimilarity = similarity;
                if (similarity < minSimilarity)
                    minSimilarity = similarity;
            }

            _logger.LogInformation("The minimum and maximum similarity values are {MinSimilarity} and {MaxSimilarity}", minSimilarity, maxSimilarity);

            List<KnowledgeEntity> entities = [.. similarEntities
                .OrderByDescending(se => se.SimilarityScore)
                .Take(matchedEntitiesMaxCount)
                .Select(se => se.Entity)];

            var remainingEntitiesCount = allEntitiesMaxCount >= matchedEntitiesMaxCount
                ? allEntitiesMaxCount - matchedEntitiesMaxCount
                : 0;

            if (remainingEntitiesCount == 0
                || relationshipsMaxDepth == 0)
                return (entities, [], []);

            var entitiesIds = entities
                .Select(e => e.UniqueId)
                .ToHashSet();

            var relationshipEntities = new List<KnowledgeEntity>();
            var relationships = new List<KnowledgeRelationship>();

            // Get the first layer of related entities for the most similar entities.
            // This is required to kick off the search for the most relevant relationships.
            var currentIndexRelatedNodes = entities
                .Select(e => knowledgeGraph.Index.Nodes[e.UniqueId])
                .SelectMany(n => n.RelatedNodes)
                .Where(rn => !entitiesIds.Contains(rn.RelatedEntity.UniqueId))
                .Distinct(new KnowledgeGraphIndexRelatedNodeComparer())
                .ToList();
            var currentSimilarityScores = currentIndexRelatedNodes
                .Select(rn => 1.0f - Distance.Cosine(
                    userPromptEmbedding.ToArray(),
                    rn.RelatedEntity.SummaryDescriptionEmbedding))
                .ToList();
            var currentDepthLevels = currentIndexRelatedNodes
                .Select(n => 1)
                .ToList();

            // Repeat the process until the specified number of entities is reached or there are no more
            // related nodes to process.
            while (remainingEntitiesCount > 0
                && currentIndexRelatedNodes.Count > 0)
            {
                // Identify the most similar node.
                var currentMaxSimilarity = currentSimilarityScores.Max();
                var mostSimilarIndex = currentSimilarityScores
                    .FindIndex(s => s == currentMaxSimilarity);
                var mostSimilarRelatedNode = currentIndexRelatedNodes[mostSimilarIndex];
                var mostSimilarRelatedNodeDepth = currentDepthLevels[mostSimilarIndex];

                // Add the node at the index to the results.
                entitiesIds.Add(mostSimilarRelatedNode.RelatedEntity.UniqueId);
                relationshipEntities.Add(mostSimilarRelatedNode.RelatedEntity);
                relationships.Add(mostSimilarRelatedNode.Relationship);

                //Remove the node from the current state.
                currentIndexRelatedNodes.RemoveAt(mostSimilarIndex);
                currentSimilarityScores.RemoveAt(mostSimilarIndex);
                currentDepthLevels.RemoveAt(mostSimilarIndex);

                // From the nodes directly related to the most similar node (if any),
                // determine the ones that are neither in the current results nor in the current state
                // and add them to the current state.
                // If the most similar node is not at the maximum allowed depth, do not continue.
                var mostSimilarIndexNode = knowledgeGraph.Index.Nodes[mostSimilarRelatedNode.RelatedEntity.UniqueId];
                if (mostSimilarIndexNode.RelatedNodes.Count > 0
                    && mostSimilarRelatedNodeDepth < relationshipsMaxDepth)
                {
                    var indexRelatedNodesToAdd = mostSimilarIndexNode
                        .RelatedNodes
                        .Where(rn =>
                            !entitiesIds.Contains(rn.RelatedEntity.UniqueId)
                            && !currentIndexRelatedNodes.Contains(rn, new KnowledgeGraphIndexRelatedNodeComparer()))
                        .ToList();
                    var similarityScoresToAdd = indexRelatedNodesToAdd
                        .Select(rn => 1.0f - Distance.Cosine(
                            userPromptEmbedding.ToArray(),
                            rn.RelatedEntity.SummaryDescriptionEmbedding))
                        .ToList();
                    var depthLevelsToAdd = indexRelatedNodesToAdd
                        .Select(rn => mostSimilarRelatedNodeDepth + 1)
                        .ToList();

                    currentIndexRelatedNodes.AddRange(indexRelatedNodesToAdd);
                    currentSimilarityScores.AddRange(similarityScoresToAdd);
                    currentDepthLevels.AddRange(depthLevelsToAdd);
                }

                remainingEntitiesCount--;
            }

            return (entities, relationshipEntities, relationships);
        }

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // Cache entries are valid for 60 minutes.
           .SetSlidingExpiration(TimeSpan.FromSeconds(1800)) // Reset expiration time if accessed within 30 minutes.
           .SetSize(1); // Each cache entry is a single knowledge graph instance.

        private string GetCacheKey(string instanceId, string knowledgeSourceId) =>
            $"{instanceId}:{knowledgeSourceId}";

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
