using Azure.Storage.Queues;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Common.Tasks;
using FoundationaLLM.DataPipelineEngine.Models;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

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

        private readonly QueueClient _queueClient;
        private readonly TaskPool _taskPool;

        private const int MESSAGE_VISIBILITY_TIMEOUT_SECONDS = 600;
        private const int MESSAGE_ERROR_VISIBILITY_TIMEOUT_SECONDS = 5;
        private const int MAX_FAILED_PROCESSING_ATTEMPTS = 10;

        /// <summary>
        /// Initializes a new instance of the service.
        /// </summary>
        /// <param name="stateService">The Data Pipeline State service providing state management services.</param>
        /// <param name="resourceProviderServices">The FoundationaLLM resource provider services.</param>
        /// <param name="options">The options with the service settings.</param>
        /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
        /// <param name="logger">The logger used for logging.</param>
        public DataPipelineWorkerService(
            IDataPipelineStateService stateService,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IOptions<DataPipelineWorkerServiceSettings> options,
            IServiceProvider serviceProvider,
            ILogger<DataPipelineWorkerService> logger)
        {
            _stateService = stateService;
            _dataPipelineResourceProvider = resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);
            _settings = options.Value;
            _logger = logger;

            _pluginResourceProvider = resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
            _pluginService = new PluginService(
                _pluginResourceProvider,
                serviceProvider);

            var queueServiceClient = new QueueServiceClient(
                new Uri($"https://{_settings.Storage.AccountName}.queue.core.windows.net"),
                ServiceContext.AzureCredential);
            _queueClient = queueServiceClient.GetQueueClient(_settings.Queue);

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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var taskPoolAvailableCapacity = _taskPool.AvailableCapacity;

                    if (taskPoolAvailableCapacity > 0
                        && (await WorkItemMessagesAvailable().ConfigureAwait(false)))
                    {
                        // We have available capacity in the task pool and there are messages in the queue.
                        var dequeuedMessages =
                            await ReceiveWorkItemMessages(taskPoolAvailableCapacity).ConfigureAwait(false);
                        var validMessages =
                            new List<(DequeuedMessage Message, DataPipelineRunWorkItem WorkItem)>();

                        foreach (var dequeuedMessage in dequeuedMessages)
                        {
                            var dataPipelineRunWorkItem =
                                await _stateService.GetDataPipelineRunWorkItem(
                                    dequeuedMessage.Message.WorkItemId,
                                    dequeuedMessage.Message.RunId);

                            if (dataPipelineRunWorkItem!.FailedProcessingAttempts ==
                                MAX_FAILED_PROCESSING_ATTEMPTS)
                            {
                                // The message has been processed too many times, we will stop processing it.
                                dataPipelineRunWorkItem.Errors.Add("Too many failed processing attempts.");
                                dataPipelineRunWorkItem.Completed = true;
                                dataPipelineRunWorkItem.Successful = false;
                                await _stateService.UpdateDataPipelineRunWorkItemsStatus(
                                    [dataPipelineRunWorkItem]);

                                await DeleteWorkItemMessage(
                                    dequeuedMessage.MessageId,
                                    dequeuedMessage.PopReceipt);
                            }
                            else
                                validMessages.Add(
                                    (dequeuedMessage, dataPipelineRunWorkItem));
                        }

                        // Add the request to the task pool for processing
                        // No need to use ConfigureAwait(false) since the code is going to be executed on a
                        // thread pool thread, with no user code higher on the stack (for details, see
                        // https://devblogs.microsoft.com/dotnet/configureawait-faq/).
                        _taskPool.Add(validMessages
                            .Select(m => new TaskInfo
                            {
                                PayloadId = m.WorkItem.Id,
                                Task = ProcessMessage(m.Message, m.WorkItem),
                                StartTime = DateTimeOffset.UtcNow
                            }));
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
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

        private async Task ProcessMessage(
            DequeuedMessage message,
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
                    dataPipelineRunWorkItem.OutputArtifactId = result.Value;
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
            {
                // The message was processed successfully, we can delete it from the queue.
                await DeleteWorkItemMessage(
                    message.MessageId,
                    message.PopReceipt);
            }
            else
            {
                // Make the request visible again in the queue to
                // allow the it to be processed again after a short delay (in case of transient errors).
                var success = await TryUpdateWorkItemMessage(
                    message.MessageId,
                    message.PopReceipt,
                    TimeSpan.FromSeconds(MESSAGE_ERROR_VISIBILITY_TIMEOUT_SECONDS)).ConfigureAwait(false);
                if (!success)
                    _logger.LogWarning("Could not update the visibility timeout of the data pipeline run work item message with id {MessageId} and work item id {WorkItemId}.",
                        message.MessageId, dataPipelineRunWorkItem.Id);
            }
        }

        #region Queue messages management

        private async Task<bool> WorkItemMessagesAvailable()
        {
            try
            {
                var message = await _queueClient.PeekMessageAsync().ConfigureAwait(false);
                return message.Value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while attempting to peek at messages in queue {QueueName}.",
                    _settings.Queue);
                return false;
            }
        }

        private async Task<IEnumerable<DequeuedMessage>> ReceiveWorkItemMessages(int count)
        {
            var receivedMessages = await _queueClient.ReceiveMessagesAsync(
                count,
                TimeSpan.FromSeconds(MESSAGE_VISIBILITY_TIMEOUT_SECONDS)).ConfigureAwait(false);

            var result = new List<DequeuedMessage>();

            if (receivedMessages.HasValue)
            {
                foreach (var m in receivedMessages.Value)
                {
                    try
                    {
                        result.Add(new DequeuedMessage()
                        {
                            Message = JsonSerializer.Deserialize<DataPipelineRunWorkItemMessage>(m.Body)!,
                            MessageId = m.MessageId,
                            PopReceipt = m.PopReceipt!,
                            DequeueCount = m.DequeueCount
                        });

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cannot deserialize message with id {MessageId}.", m.MessageId);
                    }
                }
            }

            return result;
        }

        private async Task<bool> TryUpdateWorkItemMessage(
            string messageId,
            string popReceipt,
            TimeSpan visibilityTimeout)
        {
            try
            {
                await _queueClient.UpdateMessageAsync(
                    messageId,
                    popReceipt,
                    visibilityTimeout: visibilityTimeout).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message with id {MessageId}.", messageId);
                return false;
            }
        }

        private async Task DeleteWorkItemMessage(
            string messageId,
            string popReceipt)
        {
            var response = await _queueClient.DeleteMessageAsync(messageId, popReceipt).ConfigureAwait(false);
            if (response.IsError)
            {
                _logger.LogError("Error deleting message with id {MessageId}.", messageId);
            }
            else
            {
                _logger.LogInformation("Message with id {MessageId} deleted.", messageId);
            }
        }

        #endregion

        #region Plugin management

        private async Task<PluginResult<string>> InvokeDataPipelineStagePlugin(
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
