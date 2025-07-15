using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;

namespace FoundationaLLM.Common.Services.Plugins
{
    /// <summary>
    /// Provides functionality to resolve and manage plugin packages.
    /// </summary>
    /// <param name="pluginResourceProvider">The resource provider service for plugins.</param>
    /// <remarks>This class implements the <see cref="IPluginPackageManagerResolver"/> interface to offer
    /// methods for handling plugin package resolution. It is designed to be used in scenarios where dynamic loading and
    /// management of plugins are required.</remarks>
    public class PluginPackageManagerResolver(
        IResourceProviderService pluginResourceProvider) : IPluginPackageManagerResolver
    {
        private readonly IResourceProviderService _pluginResourceProvider = pluginResourceProvider;

        /// <inheritdoc/>
        public async Task<IPluginPackageManager> GetPluginPackageManager(
            string pluginObjectId,
            UnifiedUserIdentity userIdentity)
        {
            var pluginDefinition = await _pluginResourceProvider.GetResourceAsync<PluginDefinition>(
                pluginObjectId,
                userIdentity);

           return await GetPluginPackageManager(pluginDefinition, userIdentity);
        }

        /// <inheritdoc/>
        public async Task<IPluginPackageManager> GetPluginPackageManager(
            PluginDefinition pluginDefinition,
            UnifiedUserIdentity userIdentity)
        {
            var resourcePath = ResourcePath.GetResourcePath(pluginDefinition.PluginPackageObjectId);

            var pluginPackage = await _pluginResourceProvider.ExecuteResourceActionAsync<PluginPackageDefinition, object, ResourceProviderActionResult<PluginPackageManagerInstance>>(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                ResourceProviderActions.LoadPluginPackage,
                null!,
                userIdentity);

            return pluginPackage.Resource!.Instance;
        }
    }
}
