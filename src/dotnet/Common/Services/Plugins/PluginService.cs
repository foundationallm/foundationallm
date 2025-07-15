using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Services.Plugins
{
    /// <summary>
    /// Provides capabilities for managing plugins.
    /// </summary>
    /// <param name="pluginResourceProvider">The FoundationaLLM Plugin resource provider service.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    public class PluginService(
        IResourceProviderService pluginResourceProvider,
        IServiceProvider serviceProvider) : IPluginService
    {
        private readonly IResourceProviderService _pluginResourceProvider = pluginResourceProvider;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IPluginPackageManagerResolver _packageManagerResolver =
            new PluginPackageManagerResolver(pluginResourceProvider);

        /// <inheritdoc/>
        public async Task<IDataSourcePlugin> GetDataSourcePlugin(
            string instanceId,
            string pluginObjectId,
            Dictionary<string, object> pluginParameters,
            string dataSourceObjectId,
            UnifiedUserIdentity userIdentity)
        {
            var pluginDefinition = await _pluginResourceProvider.GetResourceAsync<PluginDefinition>(
                pluginObjectId,
                userIdentity);

            var packageManager = await _packageManagerResolver.GetPluginPackageManager(
                pluginDefinition,
                userIdentity);

            var dataSourcePlugin = packageManager.GetDataSourcePlugin(
                pluginDefinition.Name,
                dataSourceObjectId,
                pluginParameters,
                _packageManagerResolver,
                _serviceProvider);

            return dataSourcePlugin;
        }

        /// <inheritdoc/>
        public async Task<IDataPipelineStagePlugin> GetDataPipelineStagePlugin(
            string instanceId,
            string pluginObjectId,
            Dictionary<string, object> pluginParameters,
            UnifiedUserIdentity userIdentity)
        {
            var pluginDefinition = await _pluginResourceProvider.GetResourceAsync<PluginDefinition>(
                pluginObjectId,
                userIdentity);

            var packageManager = await _packageManagerResolver.GetPluginPackageManager(
                pluginDefinition,
                userIdentity);

            var dataPipelineStagePlugin =packageManager.GetDataPipelineStagePlugin(
                pluginDefinition.Name,
                pluginParameters,
                _packageManagerResolver,
                _serviceProvider);

            return dataPipelineStagePlugin;
        }

        /// <inheritdoc/>
        public async Task<IPluginPackageManager> GetPluginPackageManager(
            string instanceId,
            string pluginObjectId,
            UnifiedUserIdentity userIdentity) =>
            await _packageManagerResolver.GetPluginPackageManager(
                pluginObjectId,
                userIdentity);
    }
}
