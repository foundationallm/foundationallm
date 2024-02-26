using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.Integration
{
    /// <summary>
    /// Encapsulates the PII (personally identifiable information) results.
    /// </summary>
    public class PIIResult
    {
        /// <summary>
        /// The type of personally identifiable information (i.e. Person, Location, Date).
        /// </summary>
        [JsonPropertyName("entity_type")]
        public required string EntityType {  get; set; }

        /// <summary>
        /// The start index where a PII was detected.
        /// </summary>
        [JsonPropertyName("start_index")]
        public required int StartIndex { get; set; }

        /// <summary>
        /// The end index where a PII entity was detected.
        /// </summary>
        [JsonPropertyName("end_index")]
        public required int EndIndex { get; set; }
    }
}
