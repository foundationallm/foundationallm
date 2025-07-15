using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Defines a mechanism to resolve and retrieve a plugin package manager for a specified plugin object.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for providing the appropriate plugin
    /// package manager based on the given plugin object identifier.</remarks>
    public interface IPluginPackageManagerResolver
    {
        /// <summary>
        /// Asynchronously retrieves the plugin package manager associated with the specified plugin object identifier.
        /// </summary>
        /// <param name="pluginObjectId">The unique identifier of the plugin object for which the package manager is requested.</param>
        /// <param name="userIdentity">The identity of the user requesting the package manager.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see
        /// cref="IPluginPackageManager"/> associated with the specified plugin object identifier.</returns>
        Task<IPluginPackageManager> GetPluginPackageManager(
            string pluginObjectId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves an instance of the plugin package manager for the specified plugin definition and user identity.
        /// </summary>
        /// <param name="pluginDefinition">The definition of the plugin for which the package manager is requested.</param>
        /// <param name="userIdentity">The identity of the user requesting the plugin package manager.</param>
        Task<IPluginPackageManager> GetPluginPackageManager(
            PluginDefinition pluginDefinition,
            UnifiedUserIdentity userIdentity);
    }
}
