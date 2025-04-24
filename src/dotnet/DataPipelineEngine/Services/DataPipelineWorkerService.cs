using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilites for background processing of data pipeline work items.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="resourceProviderServices">The FoundationaLLM resource provider services.</param>
    /// <param name="options">The options with the service settings.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineWorkerService(
        IAzureCosmosDBDataPipelineService cosmosDBService,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IOptions<DataPipelineWorkerServiceSettings> options,
        ILogger<DataPipelineWorkerService> logger) : BackgroundService
    {
        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;
        private readonly IResourceProviderService _dataPipelineResourceProvider =
            resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);
        private readonly IResourceProviderService _pluginResourceProvider =
            resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
        private readonly DataPipelineWorkerServiceSettings _settings = options.Value;
        private readonly ILogger<DataPipelineWorkerService> _logger = logger;

        private readonly string _serviceName = "Data Pipeline Worker Service";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The {ServiceName} service is starting...", _serviceName);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("The {ServiceName} service is executing...", _serviceName);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            _logger.LogInformation("The {ServiceName} service is stopping.", _serviceName);
        }
    }
}
