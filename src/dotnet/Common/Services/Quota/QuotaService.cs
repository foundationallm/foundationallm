using Azure.Messaging;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.Quota;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
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
        private const string QUOTA_SERVICE_NAME = "QuotaService";
        private const string STORAGE_CONTAINER_NAME = "quota";
        private const string QUOTA_STORE_FILE_PATH = "/quota-store.json";
        private readonly QuotaEvaluationResult QUOTA_NOT_EXCEEDED_EVALUATION_RESULT = new();

        private DateTimeOffset _initializationStartTime;
        private bool _isInitialized = false;
        // The service is enabled by default.
        // Once initialization completes, the service will be disabled if there are no quota definitions in the quota store.
        // While initialization is in progress, the service is enabled to make sure we can handle the situation where initialization fails.
        private bool _enabled = true;
        private readonly IStorageService _storageService;
        private readonly IEventService _eventService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<QuotaService> _logger;

        private List<QuotaDefinition> _quotaDefinitions = [];
        private Dictionary<string, QuotaContextBase> _quotaContexts = [];

        private LocalEventService? _localEventService;

        /// <inheritdoc/>
        public bool Enabled => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotaService"/> class.
        /// </summary>
        /// <param name="storageService">The storage service used for storing quota configuration.</param>
        /// <param name="eventService">The <see cref="IEventService"/> providing event services to the quota service.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        public QuotaService(
            IStorageService storageService,
            IEventService eventService,
            ILoggerFactory loggerFactory)
        {
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
                _logger.LogInformation("Starting to initialize the quota service ...");
                _initializationStartTime = DateTimeOffset.UtcNow;

                await LoadQuotaDefinitions();

                _quotaContexts = _quotaDefinitions
                    .Select(qd => qd.MetricPartition switch
                    {
                        QuotaMetricPartition.None => (new PartitionlessQuotaContext(
                            qd, _loggerFactory.CreateLogger<PartitionlessQuotaContext>())) as QuotaContextBase,
                        QuotaMetricPartition.UserIdentifier => (new UserIdentifierQuotaContext(
                            qd, _loggerFactory.CreateLogger<UserIdentifierQuotaContext>())) as QuotaContextBase,
                        QuotaMetricPartition.UserPrincipalName => (new UserPrincipalNameQuotaContext(
                            qd, _loggerFactory.CreateLogger<UserPrincipalNameQuotaContext>())) as QuotaContextBase,
                        _ => throw new QuotaException($"Unsupported metric partition: {qd.MetricPartition}")
                    })
                    .ToDictionary(qc => qc.Quota.Context);

                InitializeLocalEventService();

                _isInitialized = true;
                _enabled = _quotaDefinitions.Count > 0;

                _logger.LogInformation("The quota service was successfully initialized.");

                if (!_enabled)
                    _logger.LogWarning("The quota service is disabled because there are no quota definitions in the quota store.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The quota service failed to initialize.");
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
                _logger.LogInformation("The local event service is not configured because there are not quotas with distributed enforcement.");
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
                new QuotaException("The APIRequestQuotaService service failed to initialize within the required time interval of 60 seconds.");
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

            _logger.LogDebug("{EventsCount} events of type {EventType} received out of which {QuotaServiceEventsCount} are targeted for the quota service.",
                originalEventCount,
                e.EventType,
                eventsToProcess.Count);

            // If the quota service doesn't have any events to process, return.
            if (eventsToProcess.Count == 0)
                return;

            // Handle the common events here and defer the rest to the derived classes.
            switch (e.EventType)
            {
                case EventTypes.FoundationaLLM_Quota_MetricUpdate:
                    HandleQuotaMetricUpdates(eventsToProcess);
                    break;
                default:
                    _logger.LogWarning("The quota service does not handle events of type {EventType}.", e.EventType);
                    break;
            }

            await Task.CompletedTask;
        }

        private void HandleQuotaMetricUpdates(List<CloudEvent> quotaMetricUpdateEvents)
        {
            foreach(var quotaMetricUpdateEvent in quotaMetricUpdateEvents)
            {

            }
        }

        /// <summary>
        /// Sends a quota metric update event to the event service.
        /// </summary>
        /// <param name="data">The optional data to send with the event.</param>
        /// <returns></returns>
        /// <remarks>
        /// See <see cref="EventTypes"/> for a list of event types.
        /// </remarks>
        private async Task SendQuotaMetricUpdateEvent(object? data = null)
        {
            if (_eventService == null)
            {
                _logger.LogWarning("The quota service does not have an event service configured and cannot send events.");
                return;
            }

            // The CloudEvent source is automatically filled in by the event service.
            await _eventService.SendEvent(
                EventGridTopics.FoundationaLLM_API_Statistics,
                new CloudEvent(
                    string.Empty,
                    EventTypes.FoundationaLLM_Quota_MetricUpdate,
                    data ?? new { })
                {
                    Subject = QUOTA_SERVICE_NAME
                });
        }

        #endregion

        /// <inheritdoc/>
        public QuotaEvaluationResult EvaluateRawRequestForQuota(
            string apiName,
            string? controllerName,
            UnifiedUserIdentity? userIdentity)
        {
            if (!_isInitialized)
                if (InitializationPending())
                    return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;

            var context = BuildContext([apiName, controllerName]);
            var userIdentifier = userIdentity?.UserId ?? "__default__";
            var userPrincipalName = userIdentity?.UPN ?? "__default__";

            if (_quotaContexts.TryGetValue(context, out var quotaContext))
                return quotaContext.AddLocalMetricUnitAndEvaluateQuota(userIdentifier, userPrincipalName);
            else
                return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;
        }

        /// <inheritdoc/>
        public QuotaEvaluationResult EvaluateCompletionRequestForQuota(
            string apiName,
            string controllerName,
            UnifiedUserIdentity? userIdentity,
            CompletionRequest completionRequest)
        {
            if (!_isInitialized)
                if (InitializationPending())
                    return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;

            var context = BuildContext([apiName, controllerName, completionRequest?.AgentName]);
            var userIdentifier = userIdentity?.UserId ?? "__default__";
            var userPrincipalName = userIdentity?.UPN ?? "__default__";

            if (_quotaContexts.TryGetValue(context, out var quotaContext))
                return quotaContext.AddLocalMetricUnitAndEvaluateQuota(userIdentifier, userPrincipalName);
            else
                return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;
        }

        private string BuildContext(string?[] tokens) =>
            string.Join(":", tokens
                .Select(t => string.IsNullOrWhiteSpace(t) ? "__default+__" : t)
                .ToArray());
    }
}
