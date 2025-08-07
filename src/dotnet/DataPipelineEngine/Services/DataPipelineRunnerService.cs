using Azure.Storage.Queues;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.DataPipelineEngine.Services.Runners;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.ContentModel;

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

        private readonly Dictionary<string, DataPipelineRunner> _currentRunners = [];
        private readonly object _syncRoot = new();

        /// <summary>
        public Dictionary<string, DataPipelineRunner> CurrentRunners => _currentRunners;

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
                base(TimeSpan.FromSeconds(1), serviceProvider, logger)
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

            var activeDataPipelineRuns =
                await _stateService.GetActiveDataPipelineRuns();

            if (activeDataPipelineRuns.Count > 0)
                _logger.LogInformation("Initializing data pipeline runners for {ActiveRunsCount} existing active runs.",
                    activeDataPipelineRuns.Count);
            else
                _logger.LogInformation("There are no active data pipeline runs.");

            foreach (var activeDataPipelineRun in activeDataPipelineRuns)
            {
                var dataPipelineDefinitionSnapshot =
                    await _dataPipelineResourceProvider.GetResourceAsync<DataPipelineDefinitionSnapshot>(
                        activeDataPipelineRun.DataPipelineObjectId,
                        ServiceContext.ServiceIdentity!);

                await AddRunner(
                    activeDataPipelineRun,
                    (runner) => runner.InitializeExisting(
                        dataPipelineDefinitionSnapshot.DataPipelineDefinition,
                        activeDataPipelineRun));
            }

            if (!await _stateService.StartDataPipelineRunWorkItemProcessing(
                ProcessDataPipelineRunWorkItem))
            {
                _logger.LogError("Failed to start data pipeline run work item processing.");
                throw new DataPipelineServiceException(
                    "Failed to start data pipeline run work item processing.",
                    StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("The {ServiceName} service has been initialized.", ServiceName);
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken) =>
            await _stateService.StopDataPipelineRunWorkItemProcessing();

        private async Task ProcessDataPipelineRunWorkItem(
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!_currentRunners.TryGetValue(dataPipelineRunWorkItem.RunId, out var runner))
            {
                _logger.LogError("Data pipeline run {RunId} not found when processing data pipeline run work item with id {WorkItemId}.",
                    dataPipelineRunWorkItem.RunId, dataPipelineRunWorkItem.Id);
                return;
            }

            await runner.ProcessDataPipelineRunWorkItem(dataPipelineRunWorkItem);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsyncInternal(
            CancellationToken stoppingToken)
        {
            var runIdsToRemove = new List<string>();

            foreach (var item in _currentRunners.Where(cr => cr.Value.Initialized))
            {
                var complete = await item.Value.Completed();
                if (complete)
                {
                    runIdsToRemove.Add(item.Key);
                    _logger.LogInformation("Data pipeline run {RunId} has completed.", item.Key);
                }
            }

            foreach (var runId in runIdsToRemove)
                _currentRunners.Remove(runId);
        }

        /// <inheritdoc/>
        public async Task<bool> CanStartRun(
           DataPipelineRun dataPipelineRun)
        {
            await _initializationTask;

            lock (_syncRoot)
            {
                // This comparer ensures that we do not start a new run that would be incompatible with an existing run.
                var comparer = new DataPipelineRunCanonicalComparer();
                return
                    !_currentRunners.ContainsKey(dataPipelineRun.RunId)
                    && !_currentRunners.Values.Any(r => comparer.Equals(r.DataPipelineRun, dataPipelineRun));
            }
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

            if (contentItems.Count == 0)
            {
                _logger.LogWarning("No content items provided for data pipeline run {RunId}.", dataPipelineRun.RunId);

                dataPipelineRun.Completed = true;
                dataPipelineRun.Successful = true;
                await _stateService.UpdateDataPipelineRunStatus(dataPipelineRun);

                return dataPipelineRun;
            }

            await AddRunner(
                dataPipelineRun,
                (runner) => runner.InitializeNew(
                    dataPipelineRun,
                    contentItems,
                    dataPipelineDefinition,
                    userIdentity));

            return dataPipelineRun;
        }

        private async Task AddRunner(
            DataPipelineRun dataPipelineRun,
            Func<DataPipelineRunner, Task> runnerInitializer)
        {
            DataPipelineRunner dataPipelineRunner;

            lock (_syncRoot)
            {
                // This comparer ensures that we do not start a new run that would be incompatible with an existing run.
                var comparer = new DataPipelineRunCanonicalComparer();
                if (_currentRunners.ContainsKey(dataPipelineRun.RunId)
                    || _currentRunners.Values.Any(r => comparer.Equals(r.DataPipelineRun, dataPipelineRun)))
                {
                    _logger.LogError(
                    "The data pipeline with run id {DataPipelineRunId} and canonical run id {DataPipelineCanonicalRunId} is conflicting with an already existing run and cannot be started.",
                    dataPipelineRun.RunId,
                    dataPipelineRun.CanonicalRunId);
                    throw new DataPipelineServiceException(
                        $"The data pipeline with run id {dataPipelineRun.RunId} and canonical run id {dataPipelineRun.CanonicalRunId} is conflicting with an already existing run and cannot be started.",
                        StatusCodes.Status400BadRequest);
                }

                dataPipelineRunner = new DataPipelineRunner(
                    _stateService,
                    _pluginService,
                    dataPipelineRun.Processor == DataPipelineRunProcessors.Frontend
                        ? _frontendQueueClient
                        : _backendQueueClient,
                    _serviceProvider);

                _currentRunners[dataPipelineRun.RunId] = dataPipelineRunner;
            }

            try
            {
                await runnerInitializer(dataPipelineRunner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize data pipeline runner for run {RunId}.", dataPipelineRun.RunId);

                lock (_syncRoot)
                {
                    // Remove from the current runners if it was added.
                    if (_currentRunners.ContainsKey(dataPipelineRun.RunId))
                        _currentRunners.Remove(dataPipelineRun.RunId);
                }

                throw new DataPipelineServiceException(
                    $"Failed to initialize data pipeline runner for run {dataPipelineRun.RunId}.",
                    ex,
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
