using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
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
        ILogger logger) : BackgroundService
    {
        private readonly TimeSpan _executionCycleInterval = executionCycleInterval;

        protected readonly IServiceProvider _serviceProvider = serviceProvider;
        protected readonly ILogger _logger = logger;

        protected Task _initializationTask = null!;

        protected IResourceProviderService _dataPipelineResourceProvider = null!;
        protected IResourceProviderService _pluginResourceProvider = null!;

        protected virtual string ServiceName => string.Empty;

        #region Initialization

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

        private async Task EnsureDependencyInjectionResolution(
            CancellationToken stoppingToken)
        {
            // Wait until the Data Pipeline Resource Provider is set and then
            // wait for its initialization task to complete.
            while (_dataPipelineResourceProvider == null)
            {
                _logger.LogWarning("The Data Pipeline Resource Provider is not set. Waiting for it to be set...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The {ServiceName} service is stopping.", ServiceName);
                    return;
                }
            }

            _logger.LogInformation("The Data Pipeline Resource Provider is set. Waiting for its initialization task to complete...");
            await _dataPipelineResourceProvider.InitializationTask;
            _logger.LogInformation("The Data Pipeline Resource Provider is initialized.");

            EnsureDependencyInjectionResolutionInternal(stoppingToken);
        }

        /// <summary>
        /// Ensures that all dependencies are resolved and initialized before the service starts.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token used to signal a request to stop the service.</param>
        protected virtual void EnsureDependencyInjectionResolutionInternal(
            CancellationToken stoppingToken)
        {
            // This method is intentionally left empty. It can be overridden in derived classes
            // to perform additional dependency injection resolution if needed.
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

            // Perform dependency injection resolution and wait for the initialization task to complete.
            // This is necessary to ensure that all dependencies are resolved before the service starts and
            // cirucular references are avoided.
            _initializationTask = EnsureDependencyInjectionResolution(stoppingToken);
            await _initializationTask;

            // Perform additional initialization for the background service.
            await InitializeAsyncInternal(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteAsyncInternal(stoppingToken);
                await Task.Delay(_executionCycleInterval, stoppingToken);
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

        /// <summary>
        /// Retrieves the data source plugin based on the specified plugin object identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="pluginObjectId">The plugin object identifier.</param>
        /// <param name="pluginParameters">The dictionary containing the names and values of the plugin parameters.</param>
        /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The data source plugin.</returns>
        protected async Task<IDataSourcePlugin> GetDataSourcePlugin(
            string instanceId,
            string pluginObjectId,
            Dictionary<string, object> pluginParameters,
            string dataSourceObjectId,
            UnifiedUserIdentity userIdentity)
        {
            var result = await GetPluginPackageManagerInternal(
                instanceId,
                pluginObjectId,
                userIdentity);

            var dataSourcePlugin = result.PackageManager.GetDataSourcePlugin(
                result.PluginDefinition.Name,
                dataSourceObjectId,
                pluginParameters,
                _serviceProvider);

            return dataSourcePlugin;
        }

        /// <summary>
        /// Retrieves the data pipeline stage plugin based on the specified plugin object identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="pluginObjectId">The plugin object identifier.</param>
        /// <param name="pluginParameters">The dictionary containing the names and values of the plugin parameters.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The data pipeline stage plugin.</returns>
        protected async Task<IDataPipelineStagePlugin> GetDataPipelineStagePlugin(
            string instanceId,
            string pluginObjectId,
            Dictionary<string, object> pluginParameters,
            UnifiedUserIdentity userIdentity)
        {
            var result = await GetPluginPackageManagerInternal(
                instanceId,
                pluginObjectId,
                userIdentity);

            var dataPipelineStagePlugin = result.PackageManager.GetDataPipelineStagePlugin(
                result.PluginDefinition.Name,
                pluginParameters,
                _serviceProvider);

            return dataPipelineStagePlugin;
        }

        /// <summary>
        /// Retrieves the data source plugin based on the specified plugin object identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="pluginObjectId">The plugin object identifier.</param>
        /// <param name="pluginParameters">The dictionary containing the names and values of the plugin parameters.</param>
        /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The plugin package manager instance that manages the plugin.</returns>
        public async Task<IPluginPackageManager> GetPluginPackageManager(
            string instanceId,
            string pluginObjectId,
            UnifiedUserIdentity userIdentity) =>
            (await GetPluginPackageManagerInternal(
                instanceId,
                pluginObjectId,
                userIdentity)).PackageManager;

        private async Task<(IPluginPackageManager PackageManager, PluginDefinition PluginDefinition)> GetPluginPackageManagerInternal(
            string instanceId,
            string pluginObjectId,
            UnifiedUserIdentity userIdentity)
        {
            var pluginDefinition = await _pluginResourceProvider.GetResourceAsync<PluginDefinition>(
                pluginObjectId,
                userIdentity);

            var pluginPackage = await _pluginResourceProvider.ExecuteResourceActionAsync<PluginPackageDefinition, object, ResourceProviderActionResult<PluginPackageManagerInstance>>(
                instanceId,
                ResourcePath.GetResourcePath(pluginDefinition.PluginPackageObjectId).MainResourceId!,
                ResourceProviderActions.LoadPluginPackage,
                null!,
                userIdentity);

            return (pluginPackage.Resource!.Instance, pluginDefinition);
        }
    }
}
