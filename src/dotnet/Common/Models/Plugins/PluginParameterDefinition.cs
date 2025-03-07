using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// Provides the model for a plugin parameter definition.
    /// </summary>
    public class PluginParameterDefinition
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }
    }
}
