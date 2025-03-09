using FoundationaLLM.Common.Constants.Plugins;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins.Metadata
{
    /// <summary>
    /// Provides the model for plugin dependency metadata.
    /// </summary>
    public class PluginDependencyMetadata
    {
        /// <summary>
        /// Gets or sets the type of dependency selection.
        /// </summary>
        [JsonPropertyName("selection_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PluginDependencySelectionTypes SelectionType { get; set; }

        /// <summary>
        /// Gets or sets the list of dependencies for the plugin.
        /// </summary>
        /// <remarks>
        /// The list contains the names of the plugins that this plugin depends on.
        /// </remarks>
        [JsonPropertyName("dependency_plugin_names")]
        public List<string> DependencyPluginNames { get; set; } = [];
    }
}
