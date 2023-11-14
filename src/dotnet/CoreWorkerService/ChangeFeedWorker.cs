using FoundationaLLM.Core.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace CoreWorkerService
{
    /// <summary>
    /// Background service that runs the Change Feed Processor.
    /// </summary>
    public class ChangeFeedWorker : BackgroundService
    {
        private readonly ILogger<ChangeFeedWorker> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly ICosmosDbChangeFeedService _cosmosDbChangeFeedService;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ChangeFeedWorker"/>.
        /// </summary>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="ChangeFeedWorker"/> type name.</param>
        /// <param name="telemetryClient">The telemetry client sends events,
        /// metrics, and other telemetry to the App Insights service.</param>
        /// <param name="cosmosDbChangeFeedService">The Cosmos DB change feed
        /// processor that performs the event processing tasks for the worker.</param>
        public ChangeFeedWorker(ILogger<ChangeFeedWorker> logger,
            TelemetryClient telemetryClient,
            ICosmosDbChangeFeedService cosmosDbChangeFeedService)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _cosmosDbChangeFeedService = cosmosDbChangeFeedService;
        }

        /// <summary>
        /// Executes the Change Feed Processor.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{time}: Starting the ChangeFeedWorker", DateTimeOffset.Now);
            using (_telemetryClient.StartOperation<RequestTelemetry>("StartChangeFeedWorker"))
            {
                await _cosmosDbChangeFeedService.StartChangeFeedProcessorsAsync();
            }
        }
    }
}