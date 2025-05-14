using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Azure.Cosmos;
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
            UnifiedUserIdentity userIdentity) =>
            await _cosmosDBService.RetrieveItem<DataPipelineRun>(
                runId,
                runId);

        /// <inheritdoc/>
        public async Task<DataPipelineRunWorkItem?> GetDataPipelineRunWorkItem(
            string workItemId,
            string runId) =>
            await _cosmosDBService.RetrieveItem<DataPipelineRunWorkItem>(
                workItemId,
                runId);

        /// <inheritdoc/>
        public async Task<bool> UpdateDataPipelineRunStatus(
            DataPipelineRun dataPipelineRun)
        {
            try
            {
                await _cosmosDBService.PatchItemPropertiesAsync<DataPipelineRun>(
                    dataPipelineRun.RunId,
                    dataPipelineRun.Id,
                    new Dictionary<string, object?>
                    {
                        { "/active_stages", dataPipelineRun.ActiveStages },
                        { "/completed_stages", dataPipelineRun.CompletedStages },
                        { "/failed_stages", dataPipelineRun.FailedStages },
                        { "/completed", dataPipelineRun.Completed },
                        { "/successful", dataPipelineRun.Successful }
                    });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update data pipeline run status for {RunId}.", dataPipelineRun.RunId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PersistDataPipelineRunWorkItems(
            List<DataPipelineRunWorkItem> workItems) =>
            await _cosmosDBService.UpsertDataPipelineRunBatchAsync(workItems.ToArray());

        /// <inheritdoc/>
        public async Task<bool> UpdateDataPipelineRunWorkItemsStatus(
            List<DataPipelineRunWorkItem> workItems) =>
            await _cosmosDBService.PatchDataPipelineRunWorkItemsStatusAsync(workItems);

        /// <inheritdoc/>
        public async Task<List<DataPipelineRun>> GetActiveDataPipelineRuns()
        {
            var query = new QueryDefinition(
                $"SELECT DISTINCT * FROM c WHERE c.type = @type AND c.completed = false")
                .WithParameter("@type", DataPipelineTypes.DataPipelineRun);

            var dataPipelineRuns = await _cosmosDBService.RetrieveItemsAsync<DataPipelineRun>(query);

            return dataPipelineRuns;
        }

        /// <inheritdoc/>
        public async Task<List<DataPipelineRunWorkItem>> GetDataPipelineRunStageWorkItems(
            string runId,
            string stage)
        {
            var query = new QueryDefinition(
                $"SELECT * FROM c WHERE c.type = @type AND c.run_id = @runId and c.stage = @stage")
                .WithParameter("@type", DataPipelineTypes.DataPipelineRunWorkItem)
                .WithParameter("@runId", runId)
                .WithParameter("@stage", stage);
            var dataPipelineRunWorkItems = await _cosmosDBService.RetrieveItemsAsync<DataPipelineRunWorkItem>(query);
            return dataPipelineRunWorkItems;
        }

        /// <inheritdoc/>
        public async Task<bool> StartDataPipelineRunWorkItemProcessing(
            Func<DataPipelineRunWorkItem, Task> processWorkItem) =>
            await _cosmosDBService.StartChangeFeedProcessorAsync(processWorkItem);

        /// <inheritdoc/>
        public async Task StopDataPipelineRunWorkItemProcessing() =>
            await _cosmosDBService.StopChangeFeedProcessorAsync();
    }
}
