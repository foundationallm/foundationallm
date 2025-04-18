using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.Plugins.Metadata;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Plugin
{
    /// <summary>
    /// Provides the model for a plugin.
    /// </summary>
    public class PluginDefinition: ResourceBase
    {
        /// <summary>
        /// Gets or sets the FoundationaLLM object identifier of the plugin package that provides the plugin.
        /// </summary>
        [JsonPropertyName("plugin_package_object_id")]
        public required string PluginPackageObjectId { get; set; }

        /// <summary>
        /// Gets or sets the category of the plugin.
        /// </summary>
        [JsonPropertyName("category")]
        public required string Category { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the plugin parameters.
        /// </summary>
        [JsonPropertyName("parameters")]
        public List<PluginParameterMetadata> Parameters { get; set; } = [];

        /// <summary>
        /// Gets or sets the metadata associated with the plugin parameter selection hints.
        /// </summary>
        /// <remarks>
        /// <para>The keys in the dictionary are the name of the paramters that are of type resource-object-id.</para>
        /// <para>The values in the dictionary are <see cref="PluginParameterSelectionHint"/> object providing the selection hints for the parameter.</para>
        /// </remarks>
        [JsonPropertyName("parameter_selection_hints")]
        public Dictionary<string, PluginParameterSelectionHint> ParameterSelectionHints { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of <see cref="PluginDependencyMetadata"/> objects that represent the dependencies of the plugin.
        /// </summary>
        [JsonPropertyName("dependencies")]
        public List<PluginDependencyMetadata> Dependencies { get; set; } = [];
    }
}
