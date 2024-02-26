using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.Integration
{
    /// <summary>
    /// Encapsulates a text analysis request.
    /// </summary>
    public class AnalyzeRequest
    {
        /// <summary>
        /// The text to be analyzed.
        /// </summary>
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        /// <summary>
        /// A flag used to tell if PII found by analysis should be anonymized.
        /// </summary>
        [JsonPropertyName("anonymize")]
        public required bool Anonymize {  get; set; }

        /// <summary>
        /// The language used to detect PII.
        /// </summary>
        [JsonPropertyName("language")]
        public string? Language {  get; set; }
    }
}
