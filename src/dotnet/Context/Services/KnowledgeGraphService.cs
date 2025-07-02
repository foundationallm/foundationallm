using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Logging;

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
            string entitiesSourceFilePath,
            string relationshipsSourceFilePath)
        {
            await _storageService.CopyFileAsync(
                instanceId,
                entitiesSourceFilePath,
                string.Join('/',
                    [
                        KNOWLEDGE_GRAPH_ROOT_PATH,
                        knowledgeGraphId,
                        KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                    ]));

            await _storageService.CopyFileAsync(
                instanceId,
                relationshipsSourceFilePath,
                string.Join('/',
                    [
                        KNOWLEDGE_GRAPH_ROOT_PATH,
                        knowledgeGraphId,
                        KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                    ]));
        }
    }
}
