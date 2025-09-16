using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Application configuration set resource.
    /// </summary>
    public class AppConfigurationSet : ResourceBase
    {
        /// <summary>
        /// Gets or sets the configuration values in the set.
        /// </summary>
        [JsonPropertyName("configuration_values")]
        public Dictionary<string, object?> ConfigurationValues { get; set; } = [];
    }
}
