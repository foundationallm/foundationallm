using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides methods to manage plugins in FoundationaLLM plugin packages.
    /// </summary>
    public interface IPluginPackageManager
    {
        /// <summary>
        /// Gets the plugin package configuration with the plugin configurations of to the plugins in the package.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>An object of type <see cref="PluginPackageConfiguration"/>.</returns>
        PluginPackageConfiguration GetConfiguration(string instanceId);
    }
}
