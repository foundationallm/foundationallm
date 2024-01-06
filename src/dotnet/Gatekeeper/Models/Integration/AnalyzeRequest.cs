using Newtonsoft.Json;

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
        [JsonProperty("content")]
        public required string Content { get; set; }

        /// <summary>
        /// A flag used to tell if PII found by analysis should be anonymized.
        /// </summary>
        [JsonProperty("anonymize")]
        public required bool Anonymize {  get; set; }

        /// <summary>
        /// The language used to detect PII.
        /// </summary>
        [JsonProperty("language")]
        public string? Language {  get; set; }
    }
}
