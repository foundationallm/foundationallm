using FoundationaLLM.Common.Constants.Plugins;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// Provides the model for a plugin configuration.
    /// </summary>
    public class PluginConfiguration
    {
        /// <summary>
        /// Gets or sets the object identifier of the FoundationaLLM Plugin resource referred by the configuration.
        /// </summary>
        [JsonPropertyName("plugin_object_id")]
        public required string PluginObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>\
        [JsonPropertyName("plugin_name")]
        public string? PluginName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the plugin.
        /// </summary>
        [JsonPropertyName("plugin_display_name")]
        public string? PluginDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the plugin.
        /// </summary>
        [JsonPropertyName("plugin_description")]
        public string? PluginDescription { get; set; }

        /// <summary>
        /// Gets or sets the category of the plugin.
        /// </summary>
        [JsonPropertyName("plugin_category")]
        public string? PluginCategory { get; set; }

        /// <summary>
        /// Gets or sets the list of plugin parameters.
        /// </summary>
        [JsonPropertyName("plugin_parameters")]
        public List<PluginConfigurationParameter> PluginParameters { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of plugin configurations this configuration depends on.
        /// </summary>
        [JsonPropertyName("plugin_dependencies")]
        public List<PluginConfiguration> PluginDependencies { get; set; } = [];
    }
}
