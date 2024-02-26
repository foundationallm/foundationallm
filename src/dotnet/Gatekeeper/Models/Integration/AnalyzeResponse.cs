using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.Integration
{
    /// <summary>
    /// Encapsulates a text analysis response.
    /// </summary>
    public class AnalyzeResponse
    {
        /// <summary>
        /// The text that was analyzed.
        /// </summary>
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        /// <summary>
        /// A list of PII (personally identifiable information) entities identified in the analyzed text.
        /// </summary>
        [JsonPropertyName("results")]
        public required List<PIIResult> Results { get; set; }
    }
}
