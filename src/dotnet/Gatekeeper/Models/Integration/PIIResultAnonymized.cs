using Newtonsoft.Json;

namespace FoundationaLLM.Gatekeeper.Core.Models.Integration
{
    /// <summary>
    /// Encapsulates the anonymized PII (personally identifiable information) results.
    /// </summary>
    public class PIIResultAnonymized : PIIResult
    {
        /// <summary>
        /// The anonymized text to be used instead of the original text.
        /// </summary>
        [JsonProperty("anonymized_text")]
        public required string AnonymizedText { get; set; }

        /// <summary>
        /// The operator needed to anonymize the original text.
        /// </summary>
        [JsonProperty("operator")]
        public required string Operator { get; set; }
    }
}
