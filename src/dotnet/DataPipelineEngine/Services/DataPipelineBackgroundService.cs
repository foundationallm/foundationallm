using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for data pipeline services that run in the background.
    /// </summary>
    /// <param name="executionCycleInterval">The time interval between two successive execution cycles.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineBackgroundService(
        TimeSpan executionCycleInterval,
        IServiceProvider serviceProvider,
        ILogger logger) : BackgroundService, IResourceProviderClient
    {
        private readonly TimeSpan _executionCycleInterval = executionCycleInterval;

        protected readonly IServiceProvider _serviceProvider = serviceProvider;
        protected readonly ILogger _logger = logger;

        protected Task<bool> _initializationTask = null!;

        protected IPluginService _pluginService = null!;
        protected IResourceProviderService _dataPipelineResourceProvider = null!;

        protected virtual string ServiceName => string.Empty;

        #region Initialization

        /// <inheritdoc/>
        public IEnumerable<IResourceProviderService> ResourceProviders
        {
            set
            {
                var pluginResourceProvider = value
                    .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);

                _pluginService = new PluginService(
                    pluginResourceProvider,
                    _serviceProvider);

                _dataPipelineResourceProvider = value
                    .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);

                SetResourceProviders(value);
            }
        }

        private async Task<bool> WaitForResourceProviders()
        {
            _logger.LogInformation("The {ServiceName} service is waiting for resource providers...", ServiceName);

            // Wait up to 5 minutes in increments of 5 seconds for the resource provider to be available.
            foreach (var _ in Enumerable.Range(1, 60))
            {
                if (_dataPipelineResourceProvider is not null)
                {
                    _logger.LogInformation("The {ServiceName} service was configured with the required resource providers.", ServiceName);
                    return true;
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            _logger.LogError("The {ServiceName} service failed to initialize because the resource providers are not available.",
                    ServiceName);
            return false;
        }

        /// <summary>
        /// Implements additional resource provider initialization.
        /// </summary>
        /// <param name="resourceProviders">The collection of resource providers available in the main dependency injection container.</param>
        protected virtual void SetResourceProviders(
            IEnumerable<IResourceProviderService> resourceProviders)
        {
        }

        /// <summary>
        /// Performs additional initialization for the background service.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token used to signal a request to stop the service.</param>
        protected virtual async Task InitializeAsyncInternal(
            CancellationToken stoppingToken) =>
            // This method is intentionally left empty. It can be overridden in derived classes
            // to perform additional actions during the initialization of the background service.
            await Task.CompletedTask;

        #endregion

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The {ServiceName} service is starting...", ServiceName);

            _initializationTask = WaitForResourceProviders();
            if (!await _initializationTask)
                throw new DataPipelineServiceException(
                    $"The {ServiceName} service failed to initialize because the resource providers are not available.");

            // Perform additional initialization for the background service.
            await InitializeAsyncInternal(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsyncInternal(stoppingToken);
                    await Task.Delay(_executionCycleInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the {ServiceName} service.", ServiceName);
                }
            }

            _logger.LogInformation("The {ServiceName} service is stopping.", ServiceName);
        }

        /// <summary>
        /// Executes the background service's main logic.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token used to signal a request to stop the service.</param>
        protected virtual async Task ExecuteAsyncInternal(
            CancellationToken stoppingToken) =>
            // This method is intentionally left empty. It can be overridden in derived classes
            // to perform additional actions during the execution of the background service.
            await Task.CompletedTask;
    }
}
