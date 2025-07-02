using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="authorizationServiceClient">The client for the FoundationaLLM Authorization API.</param>
    /// <param name="settings">The settings for the Knowledge Graph service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class KnowledgeGraphService(
        IStorageService storageService,
        IAuthorizationServiceClient authorizationServiceClient,
        KnowledgeGraphServiceSettings settings,
        ILogger<KnowledgeGraphService> logger) : IKnowledgeGraphService
    {
        private readonly IStorageService _storageService = storageService;
        private readonly IAuthorizationServiceClient _authorizationServiceClient = authorizationServiceClient;
        private readonly KnowledgeGraphServiceSettings _settings = settings;
        private readonly ILogger<KnowledgeGraphService> _logger = logger;

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100, // Limit cache size to 100 units (knowledge graph instances).
            ExpirationScanFrequency = TimeSpan.FromSeconds(60) // How often to scan for expired items.
        });

        private const string KNOWLEDGE_GRAPH_ROOT_PATH = "knowledge-graph";
        private const string KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME = "entities.parquet";
        private const string KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME = "relationships.parquet";

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
        }

        /// <inheritdoc/>
        public Task<ContextKnowledgeGraphQueryResponse> QueryKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            ContextKnowledgeGraphQueryRequest queryRequest,
            UnifiedUserIdentity userIdentity) =>
            throw new NotImplementedException();

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

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // Cache entries are valid for 60 minutes.
           .SetSlidingExpiration(TimeSpan.FromSeconds(1800)) // Reset expiration time if accessed within 30 minutes.
           .SetSize(1); // Each cache entry is a single knowledge graph instance.
    }
}
