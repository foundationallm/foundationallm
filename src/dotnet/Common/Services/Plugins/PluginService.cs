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

        /// <inheritdoc/>
        public async Task<IDataSourcePlugin> GetDataSourcePlugin(
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

        /// <inheritdoc/>
        public async Task<IDataPipelineStagePlugin> GetDataPipelineStagePlugin(
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

        /// <inheritdoc/>
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
