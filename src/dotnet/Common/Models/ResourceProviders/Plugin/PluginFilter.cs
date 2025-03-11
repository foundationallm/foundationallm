using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Plugin
{
    /// <summary>
    /// Provides filter properties for Plugin resources.
    /// </summary>
    public class PluginFilter
    {
        /// <summary>
        /// Gets or sets a value representing the list of plugin categories to retrieve.
        /// </summary>
        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = [];
    }
}
