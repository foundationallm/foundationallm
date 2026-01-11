using Azure.Messaging;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.Quota;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Environment;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Quota;
using FoundationaLLM.Common.Services.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements the FoundationaLLM quota service.
    /// </summary>
    public class QuotaService : IQuotaService
    {
        private readonly DependencyInjectionContainerSettings _dependencyInjectionContainerSettings;
        private readonly string _serviceIdentifier;

        private const string QUOTA_SERVICE_NAME = "QuotaService";
        private const string STORAGE_CONTAINER_NAME = "quota";
        private const string QUOTA_STORE_FILE_PATH = "/quota-store.json";
        private readonly QuotaMetricPartitionState QUOTA_NOT_EXCEEDED_EVALUATION_RESULT = new()
        {
            QuotaName = "N/A",
            QuotaContext = "N/A",
            QuotaMetricPartitionId = "N/A"
        };

        private DateTimeOffset _initializationStartTime;
        private bool _isInitialized = false;
        // The service is enabled by default.
        // Once initialization completes, the service will be disabled if there are no quota definitions in the quota store.
        // While initialization is in progress, the service is enabled to make sure we can handle the situation where initialization fails.
        private bool _enabled = true;
        private readonly TaskCompletionSource<bool> _initializationTaskCompletionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly IStorageService _storageService;
        private readonly IEventService _eventService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<QuotaService> _logger;

        private List<QuotaDefinition> _quotaDefinitions = [];
        private Dictionary<string, QuotaContextBase> _quotaContexts = [];

        private readonly object _distributedEnfocementQueueLock = new();
        private LocalEventService? _localEventService;
        // Since we are in a pure producer-consumer scenario that does not have a dedicated producer thread, we use a Queue instead of a ConcurrentQueue.
        // For details, see https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/when-to-use-a-thread-safe-collection#concurrentqueuet-vs-queuet.
        private readonly Queue<DistributedQuotaEnforcementEventData> _distributedEnforcementQueue = [];

        /// <inheritdoc/>
        public bool Enabled => true;

        /// <inheritdoc/>
        public Task<bool> InitializationTask => _initializationTaskCompletionSource.Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotaService"/> class.
        /// </summary>
        /// <param name="dependencyInjectionContainerSettings">The <see cref="DependencyInjectionContainerSettings"/> providing the configuration of the dependency injection container.</param>
        /// <param name="storageService">The storage service used for storing quota configuration.</param>
        /// <param name="eventService">The <see cref="IEventService"/> providing event services to the quota service.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        public QuotaService(
            DependencyInjectionContainerSettings dependencyInjectionContainerSettings,
            IStorageService storageService,
            IEventService eventService,
            ILoggerFactory loggerFactory)
        {
            _dependencyInjectionContainerSettings = dependencyInjectionContainerSettings;
            _serviceIdentifier = $"{_dependencyInjectionContainerSettings.Id:D3}";
            _storageService = storageService;
            _eventService = eventService;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<QuotaService>();

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

        #region Initialization

        private async Task Initialize()
        {
            try
            {
                _logger.LogInformation("[QuotaService {ServiceIdentifier}] Starting to initialize the quota service ...",
                    _serviceIdentifier);
                _initializationStartTime = DateTimeOffset.UtcNow;

                await LoadQuotaDefinitions();

                _quotaContexts = _quotaDefinitions
                    .Select(qd => qd.MetricPartition switch
                    {
                        QuotaMetricPartitionType.None => (new SinglePartitionQuotaContext(
                            _serviceIdentifier, qd, _loggerFactory.CreateLogger<SinglePartitionQuotaContext>())) as QuotaContextBase,
                        QuotaMetricPartitionType.UserIdentifier => (new UserIdentifierQuotaContext(
                            _serviceIdentifier, qd, _loggerFactory.CreateLogger<UserIdentifierQuotaContext>())) as QuotaContextBase,
                        QuotaMetricPartitionType.UserPrincipalName => (new UserPrincipalNameQuotaContext(
                            _serviceIdentifier, qd, _loggerFactory.CreateLogger<UserPrincipalNameQuotaContext>())) as QuotaContextBase,
                        _ => throw new QuotaException($"Unsupported metric partition: {qd.MetricPartition}")
                    })
                    .ToDictionary(qc => qc.Quota.Context);

                InitializeLocalEventService();

                _isInitialized = true;
                _enabled = _quotaDefinitions.Count > 0;

                _logger.LogInformation("[QuotaService {ServiceIdentifier}] The quota service was successfully initialized.",
                    _serviceIdentifier);

                if (!_enabled)
                    _logger.LogWarning("[QuotaService {ServiceIdentifier}] The quota service is disabled because there are no quota definitions in the quota store.",
                        _serviceIdentifier);

                _initializationTaskCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                _initializationTaskCompletionSource.SetResult(false);
                _logger.LogError(ex, "[QuotaService {ServiceIdentifier}] The quota service failed to initialize.",
                    _serviceIdentifier);
            }
        }

        private async Task LoadQuotaDefinitions()
        {
            if (await _storageService.FileExistsAsync(
                    STORAGE_CONTAINER_NAME,
                    QUOTA_STORE_FILE_PATH,
                    default))
            {
                var fileContent = await _storageService.ReadFileAsync(
                    STORAGE_CONTAINER_NAME,
                    QUOTA_STORE_FILE_PATH,
                    default);
                _quotaDefinitions = JsonSerializer.Deserialize<List<QuotaDefinition>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()))!;
            }
            else
            {
                // The quota store file does not exist, so create it.
                await _storageService.WriteFileAsync(
                    STORAGE_CONTAINER_NAME,
                    QUOTA_STORE_FILE_PATH,
                    JsonSerializer.Serialize(_quotaDefinitions),
                    default,
                    default);
            }
        }

        private void InitializeLocalEventService()
        {
            if (!_quotaDefinitions.Any(qd => qd.DistributedEnforcement))
            {
                // The quota service does not have any quota definitions that require distributed enforcement.
                // Therefore, the local event service is not needed.
                _logger.LogInformation("[QuotaService {ServiceIdentifier}] The local event service is not configured because there are not quotas with distributed enforcement.",
                    _serviceIdentifier);
                return;
            }

            _localEventService = new LocalEventService(
                new LocalEventServiceSettings { EventProcessingCycleSeconds = 5 },
                _eventService,
                _loggerFactory.CreateLogger<LocalEventService>());
            _localEventService.SubscribeToEventTypes(
                [
                    EventTypes.FoundationaLLM_Quota_MetricUpdate
                ]);
            _localEventService.StartLocalEventProcessing(HandleEvents);

            // Kicks off the continuous loop to send distributed enforcement events.
            _ = Task.Run(SendDistributedEnforcementEvents);
        }

        /// <summary>
        /// Checks if the initialization is pending.
        /// </summary>
        /// <remarks>
        /// Initialization is considered to be pending if the service is not yet initialized and the time since initialization started is less than 30 seconds.
        /// If the service is not yet initialized and the time since initialization started is greater than 30 seconds, the service is considered to be in a failed state.
        /// This will result in an exception to be thrown when the service is used.
        /// </remarks>
        /// <returns>True if the service is initialized or initialization is still pending, False otherwise.</returns>
        private bool InitializationPending()
        {
            if (_isInitialized)
                return false;

            if ((DateTimeOffset.UtcNow - _initializationStartTime).TotalSeconds < 60)
                return true;

            throw
                new QuotaException("The quota service failed to initialize within the required time interval of 60 seconds.");
        }

        #endregion

        #region Event Handling

        private async Task HandleEvents(EventTypeEventArgs e)
        {
            // If the quota service doesn't have any events to process, return.
            if (e.Events.Count == 0)
                return;

            var originalEventCount = e.Events.Count;

            // Only process events that are targeted to the quota service.
            var eventsToProcess = e.Events
                .Where(e => e.Subject == QUOTA_SERVICE_NAME).ToList();

            _logger.LogDebug("[QuotaService {ServiceIdentifier}] {EventsCount} events of type {EventType} received out of which {QuotaServiceEventsCount} are targeted for the quota service.",
                _serviceIdentifier,
                originalEventCount,
                e.EventType,
                eventsToProcess.Count);

            // If the quota service doesn't have any events to process, return.
            if (eventsToProcess.Count == 0)
                return;

            // Handle the supported events here.
            switch (e.EventType)
            {
                case EventTypes.FoundationaLLM_Quota_MetricUpdate:
                    HandleQuotaMetricUpdates(eventsToProcess);
                    break;
                default:
                    _logger.LogWarning("[QuotaService {ServiceIdentifier}] The quota service does not handle events of type {EventType}.",
                        _serviceIdentifier,
                        e.EventType);
                    break;
            }

            await Task.CompletedTask;
        }

        private void HandleQuotaMetricUpdates(List<CloudEvent> quotaMetricUpdateEvents)
        {
            foreach(var eventData in quotaMetricUpdateEvents
                .Select(qmue => qmue.Data?.ToObjectFromJson<DistributedQuotaEnforcementEventData>())
                .Where(x => x != null))
            {
                UpdateQuotaContextForDistributedEnforcement(eventData!);
            }
        }

        /// <summary>
        /// Runs a continuous loop to send distributed enforcement events.
        /// </summary>
        private async Task SendDistributedEnforcementEvents()
        {
            if (_eventService == null)
            {
                _logger.LogWarning("[QuotaService {ServiceIdentifier}] The quota service does not have an event service configured and cannot send events.",
                    _serviceIdentifier);
                return;
            }

            _logger.LogInformation("[QuotaService {ServiceIdentifier}] The quota service has started sending distributed enforcement events.",
                _serviceIdentifier);

            while (true)
            {
                try
                {
                    var dequeuedEvents = new List<DistributedQuotaEnforcementEventData>();

                    // To minimize the time the lock is held, dequeue all events and then send them.
                    // This is a best-effort approach and does not guarantee that all events will be sent (which is acceptable in this scenario).
                    lock (_distributedEnfocementQueueLock)
                    {
                        while (_distributedEnforcementQueue.TryDequeue(out var eventData))
                            dequeuedEvents.Add(eventData);
                    }

                    // Group the events by quota context and quota metric partition id
                    // to minimize the number of events sent.
                    var dataToSend = dequeuedEvents.GroupBy(
                        de => new
                        {
                            de.QuotaContext,
                            de.QuotaMetricPartitionId
                        },
                        de => de,
                        (key, group) => new DistributedQuotaEnforcementEventData
                        {
                            QuotaContext = key.QuotaContext,
                            QuotaMetricPartitionId = key.QuotaMetricPartitionId,
                            EventTimestamps = [.. group.SelectMany(g => g.EventTimestamps)]
                        }).ToList();

                    if (dataToSend.Count > 0)
                    {
                        _logger.LogDebug("[QuotaService {ServiceIdentifier}] Starting to send {EventCount} distributed enforcement events.",
                            _serviceIdentifier,
                            dataToSend.Count);

                        var eventsSent = 0;

                        foreach (var data in dataToSend)
                        {
                            // The CloudEvent source is automatically filled in by the event service.
                            await _eventService.SendEvent(
                                EventGridTopics.FoundationaLLM_API_Statistics,
                                new CloudEvent(
                                    string.Empty,
                                    EventTypes.FoundationaLLM_Quota_MetricUpdate,
                                    data,
                                    typeof(DistributedQuotaEnforcementEventData))
                                {
                                    Subject = QUOTA_SERVICE_NAME
                                });

                            eventsSent++;
                        }

                        _logger.LogDebug("[QuotaService {ServiceIdentifier}] Successfully sent {EventCount} distributed enforcement events.",
                            _serviceIdentifier,
                            eventsSent);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[QuotaService {ServiceIdentifier}] An error occurred while sending distributed enforcement events.",
                        _serviceIdentifier);
                }
            }
        }

        private void RegisterForDistributedEnforcement(
            DateTimeOffset referenceTime,
            string context,
            string quotaMetricPartitionId)
        {
            lock (_distributedEnfocementQueueLock)
            {
                _distributedEnforcementQueue.Enqueue(
                    new DistributedQuotaEnforcementEventData
                    {
                        QuotaContext = context,
                        QuotaMetricPartitionId = quotaMetricPartitionId,
                        EventTimestamps = [referenceTime]
                    });
            }
        }

        #endregion

        /// <inheritdoc/>
        public QuotaMetricPartitionState EvaluateRawRequestForQuota(
            string apiName,
            string? controllerName,
            UnifiedUserIdentity? userIdentity)
        {
            if (!_isInitialized)
                if (InitializationPending())
                    return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;

            var referenceTime = DateTimeOffset.UtcNow;
            var context = BuildContext([apiName, controllerName]);

            return UpdateQuotaContext(referenceTime, context, userIdentity);
        }

        /// <inheritdoc/>
        public QuotaMetricPartitionState EvaluateCompletionRequestForQuota(
            string apiName,
            string controllerName,
            UnifiedUserIdentity? userIdentity,
            CompletionRequest completionRequest)
        {
            if (!_isInitialized)
                if (InitializationPending())
                    return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;

            var referenceTime = DateTimeOffset.UtcNow;
            var context = BuildContext([apiName, controllerName, completionRequest?.AgentName]);

            return UpdateQuotaContext(referenceTime, context, userIdentity);
        }

        private static string BuildContext(string?[] tokens) =>
            string.Join(":", [.. tokens.Select(t => string.IsNullOrWhiteSpace(t) ? "__default__" : t)]);

        private QuotaMetricPartitionState UpdateQuotaContext(
            DateTimeOffset referenceTime,
            string context,
            UnifiedUserIdentity? userIdentity)
        {
            var userIdentifier = userIdentity?.UserId ?? "__default__";
            var userPrincipalName = userIdentity?.UPN ?? "__default__";
            var evaluationResult = QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;

            if (_quotaContexts.TryGetValue(context, out var quotaContext))
            {
                evaluationResult = quotaContext.AddLocalMetricUnit(
                    referenceTime,
                    userIdentifier,
                    userPrincipalName);

                if (quotaContext.Quota.DistributedEnforcement)
                    RegisterForDistributedEnforcement(
                        referenceTime,
                        context,
                        evaluationResult.QuotaMetricPartitionId);
            }
            else
                _logger.LogWarning("[QuotaService {ServiceIdentifier}] The quota context {QuotaContext} does not exist in the quota service.",
                    _serviceIdentifier,
                    context);

            return evaluationResult;
        }

        private void UpdateQuotaContextForDistributedEnforcement(
            DistributedQuotaEnforcementEventData data)
        {
            try
            {
                _logger.LogDebug(string.Join(
                    Environment.NewLine,
                    [
                        "----------------------------------------",
                        "[QuotaService {ServiceIdentifier}] Updating a quota context with remote data.",
                        "Quota context: {QuotaContext}, Metric partition id: {MetricPartitionId}",
                        "Timestamps: {Timestamps}"
                    ]),
                    _serviceIdentifier,
                    data.QuotaContext,
                    data.QuotaMetricPartitionId,
                    data.EventTimestamps);

                if (_quotaContexts.TryGetValue(data.QuotaContext, out var quotaContext))
                    quotaContext.AddRemoteMetricUnits(
                        data.EventTimestamps,
                        data.QuotaMetricPartitionId);
                else
                    _logger.LogWarning("[QuotaService {ServiceIdentifier}] The quota context {QuotaContext} does not exist in the quota service.",
                        _serviceIdentifier,
                        data.QuotaContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[QuotaService {ServiceIdentifier}] An error occurred while updating the quota context for distributed enforcement (quota context: {QuotaContext}, metric partition id: {MetricPartitionId})",
                    _serviceIdentifier,
                    data.QuotaContext,
                    data.QuotaMetricPartitionId);
            }
        }

        #region Quota Definition CRUD Operations

        /// <inheritdoc/>
        public async Task<List<QuotaDefinition>> GetQuotaDefinitionsAsync()
        {
            await EnsureInitialized();
            return [.. _quotaDefinitions];
        }

        /// <inheritdoc/>
        public async Task<QuotaDefinition?> GetQuotaDefinitionAsync(string name)
        {
            await EnsureInitialized();
            return _quotaDefinitions.FirstOrDefault(qd => qd.Name == name);
        }

        /// <inheritdoc/>
        public async Task<QuotaDefinition> UpsertQuotaDefinitionAsync(QuotaDefinition quotaDefinition)
        {
            await EnsureInitialized();

            var existingIndex = _quotaDefinitions.FindIndex(qd => qd.Name == quotaDefinition.Name);
            if (existingIndex >= 0)
            {
                _quotaDefinitions[existingIndex] = quotaDefinition;
                _logger.LogInformation("[QuotaService {ServiceIdentifier}] Updated quota definition '{QuotaName}'.",
                    _serviceIdentifier, quotaDefinition.Name);
            }
            else
            {
                _quotaDefinitions.Add(quotaDefinition);
                _logger.LogInformation("[QuotaService {ServiceIdentifier}] Created quota definition '{QuotaName}'.",
                    _serviceIdentifier, quotaDefinition.Name);
            }

            await SaveQuotaDefinitions();
            await RebuildQuotaContexts();

            return quotaDefinition;
        }

        /// <inheritdoc/>
        public async Task DeleteQuotaDefinitionAsync(string name)
        {
            await EnsureInitialized();

            var removed = _quotaDefinitions.RemoveAll(qd => qd.Name == name);
            if (removed > 0)
            {
                _logger.LogInformation("[QuotaService {ServiceIdentifier}] Deleted quota definition '{QuotaName}'.",
                    _serviceIdentifier, name);
                await SaveQuotaDefinitions();
                await RebuildQuotaContexts();
            }
            else
            {
                _logger.LogWarning("[QuotaService {ServiceIdentifier}] Quota definition '{QuotaName}' not found for deletion.",
                    _serviceIdentifier, name);
            }
        }

        private async Task SaveQuotaDefinitions()
        {
            await _storageService.WriteFileAsync(
                STORAGE_CONTAINER_NAME,
                QUOTA_STORE_FILE_PATH,
                JsonSerializer.Serialize(_quotaDefinitions),
                default,
                default);
        }

        private async Task RebuildQuotaContexts()
        {
            _quotaContexts = _quotaDefinitions
                .Select(qd => qd.MetricPartition switch
                {
                    QuotaMetricPartitionType.None => (new SinglePartitionQuotaContext(
                        _serviceIdentifier, qd, _loggerFactory.CreateLogger<SinglePartitionQuotaContext>())) as QuotaContextBase,
                    QuotaMetricPartitionType.UserIdentifier => (new UserIdentifierQuotaContext(
                        _serviceIdentifier, qd, _loggerFactory.CreateLogger<UserIdentifierQuotaContext>())) as QuotaContextBase,
                    QuotaMetricPartitionType.UserPrincipalName => (new UserPrincipalNameQuotaContext(
                        _serviceIdentifier, qd, _loggerFactory.CreateLogger<UserPrincipalNameQuotaContext>())) as QuotaContextBase,
                    _ => throw new QuotaException($"Unsupported metric partition: {qd.MetricPartition}")
                })
                .ToDictionary(qc => qc.Quota.Context);

            _enabled = _quotaDefinitions.Count > 0;
            await Task.CompletedTask;
        }

        private async Task EnsureInitialized()
        {
            if (!_isInitialized)
            {
                var result = await _initializationTaskCompletionSource.Task;
                if (!result)
                    throw new QuotaException("The quota service failed to initialize.");
            }
        }

        #endregion

        #region Quota Usage Metrics

        /// <inheritdoc/>
        public async Task<List<QuotaUsageMetrics>> GetQuotaUsageMetricsAsync()
        {
            await EnsureInitialized();
            return GetCurrentMetrics();
        }

        /// <inheritdoc/>
        public async Task<List<QuotaUsageMetrics>> GetQuotaUsageMetricsAsync(QuotaMetricsFilter filter)
        {
            await EnsureInitialized();
            var metrics = GetCurrentMetrics();

            if (!string.IsNullOrEmpty(filter.QuotaName))
                metrics = metrics.Where(m => m.QuotaName == filter.QuotaName).ToList();

            if (!string.IsNullOrEmpty(filter.PartitionId))
                metrics = metrics.Where(m => m.PartitionId == filter.PartitionId).ToList();

            return metrics;
        }

        /// <inheritdoc/>
        public async Task<List<QuotaUsageHistory>> GetQuotaUsageHistoryAsync(
            string quotaName,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            await EnsureInitialized();

            // For now, return empty history as we don't have historical storage yet
            // This would need to be implemented with Cosmos DB or similar for production
            _logger.LogInformation("[QuotaService {ServiceIdentifier}] Historical data requested for quota '{QuotaName}' from {StartTime} to {EndTime}. Historical storage not yet implemented.",
                _serviceIdentifier, quotaName, startTime, endTime);

            return [];
        }

        private List<QuotaUsageMetrics> GetCurrentMetrics()
        {
            var metrics = new List<QuotaUsageMetrics>();
            var now = DateTimeOffset.UtcNow;

            foreach (var kvp in _quotaContexts)
            {
                var quotaContext = kvp.Value;
                var quota = quotaContext.Quota;
                var partitionStates = quotaContext.GetPartitionStates();

                foreach (var partitionState in partitionStates)
                {
                    var currentCount = partitionState.MetricValue;
                    var utilizationPercentage = quota.MetricLimit > 0
                        ? (double)currentCount / quota.MetricLimit * 100
                        : 0;

                    metrics.Add(new QuotaUsageMetrics
                    {
                        QuotaName = quota.Name,
                        QuotaContext = quota.Context,
                        PartitionId = partitionState.QuotaMetricPartitionId,
                        CurrentCount = currentCount,
                        Limit = quota.MetricLimit,
                        UtilizationPercentage = Math.Round(utilizationPercentage, 2),
                        LockoutActive = partitionState.IsLockedOut,
                        LockoutRemainingSeconds = partitionState.LockoutRemainingSeconds,
                        Timestamp = now
                    });
                }
            }

            return metrics;
        }

        #endregion
    }
}

