using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Represents an abuse indicator.
    /// </summary>
    public class AbuseIndicator
    {
        /// <summary>
        /// The type of abuse indicator.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Description of the abuse indicator.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The severity level (Low, Medium, High, Critical).
        /// </summary>
        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "Low";

        /// <summary>
        /// When this indicator was detected.
        /// </summary>
        [JsonPropertyName("detected_at")]
        public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Additional metrics associated with this indicator.
        /// </summary>
        [JsonPropertyName("metrics")]
        public Dictionary<string, object> Metrics { get; set; } = new();
    }
}
