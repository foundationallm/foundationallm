using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineRunnerService(
        IAzureCosmosDBDataPipelineService cosmosDBService,
        IServiceProvider serviceProvider,
        ILogger<DataPipelineRunnerService> logger) :
            DataPipelineBackgroundService(
                TimeSpan.FromMinutes(1),
                serviceProvider,
                logger), IDataPipelineRunnerService
    {
        protected override string ServiceName => "Data Pipeline Runner Service";

        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;

        /// <inheritdoc/>
        protected override async Task ExecuteAsyncInternal(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("The {ServiceName} service is executing...", ServiceName);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun> StartRun(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            DataPipelineDefinition dataPipelineDefinition,
            UnifiedUserIdentity userIdentity)
        {
            // Wait for the initialization task to complete before proceeding.
            // At this point this task should be completed and the await should have no performance impact.
            // This is necessary to avoid any issues during the initialization of the service.
            await _initializationTask;

            foreach (var contentItem in contentItems)
                contentItem.RunId = dataPipelineRun.RunId;

            dataPipelineRun.ActiveStages = [.. dataPipelineDefinition.StartingStages.Select(s => s.Name)];

            // Fetch work items for all starting stages and consolidate them into a single list
            var workItems = (await Task.WhenAll(
                dataPipelineDefinition.StartingStages.Select(s =>
                    GetStartingStageWorkItems(dataPipelineRun, s, contentItems, userIdentity))
            )).SelectMany(w => w).ToList();

            // Combine dataPipelineRun and contentItems into a single array
            var combinedArray = new object[] { dataPipelineRun }
                .Concat(contentItems)
                .Concat(workItems)
                .ToArray();

            var result = await _cosmosDBService.UpsertDataPipelineRunBatchAsync(combinedArray);

            if (!result)
                throw new DataPipelineServiceException($"Failed to upsert data pipeline run {dataPipelineRun.RunId}.");

            // Send the newly created workitems to the work item queue


            return dataPipelineRun;
        }

        private async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            DataPipelineRun dataPipelineRun,
            DataPipelineStage dataPipelineStage,
            List<DataPipelineContentItem> contentItems,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipelineStagePlugin = await GetDataPipelineStagePlugin(
                dataPipelineRun.InstanceId,
                dataPipelineStage.PluginObjectId,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"Stage.{dataPipelineStage.Name}."),
                userIdentity);

            var workItems = await dataPipelineStagePlugin.GetStartingStageWorkItems(
                contentItems,
                dataPipelineRun.RunId,
                dataPipelineStage.Name);

            return workItems;
        }
    }
}
