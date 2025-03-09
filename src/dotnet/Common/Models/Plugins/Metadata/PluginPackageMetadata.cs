using FoundationaLLM.Common.Constants.Plugins;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins.Metadata
{
    /// <summary>
    /// Provides the model for plugin package metadata.
    /// </summary>
    public class PluginPackageMetadata
    {
        /// <summary>
        /// Gets or sets the plugin package name.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the plugin package display name.
        /// </summary>
        [JsonPropertyName("display_name")]
        public required string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the plugin package description.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the plugin package platform.
        /// </summary>
        [JsonPropertyName("platform")]
        public PluginPackagePlatform? Platform { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="PluginMetadata"/> objects describing the plugins in the package.
        /// </summary>
        [JsonPropertyName("plugin_metadata")]
        public List<PluginMetadata> Plugins { get; set; } = [];
    }
}
