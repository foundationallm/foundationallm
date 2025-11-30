using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Represents a user anomaly detected by the system.
    /// </summary>
    public class UserAnomaly
    {
        /// <summary>
        /// The username/UPN of the user.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The type of anomaly detected.
        /// </summary>
        [JsonPropertyName("anomaly_type")]
        public string AnomalyType { get; set; } = string.Empty;

        /// <summary>
        /// Description of the anomaly.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The severity level (Low, Medium, High, Critical).
        /// </summary>
        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "Low";

        /// <summary>
        /// When this anomaly was detected.
        /// </summary>
        [JsonPropertyName("detected_at")]
        public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Additional data about the anomaly.
        /// </summary>
        [JsonPropertyName("anomaly_data")]
        public Dictionary<string, object> AnomalyData { get; set; } = new();
    }
}
