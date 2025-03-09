using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins.Metadata
{
    /// <summary>
    /// Provides the model for plugin metadata.
    /// </summary>
    public class PluginMetadata
    {
        /// <summary>
        /// Gets or sets the FoundationaLLM resoure identifier of the plugin.
        /// </summary>
        [JsonPropertyName("object_id")]
        public required string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>\
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name of the plugin.
        /// </summary>
        [JsonPropertyName("display_name")]
        public required string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the plugin.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

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
