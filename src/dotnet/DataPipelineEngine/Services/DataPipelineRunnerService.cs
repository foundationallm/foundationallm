using Azure.Storage.Queues;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.DataPipelineEngine.Services.Runners;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    public class DataPipelineRunnerService :
            DataPipelineBackgroundService, IDataPipelineRunnerService
    {
        protected override string ServiceName => "Data Pipeline Runner Service";

        private readonly DataPipelineServiceSettings _settings;
        private readonly IDataPipelineStateService _stateService;
        private readonly QueueClient _frontendQueueClient;
        private readonly QueueClient _backendQueueClient;

        /// <summary>
        /// Initializes a new instance of the service.
        /// </summary>
        /// <param name="stateService">The Data Pipeline State service providing state management services.</param>
        /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
        /// <param name="logger">The logger used for logging.</param>
        public DataPipelineRunnerService(
            IDataPipelineStateService stateService,
            IServiceProvider serviceProvider,
            IOptions<DataPipelineServiceSettings> options,
            ILogger<DataPipelineRunnerService> logger) :
                base(TimeSpan.FromMinutes(1), serviceProvider, logger)
        {
            _settings = options.Value;
            _stateService = stateService;

            var queueServiceClient = new QueueServiceClient(
                new Uri($"https://{_settings.Storage.AccountName}.queue.core.windows.net"),
                ServiceContext.AzureCredential);
            _frontendQueueClient = queueServiceClient.GetQueueClient(_settings.FrontendWorkerQueue);
            _backendQueueClient = queueServiceClient.GetQueueClient(_settings.BackendWorkerQueue);
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsyncInternal(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("The {ServiceName} service is initializing...", ServiceName);

            await Task.CompletedTask;

            _logger.LogInformation("The {ServiceName} service has been initialized.", ServiceName);
        }

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

            var dataPipelineRunner = new DataPipelineRunner(
                _stateService,
                _serviceProvider);

            await dataPipelineRunner.Initialize(
                dataPipelineRun,
                contentItems,
                dataPipelineDefinition,
                userIdentity);

            

            dataPipelineRun.ActiveStages = [.. dataPipelineDefinition.StartingStages.Select(s => s.Name)];

            // Fetch work items for all starting stages and consolidate them into a single list
            var workItems = (await Task.WhenAll(
                dataPipelineDefinition.StartingStages.Select(s =>
                    GetStartingStageWorkItems(dataPipelineRun, s, contentItems, userIdentity))
            )).SelectMany(w => w).ToList();

            var initializationSuccessful = await _stateService.InitializeDataPipelineRunState(
                dataPipelineRun,
                contentItems);

            await _stateService.PersistDataPipelineRunWorkItems(workItems);

            if (!initializationSuccessful)
                throw new DataPipelineServiceException($"Failed to upsert data pipeline run {dataPipelineRun.RunId}.");

            #region Send the newly created workitems to the work item queue

            var queueClient = dataPipelineRun.Processor switch
            {
                DataPipelineRunProcessors.Frontend => _frontendQueueClient,
                DataPipelineRunProcessors.Backend => _backendQueueClient,
                _ => throw new DataPipelineServiceException(
                        $"Unsupported processor type: {dataPipelineRun.Processor}",
                        StatusCodes.Status400BadRequest)
            };

            _logger.LogInformation("Starting to queue data pipeline work items for {ProcessorName}...",
                dataPipelineRun.Processor);

            var failedWorkItems = new List<DataPipelineRunWorkItem>();

            foreach (var workItem in workItems)
            {
                try
                {
                    await queueClient.SendMessageAsync(workItem.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to queue work item {WorkItemId} for {ProcessorName}.",
                        workItem.Id, dataPipelineRun.Processor);
                    workItem.Completed = true;
                    workItem.Successful = false;
                    workItem.Error = "The data pipeline item could not be queued.";
                    failedWorkItems.Add(workItem);
                }

            }

            if (failedWorkItems.Count > 0)
                await _stateService.UpdateDataPipelineRunWorkItemsStatus(failedWorkItems);

            _logger.LogInformation("Finished queueing data pipeline work items for {ProcessorName}.",
                dataPipelineRun.Processor);

            #endregion

            return dataPipelineRun;
        }

        private async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            DataPipelineRun dataPipelineRun,
            DataPipelineStage dataPipelineStage,
            List<DataPipelineContentItem> contentItems,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipelineStagePlugin = await _pluginService.GetDataPipelineStagePlugin(
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
