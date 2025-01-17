using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.AppConfiguration
{
    /// <summary>
    /// The data associated with an Azure App Configuration Event Grid event.
    /// </summary>
    public class AppConfigurationEventData
    {
        /// <summary>
        /// The app configuration key name.
        /// </summary>
        [JsonPropertyName("key")]
        public required string Key { get; set; }
    }
}
