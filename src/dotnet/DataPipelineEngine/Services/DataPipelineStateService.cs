using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for data pipeline state management.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The storage service providing blob storage capabilities.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineStateService(
        IAzureCosmosDBDataPipelineService cosmosDBService,
        IStorageService storageService,
        ILogger<DataPipelineStateService> logger) : IDataPipelineStateService
    {
        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;
        private readonly IStorageService _storageService = storageService;
        private readonly ILogger<DataPipelineStateService> _logger = logger;

        /// <inheritdoc/>
        public async Task<bool> InitializeDataPipelineRunState(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems)
        {
            // Combine dataPipelineRun, contentItems, and workItems into a single array
            var combinedArray = new object[] { dataPipelineRun }
                .Concat(contentItems)
                .ToArray();

            var upsertResultSuccessfull = await _cosmosDBService.UpsertDataPipelineRunBatchAsync(combinedArray);

            return upsertResultSuccessfull;
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> GetDataPipelineRun(
            string instanceId,
            string runId,
            UnifiedUserIdentity userIdentity)
        {
            var result = await _cosmosDBService.RetrieveItem<DataPipelineRun>(
                runId,
                runId);

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> PersistDataPipelineRunWorkItems(
            List<DataPipelineRunWorkItem> workItems)
        {
            var upsertResultSuccessfull = await _cosmosDBService.UpsertDataPipelineRunBatchAsync(workItems);
            return upsertResultSuccessfull;
        }

        /// <inheritdoc/>
        public async Task UpdateDataPipelineRunWorkItemsStatus(
            List<DataPipelineRunWorkItem> workItems) =>
            await _cosmosDBService.UpsertDataPipelineRunBatchAsync(workItems);
    }
}
