using FoundationaLLM.Common.Constants.Quota;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Quota;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<QuotaService> _logger;

        private List<QuotaDefinition> _quotaDefinitions = [];
        private Dictionary<string, QuotaContextBase> _quotaContexts = [];

        /// <inheritdoc/>
        public bool Enabled => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotaService"/> class.
        /// </summary>
        /// <param name="storageService">The storage service used for storing quota configuration.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        public QuotaService(
            IStorageService storageService,
            ILoggerFactory loggerFactory)
        {
            _storageService = storageService;
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

                if (_quotaDefinitions.Any(qd => qd.DistributedEnforcement))
                {
                    // Configure the distributed enforcement context.
                }

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
                return quotaContext.AddMetricUnitAndEvaluateQuota(userIdentifier, userPrincipalName);
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
                return quotaContext.AddMetricUnitAndEvaluateQuota(userIdentifier, userPrincipalName);
            else
                return QUOTA_NOT_EXCEEDED_EVALUATION_RESULT;
        }

        private string BuildContext(string?[] tokens) =>
            string.Join(":", tokens
                .Select(t => string.IsNullOrWhiteSpace(t) ? "__default+__" : t)
                .ToArray());
    }
}
