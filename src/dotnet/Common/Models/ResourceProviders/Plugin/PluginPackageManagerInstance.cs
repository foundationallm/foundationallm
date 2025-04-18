using FoundationaLLM.Common.Interfaces.Plugins;
using NuGet.Versioning;
using System.Runtime.Loader;

namespace FoundationaLLM.Common.Models.ResourceProviders.Plugin
{
    /// <summary>
    /// Represents a plugin package manager instance resource.
    /// </summary>
    public class PluginPackageManagerInstance : ResourceBase
    {
        /// <summary>
        /// Gets or sets the plugin package manager instance.
        /// </summary>
        public IPluginPackageManager Instance { get; set; } = null!;

        /// <summary>
        /// Gets or sets the assembly load context for the plugin package manager instance.
        /// </summary>
        public AssemblyLoadContext AssemblyLoadContext { get; set; } = null!;

        /// <summary>
        /// Gets or sets the plugin package version.
        /// </summary>
        public SemanticVersion PackageVersion { get; set; } = null!;
    }
}
