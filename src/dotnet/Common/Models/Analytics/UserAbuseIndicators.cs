using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Abuse indicators detected for a user.
    /// </summary>
    public class UserAbuseIndicators
    {
        /// <summary>
        /// The username/UPN of the user.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The calculated risk score (0-100).
        /// </summary>
        [JsonPropertyName("risk_score")]
        public int RiskScore { get; set; }

        /// <summary>
        /// Volume-based abuse indicators.
        /// </summary>
        [JsonPropertyName("volume_indicators")]
        public List<AbuseIndicator> VolumeIndicators { get; set; } = [];

        /// <summary>
        /// Temporal pattern abuse indicators.
        /// </summary>
        [JsonPropertyName("temporal_indicators")]
        public List<AbuseIndicator> TemporalIndicators { get; set; } = [];

        /// <summary>
        /// Behavioral pattern abuse indicators.
        /// </summary>
        [JsonPropertyName("behavioral_indicators")]
        public List<AbuseIndicator> BehavioralIndicators { get; set; } = [];

        /// <summary>
        /// Resource exhaustion indicators.
        /// </summary>
        [JsonPropertyName("resource_indicators")]
        public List<AbuseIndicator> ResourceIndicators { get; set; } = [];
    }
}
