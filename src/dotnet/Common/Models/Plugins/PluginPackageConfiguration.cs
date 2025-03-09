using FoundationaLLM.Common.Constants.Plugins;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// Provides the model for a plugin package configuration.
    /// </summary>
    public class PluginPackageConfiguration
    {
        /// <summary>
        /// Gets or sets the plugin package name.
        /// </summary>
        [JsonPropertyName("plugin_package_name")]
        public required string PluginPackageName { get; set; }

        /// <summary>
        /// Gets or sets the plugin package display name.
        /// </summary>
        [JsonPropertyName("plugin_package_display_name")]
        public required string PluginPackageDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the plugin package description.
        /// </summary>
        [JsonPropertyName("plugin_package_description")]
        public required string PluginPackageDescription { get; set; }

        /// <summary>
        /// Gets or sets the plugin package platform.
        /// </summary>
        [JsonPropertyName("plugin_package_platform")]
        public PluginPackagePlatform? PluginPackagePlatform { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="PluginConfiguration"/> objects describing the plugins in the package.
        /// </summary>
        [JsonPropertyName("plugin_configurations")]
        public List<PluginConfiguration> PluginConfigurations { get; set; } = [];
    }
}
