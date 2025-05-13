using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Services.Plugins;
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
    public class DataPipelineWorkerService : BackgroundService
    {
        private readonly IDataPipelineStateService _stateService;
        private readonly IResourceProviderService _dataPipelineResourceProvider;
        private readonly IPluginService _pluginService;
        private readonly DataPipelineWorkerServiceSettings _settings;
        private readonly ILogger<DataPipelineWorkerService> _logger;

        private readonly string _serviceName = "Data Pipeline Worker Service";

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

            var pluginResourceProvider = resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
            _pluginService = new PluginService(
                pluginResourceProvider,
                serviceProvider);
        }

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
