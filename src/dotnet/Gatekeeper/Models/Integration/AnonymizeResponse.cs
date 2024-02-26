using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.Integration
{
    /// <summary>
    /// Encapsulates a text anonymize response.
    /// </summary>
    public class AnonymizeResponse
    {
        /// <summary>
        /// The text that was analyzed.
        /// </summary>
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        /// <summary>
        /// A list of anonymized PII (personally identifiable information) entities identified in the analyzed text.
        /// </summary>
        [JsonPropertyName("results")]
        public required List<PIIResultAnonymized> Results { get; set; }
    }
}
