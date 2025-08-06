using Azure.Storage.Queues;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    /// <param name="stateService">The Data Pipeline State service.</param>
    /// <param name="pluginService">The plugin service providing access to registered plugins.</param>
    /// <param name="queueClient">The queue client used for interacting with Azure Storage Queues.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    public class DataPipelineRunner(
        IDataPipelineStateService stateService,
        IPluginService pluginService,
        QueueClient queueClient,
        IServiceProvider serviceProvider)
    {
        private readonly IDataPipelineStateService _stateService = stateService;
        private readonly IPluginService _pluginService = pluginService;
        private readonly QueueClient _queueClient = queueClient;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<DataPipelineRunner> _logger =
            serviceProvider.GetRequiredService<ILogger<DataPipelineRunner>>();

        private readonly Dictionary<string, DataPipelineStageRunner> _currentStageRunners = [];
        private DataPipelineDefinition _dataPipelineDefinition = null!;
        private DataPipelineRun _dataPipelineRun = null!;
        private bool _initialized = false;

        public Dictionary<string, DataPipelineStageRunner> CurrentStageRunners => _currentStageRunners;

        public async Task InitializeNew(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            DataPipelineDefinition dataPipelineDefinition,
            UnifiedUserIdentity userIdentity)
        {
            _logger.LogInformation("Initializing new data pipeline run {DataPipelineRunId}...",
                dataPipelineRun.RunId);

            _dataPipelineDefinition = dataPipelineDefinition;
            _dataPipelineRun = dataPipelineRun;

            foreach (var contentItem in contentItems)
                contentItem.RunId = dataPipelineRun.RunId;

            dataPipelineRun.ActiveStages =
                [.. dataPipelineDefinition.StartingStages.Select(stage => stage.Name)];

            dataPipelineRun.AllStages = dataPipelineDefinition.AllStageNames;

            var initializationSuccessful = await _stateService.InitializeDataPipelineRunState(
                dataPipelineDefinition,
                dataPipelineRun,
                contentItems);

            if (!initializationSuccessful)
                throw new DataPipelineServiceException($"Failed to initialize state for data pipeline run {dataPipelineRun.RunId}.");

            foreach (var activeStage in dataPipelineDefinition.StartingStages)
            {
                _logger.LogInformation("Initializing stage {StageName} for new data pipeline run {DataPipelineRunId}...",
                    activeStage.Name, dataPipelineRun.RunId);

                var workItems = await GetStartingStageWorkItems(
                    dataPipelineRun,
                    activeStage,
                    contentItems,
                    userIdentity);

                await CreateStageRunner(
                    activeStage,
                    workItems);

                _logger.LogInformation("Finished initializing stage {StageName} for new data pipeline run {DataPipelineRunId}...",
                    activeStage.Name, dataPipelineRun.RunId);
            }

            _initialized = true;
            _logger.LogInformation("Finished initializing new data pipeline run {DataPipelineRunId}...",
               dataPipelineRun.RunId);
        }

        public async Task InitializeExisting(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            _logger.LogInformation("Initializing existing data pipeline run {DataPipelineRunId}...",
                dataPipelineRun.RunId);

            _dataPipelineDefinition = dataPipelineDefinition;
            _dataPipelineRun = dataPipelineRun;

            foreach (var activeStageName in dataPipelineRun.ActiveStages)
            {
                _logger.LogInformation("Initializing stage {StageName} for existing data pipeline run {DataPipelineRunId}...",
                    activeStageName, dataPipelineRun.RunId);

                var dataPipelineStageRunner = new DataPipelineStageRunner(
                    activeStageName,
                    _stateService,
                    _serviceProvider.GetRequiredService<ILogger<DataPipelineStageRunner>>());

                var workItems = await _stateService.GetDataPipelineRunStageWorkItems(
                    dataPipelineRun.RunId,
                    activeStageName);

                dataPipelineStageRunner.InitializeExisting(
                    workItems);
                _currentStageRunners[activeStageName] = dataPipelineStageRunner;

                _logger.LogInformation("Finished initializing stage {StageName} for existing data pipeline run {DataPipelineRunId}...",
                    activeStageName, dataPipelineRun.RunId);
            }

            _initialized = true;
            _logger.LogInformation("Finished initializing existing data pipeline run {DataPipelineRunId}...",
               dataPipelineRun.RunId);
        }

        public DataPipelineRun DataPipelineRun => _dataPipelineRun;

        public bool Initialized => _initialized;

        public async Task<bool> Completed()
        {
            try
            {
                var completedStageRunners =
                    _currentStageRunners.Values.Where(stageRunner => stageRunner.Completed).ToList();
                var changesInStageRunners =
                    _currentStageRunners.Values.Any(stageRunner => stageRunner.Changed);
                foreach (var stageRunner in _currentStageRunners.Values)
                    stageRunner.ResetChanged();

                foreach (var stageRunner in _currentStageRunners.Values)
                    _dataPipelineRun.StagesMetrics[stageRunner.StageName] =
                        new DataPipelineStageMetrics
                        {
                            WorkItemsCount = stageRunner.WorkItemsCount,
                            CompletedWorkItemsCount = stageRunner.CompletedWorkItemsCount,
                            SuccessfulWorkItemsCount = stageRunner.SuccessfulWorkItemsCount
                        };

                // Process completed stage runners and advance to the next stages if there are no global errors.
                // A data pipeline global error is an error that occurs outside of a specific stage,
                // usually when attempting to advance to the next stages.
                List<string> errors = [];
                bool newErrors = false;
                foreach (var stageRunner in completedStageRunners)
                    try
                    {
                        await ProcessCompletedStageRunner(
                            stageRunner,
                            // If at least one global error has occurred, do not start the next stages.
                            ((_dataPipelineRun.Errors?.Count ?? 0) == 0) || newErrors);

                        changesInStageRunners = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while processing completed stage runner {StageName} for data pipeline run {RunId}.",
                            stageRunner.StageName, _dataPipelineRun.RunId);
                        errors.Add($"[Process completed stage {stageRunner.StageName}] {ex.Message}");
                        newErrors = true;
                    }
                if (errors.Count > 0)
                    if (_dataPipelineRun.Errors == null)
                        _dataPipelineRun.Errors = errors;
                    else
                        _dataPipelineRun.Errors.AddRange(errors);

                _dataPipelineRun.Completed =
                    // No stages in progress.
                    _dataPipelineRun.ActiveStages.Count == 0
                    && (
                        // Either at least one stage failed
                        // or all stages are completed
                        // or at least one global error occurred.
                        _dataPipelineRun.FailedStages.Count > 0
                        || _dataPipelineRun.AllStages.Intersect(
                            _dataPipelineRun.CompletedStages).Count() == _dataPipelineRun.AllStages.Count
                        || (_dataPipelineRun.Errors ?? []).Count > 0
                    );
                _dataPipelineRun.Successful =
                    // The run is completed
                    // and there are no failed stages
                    // and there are no global errors.
                    _dataPipelineRun.Completed
                    && _dataPipelineRun.FailedStages.Count == 0
                    && (_dataPipelineRun.Errors?.Count ?? 0) == 0;

                if (_dataPipelineRun.Completed
                    || changesInStageRunners
                    || newErrors)
                    await _stateService.UpdateDataPipelineRunStatus(_dataPipelineRun);

                return _dataPipelineRun.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking if the data pipeline run {RunId} has completed.",
                    _dataPipelineRun.RunId);
                return true;
            }
        }

        public async Task ProcessDataPipelineRunWorkItem(
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (! _currentStageRunners.TryGetValue(dataPipelineRunWorkItem.Stage, out var stageRunner))
            {
                _logger.LogWarning("Data pipeline stage runner not found for work item {WorkItemId}.",
                    dataPipelineRunWorkItem.Id);
                return;
            }

            await stageRunner.ProcessDataPipelineRunWorkItem(dataPipelineRunWorkItem);
        }

        private async Task CreateStageRunner(
            DataPipelineStage dataPipelineStage,
            List<DataPipelineRunWorkItem> workItems)
        {
            var dataPipelineStageRunner = new DataPipelineStageRunner(
                dataPipelineStage.Name,
                _stateService,
                _serviceProvider.GetRequiredService<ILogger<DataPipelineStageRunner>>());

            await dataPipelineStageRunner.InitializeNew(workItems);

            // Add the runner to the list of current stage runners before queueing the work items.
            // This prevents a racing condition where the stage runner is not found when a work item
            // is processed and the change feed triggers the registered handler.
            _currentStageRunners[dataPipelineStage.Name] = dataPipelineStageRunner;

            var failedWorkItems = await QueueWorkItems(
                _dataPipelineRun,
                dataPipelineStage,
                workItems);

            if (failedWorkItems.Count > 0)
            {
                await dataPipelineStageRunner.SetFailedWorkItems(failedWorkItems);
                _logger.LogWarning("Failed to queue {FailedWorkItemsCount} work items for {ProcessorName}.",
                    failedWorkItems.Count, _dataPipelineRun.Processor);
            }
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

        private async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            DataPipelineRun dataPipelineRun,
            DataPipelineStage dataPipelineStage,
            DataPipelineStageRunner previousStageRunner,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipelineStagePlugin = await _pluginService.GetDataPipelineStagePlugin(
                dataPipelineRun.InstanceId,
                dataPipelineStage.PluginObjectId,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"Stage.{dataPipelineStage.Name}."),
                userIdentity);

            var workItems = await dataPipelineStagePlugin.GetStageWorkItems(
                previousStageRunner.ContentItemsCanonicalIds,
                dataPipelineRun.RunId,
                dataPipelineStage.Name,
                previousStageRunner.StageName);

            return workItems;
        }

        private async Task<List<DataPipelineRunWorkItem>> QueueWorkItems(
            DataPipelineRun dataPipelineRun,
            DataPipelineStage dataPipelineStage,
            List<DataPipelineRunWorkItem> workItems)
        {
            _logger.LogInformation("Starting to queue {WorkItemsCount} work items for {ProcessorName}, data pipeline run id {RunId}. stage {StageName}.",
                workItems.Count, dataPipelineRun.Processor, dataPipelineRun.RunId, dataPipelineStage.Name);

            var failedWorkItems = new List<DataPipelineRunWorkItem>();
            var processedWorkItems = 0;

            foreach (var workItem in workItems)
            {
                try
                {
                    await _queueClient.SendMessageAsync(JsonSerializer.Serialize(
                        new DataPipelineRunWorkItemMessage
                        {
                            WorkItemId = workItem.Id,
                            RunId = workItem.RunId
                        }));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to queue work item {WorkItemId} for {ProcessorName}.",
                        workItem.Id, dataPipelineRun.Processor);
                    workItem.Completed = true;
                    workItem.Successful = false;
                    workItem.Errors = ["The data pipeline item could not be queued."];
                    failedWorkItems.Add(workItem);
                }

                processedWorkItems++;

                if (processedWorkItems % 100 == 0)
                {
                    _logger.LogInformation("Queued {ProcessedWorkItemsCount} work items for {ProcessorName}, data pipeline run id {RunId}. stage {StageName}.",
                        processedWorkItems, dataPipelineRun.Processor, dataPipelineRun.RunId, dataPipelineStage.Name);
                }
            }

            _logger.LogInformation("Finished queueing {WorkItemsCount} work items for {ProcessorName}, data pipeline run id {RunId}. stage {StageName} ({FailedItemsCount} items failed to queue).",
                workItems.Count, dataPipelineRun.Processor, dataPipelineRun.RunId, dataPipelineStage.Name, failedWorkItems.Count);

            return failedWorkItems;
        }

        private async Task ProcessCompletedStageRunner(
            DataPipelineStageRunner stageRunner,
            bool startNextStage)
        {
            _currentStageRunners.Remove(stageRunner.StageName);

            if (!_dataPipelineRun.CompletedStages.Contains(stageRunner.StageName))
                _dataPipelineRun.CompletedStages.Add(stageRunner.StageName);

            if (_dataPipelineRun.ActiveStages.Contains(stageRunner.StageName))
                _dataPipelineRun.ActiveStages.Remove(stageRunner.StageName);

            if (stageRunner.Successful)
            {
                if (!startNextStage)
                    // A global error occurred, so we do not start the next stages.
                    return;

                // Attempt to kick off the next stages.
                var nextStages = _dataPipelineDefinition.GetNextStages(stageRunner.StageName);

                foreach (var nextStage in nextStages)
                {
                    _logger.LogInformation("Initializing stage {StageName} for new data pipeline run {DataPipelineRunId}...",
                        nextStage.Name, _dataPipelineRun.RunId);

                    var stageWorkItems = await GetStageWorkItems(
                        _dataPipelineRun,
                        nextStage,
                        stageRunner,
                        ServiceContext.ServiceIdentity!);

                    await CreateStageRunner(
                        nextStage,
                        stageWorkItems);

                    if (!_dataPipelineRun.ActiveStages.Contains(nextStage.Name))
                        _dataPipelineRun.ActiveStages.Add(nextStage.Name);

                    _logger.LogInformation("Finished initializing stage {StageName} for new data pipeline run {DataPipelineRunId}...",
                        nextStage.Name, _dataPipelineRun.RunId);
                }
            }
            else
            {
                if (!_dataPipelineRun.FailedStages.Contains(stageRunner.StageName))
                    _dataPipelineRun.FailedStages.Add(stageRunner.StageName);
            }
        }
    }
}
