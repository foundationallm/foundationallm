using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Hosting;
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
        ILogger<DataPipelineRunnerService> logger) : BackgroundService, IDataPipelineRunnerService
    {
        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<DataPipelineRunnerService> _logger = logger;

        private IResourceProviderService _dataPipelineResourceProvider = null!;
        private IResourceProviderService _pluginResourceProvider = null!;

        /// <inheritdoc/>
        public IEnumerable<IResourceProviderService> ResourceProviders
        {
            set
            {
                _dataPipelineResourceProvider = value
                    .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);

                _pluginResourceProvider = value
                    .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
            }
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The Data Pipeline Runner service is starting...");

            // Wait until the Data Pipeline Resource Provider is set and then
            // wait for its initialization task to complete.
            while (_dataPipelineResourceProvider == null)
            {
                _logger.LogWarning("The Data Pipeline Resource Provider is not set. Waiting for it to be set...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The Data Pipeline Runner service is stopping.");
                    return;
                }
            }

            _logger.LogInformation("The Data Pipeline Resource Provider is set. Waiting for its initialization task to complete...");
            await _dataPipelineResourceProvider.InitializationTask;
            _logger.LogInformation("The Data Pipeline Resource Provider is initialized.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("The Data Pipeline Runner service is running.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("The Data Pipeline Runner service is stopping.");
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun> StartRun(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            DataPipelineDefinition dataPipelineDefinition)
        {
            foreach (var contentItem in contentItems)
                contentItem.RunId = dataPipelineRun.RunId;

            dataPipelineRun.ActiveStages = [.. dataPipelineDefinition.StartingStages.Select(s => s.Name)];

            // Combine dataPipelineRun and contentItems into a single array
            var combinedArray = new object[] { dataPipelineRun }.Concat(contentItems).ToArray();

            var result = await _cosmosDBService.UpsertDataPipelineRunBatchAsync(combinedArray);

            if (!result)
                throw new DataPipelineServiceException($"Failed to upsert data pipeline run {dataPipelineRun.RunId}.");

            return dataPipelineRun;
        }
    }
}
