using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM File service.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="authorizationServiceClient">The client for the FoundationaLLM Authorization API.</param>
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
    }
}
