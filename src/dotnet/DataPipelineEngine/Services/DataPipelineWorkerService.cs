using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Constants.Telemetry;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Common.Tasks;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.DataPipelineEngine.Services.Queueing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilites for background processing of data pipeline work items.
    /// </summary>
    public class DataPipelineWorkerService : BackgroundService
    {
        private readonly IDataPipelineStateService _stateService;
        private readonly IResourceProviderService _dataPipelineResourceProvider;
        private readonly IResourceProviderService _pluginResourceProvider;
        private readonly IPluginService _pluginService;
        private readonly DataPipelineWorkerServiceSettings _settings;
        private readonly ILogger<DataPipelineWorkerService> _logger;

        private readonly string _serviceName = "Data Pipeline Worker Service";

        private readonly ProcessingPayloadsRegistry<DataPipelineRunWorkItemMessage> _payloadsRegistry;
        private readonly TaskPool _taskPool;

        private const int MAX_FAILED_PROCESSING_ATTEMPTS = 10;

        private const int AGGRESSIVE_CYCLE_TIME_MILLISECONDS = 100;
        private const int NORMAL_CYCLE_TIME_MILLISECONDS = 1000;

        /// <summary>
        /// Initializes a new instance of the service.
        /// </summary>
        /// <param name="stateService">The Data Pipeline State service providing state management services.</param>
        /// <param name="resourceProviderServices">The FoundationaLLM resource provider services.</param>
        /// <param name="queueService">The message queue service providing queueing capabilities.</param>
        /// <param name="options">The options with the service settings.</param>
        /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
        /// <param name="logger">The logger used for logging.</param>
        public DataPipelineWorkerService(
            IDataPipelineStateService stateService,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IMessageQueueService<DataPipelineRunWorkItemMessage> queueService,
            IOptions<DataPipelineWorkerServiceSettings> options,
            IServiceProvider serviceProvider,
            ILogger<DataPipelineWorkerService> logger)
        {
            _stateService = stateService;
            _dataPipelineResourceProvider = resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);
            _settings = options.Value;
            _logger = logger;

            _payloadsRegistry = new ProcessingPayloadsRegistry<DataPipelineRunWorkItemMessage>(
                queueService,
                _logger);

            _pluginResourceProvider = resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
            _pluginService = new PluginService(
                _pluginResourceProvider,
                serviceProvider);

            _taskPool = new TaskPool(
                _settings.ParallelProcessorsCount,
                _logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The {ServiceName} service is starting...", _serviceName);

            _logger.LogInformation("The {ServiceName} service is waiting for the resource providers to initialize...", _serviceName);

            await Task.WhenAll(
                _dataPipelineResourceProvider.InitializationTask,
                _pluginResourceProvider.InitializationTask);

            _logger.LogInformation("The {ServiceName} service has all the resource providers properly initialized.", _serviceName);

            // Starting with an aggressive cycle time of 100 milliseconds to ensure quick response to new messages.
            var cycleTimeMilliseconds = AGGRESSIVE_CYCLE_TIME_MILLISECONDS;
            var mostRecentAvailableWorkTime = DateTimeOffset.MinValue;
            _logger.LogInformation("The {ServiceName} service is using a processing cycle time of {CycleTimeMilliseconds} milliseconds.",
                _serviceName, cycleTimeMilliseconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var taskPoolAvailableCapacity = _taskPool.AvailableCapacity;

                    if (taskPoolAvailableCapacity > 0
                        && (await _payloadsRegistry.NewPayloadsAvailable()))
                    {
                        mostRecentAvailableWorkTime = DateTimeOffset.UtcNow;
                        if (cycleTimeMilliseconds != AGGRESSIVE_CYCLE_TIME_MILLISECONDS)
                        {
                            cycleTimeMilliseconds = AGGRESSIVE_CYCLE_TIME_MILLISECONDS; // Ensure cycle time is set to aggressive value.
                            _logger.LogInformation("The {ServiceName} service is switching to a processing cycle time of {CycleTimeMilliseconds} milliseconds.",
                                _serviceName, cycleTimeMilliseconds);
                        }

                        // Ensure the messages that are being processed have their visibility timeout updated
                        await _payloadsRegistry.ExtendPayloadsProcessingTime(
                            _taskPool.ActivePayloadIds);

                        // We have available capacity in the task pool and there are messages in the queue.
                        var dequeuedPayloads = await _payloadsRegistry.ReceivePayloadsForProcessing(
                            taskPoolAvailableCapacity,
                            p => p.WorkItemId);

                        #region Check for processing attempts that must be stoppped because of too many failures

                        var validPayloads =
                            new List<(DataPipelineRunWorkItemMessage Message, DataPipelineRunWorkItem WorkItem)>();

                        foreach (var dequeuedPayload in dequeuedPayloads)
                        {
                            var dataPipelineRunWorkItem =
                                await _stateService.GetDataPipelineRunWorkItem(
                                    dequeuedPayload.WorkItemId,
                                    dequeuedPayload.RunId);

                            if (dataPipelineRunWorkItem!.FailedProcessingAttempts >=
                                MAX_FAILED_PROCESSING_ATTEMPTS)
                            {
                                // The message has been processed too many times, we will stop processing it.
                                dataPipelineRunWorkItem.Errors.Add("Too many failed processing attempts.");
                                dataPipelineRunWorkItem.Completed = true;
                                dataPipelineRunWorkItem.Successful = false;
                                await _stateService.UpdateDataPipelineRunWorkItemsStatus(
                                    [dataPipelineRunWorkItem]);

                                await _payloadsRegistry.RemovePayload(
                                    dequeuedPayload.WorkItemId);
                            }
                            else
                                validPayloads.Add(
                                    (dequeuedPayload, dataPipelineRunWorkItem));
                        }

                        #endregion

                        #region Check for newly dequeued payloads that are still processing

                        // If this happens, there is likely an issue in the code that is processing the payloads.
                        // When a payload that is still being processed is dequeued,
                        // it means that when the processing completes, there will be an error trying to cleanup the associated payload
                        // artifacts (e.g. deleting the message from the queue).
                        // This situation is considered an error and the payload will be ignored.

                        var ignoredPayloadIds = validPayloads
                            .Where(m => _taskPool.HasRunningTaskForPayload(m.Message.WorkItemId))
                            .Select(m => m.Message.WorkItemId)
                            .ToList();

                        if (ignoredPayloadIds.Count > 0)
                        {
                            _logger.LogError("The following payloads were dequeued while still being processed: {IgnoredPayloadIds}. The payloads will be ignored.",
                                string.Join(",", ignoredPayloadIds));
                            await _payloadsRegistry.IgnorePayloads(ignoredPayloadIds);
                        }

                        #endregion

                        // Add the request to the task pool for processing
                        // No need to use ConfigureAwait(false) since the code is going to be executed on a
                        // thread pool thread, with no user code higher on the stack (for details, see
                        // https://devblogs.microsoft.com/dotnet/configureawait-faq/).
                        _taskPool.Add(validPayloads
                            .Where(m => !_taskPool.HasRunningTaskForPayload(m.Message.WorkItemId))
                            .Select(m => new TaskInfo
                            {
                                PayloadId = m.WorkItem.Id,
                                Task = ProcessPayload(
                                    m.Message,
                                    m.WorkItem),
                                StartTime = DateTimeOffset.UtcNow
                            }));
                    }

                    if ((DateTimeOffset.UtcNow - mostRecentAvailableWorkTime).TotalSeconds > 300
                        && cycleTimeMilliseconds != NORMAL_CYCLE_TIME_MILLISECONDS)
                    {
                        // If there are no messages available for the last 300 seconds, increase the cycle time to reduce the load on the queue.
                        cycleTimeMilliseconds = NORMAL_CYCLE_TIME_MILLISECONDS;
                        _logger.LogInformation("The {ServiceName} service is switching to a processing cycle time of {CycleTimeMilliseconds} milliseconds because there is no work available.",
                            _serviceName, cycleTimeMilliseconds);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(cycleTimeMilliseconds), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Ignore the exception, the service is stopping.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error while processing data pipeline run work items.");
                }
            }

            _logger.LogInformation("The {ServiceName} service is stopping.", _serviceName);
        }

        private async Task ProcessPayload(
            DataPipelineRunWorkItemMessage payload,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            var successfulProcessing = true;
            dataPipelineRunWorkItem.ProcessingAttempts++;

            try
            {
                var result =
                    await InvokeDataPipelineStagePlugin(dataPipelineRunWorkItem);

                if (result.Success)
                {
                    dataPipelineRunWorkItem.Completed = true;
                    dataPipelineRunWorkItem.Successful = true;
                }
                else
                {
                    dataPipelineRunWorkItem.Errors.Add(result.ErrorMessage
                        ?? "The data pipeline stage plugin encountered an error while processing the data pipeline run work item.");
                    dataPipelineRunWorkItem.FailedProcessingAttempts++;

                    if (result.StopProcessing)
                    {
                        // The plugin has identified a permanent error that will not be resolved by retrying the operation.
                        // This is considered a processing success.
                        dataPipelineRunWorkItem.Completed = true;
                    }
                    else
                        successfulProcessing = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the data pipeline run work item with id {WorkItemId}.",
                    dataPipelineRunWorkItem.Id);
                successfulProcessing = false;
                dataPipelineRunWorkItem.Errors.Add(ex.Message);
                dataPipelineRunWorkItem.FailedProcessingAttempts++;
            }

            try
            {
                await _stateService.UpdateDataPipelineRunWorkItem(
                    dataPipelineRunWorkItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the status for the data pipeline run work item with id {WorkItemId}.",
                    dataPipelineRunWorkItem.Id);
                successfulProcessing = false;
            }

            if (successfulProcessing)
                await _payloadsRegistry.RemovePayload(
                    payload.WorkItemId);
            else
            {
                // An error occured, but it's not one that results in preventing future processing attempts.
                // Extend the payload processing time with a flag to recover from error,
                // and remove the payload from processing (without deleting the underlying message) so it can be retried.
                await _payloadsRegistry.ExtendPayloadsProcessingTime(
                    [payload.WorkItemId],
                    recoverFromError: true);
                await _payloadsRegistry.RemovePayload(
                    payload.WorkItemId,
                    deleteMessage: false);
            }
        }

        #region Plugin management

        private async Task<PluginResult> InvokeDataPipelineStagePlugin(
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            var dataPipelineRun =
                await _stateService.GetDataPipelineRun(dataPipelineRunWorkItem.RunId);

            var dataPipelineSnapshot =
                await _dataPipelineResourceProvider.GetResourceAsync<DataPipelineDefinitionSnapshot>(
                    dataPipelineRun!.DataPipelineObjectId,
                    ServiceContext.ServiceIdentity!);

            var dataPipeline = dataPipelineSnapshot.DataPipelineDefinition;

            var dataPipelineStage = dataPipeline.GetStage(dataPipelineRunWorkItem.Stage);

            var dataPipelineStagePlugin = await _pluginService.GetDataPipelineStagePlugin(
                dataPipelineRun.InstanceId,
                dataPipelineStage.PluginObjectId,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"Stage.{dataPipelineRunWorkItem.Stage}."),
                ServiceContext.ServiceIdentity!);

            using var telemetryActivity = ServiceContext.TelemetryActivitySource.StartActivity(
                TelemetryActivityNames.DataPipelineWorkerService_Stage_ProcessWorkItem,
                ActivityKind.Internal,
                parentContext: default,
                tags: new Dictionary<string, object?>
                {
                    { TelemetryActivityTagNames.InstanceId, dataPipelineRun.InstanceId },
                    { TelemetryActivityTagNames.DataPipelineRunId, dataPipelineRun.RunId },
                    { TelemetryActivityTagNames.DataPipelineRunStage, dataPipelineRunWorkItem.Stage },
                    { TelemetryActivityTagNames.DataPipelineRunWorkItemId, dataPipelineRunWorkItem.Id }
                });

            var result =
                await dataPipelineStagePlugin.ProcessWorkItem(
                    dataPipeline,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

            return result;
        }

        #endregion
    }
}
