using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// Provides the base model for an artifact handled by a plugin.
    /// </summary>
    public class PluginComponent : PluginConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the artifact.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the artifact.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }
    }
}
