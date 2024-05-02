using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    /// <summary>
    /// How each of the input fields map to the testing fields.
    /// </summary>
    public class InputsMapping
    {
        /// <summary>
        /// Name of the input field for the question
        /// </summary>
        [JsonPropertyName("question")]
        public string? Question { get; set; }
        /// <summary>
        /// Name of the input field for the answer
        /// </summary>
        [JsonPropertyName("answer")]
        public string? Answer { get; set; }
        /// <summary>
        /// Name of the input field for the context
        /// </summary>
        [JsonPropertyName("context")]
        public string? Context { get; set; }
        /// <summary>
        /// Name of the input field for the ground truth.
        /// </summary>
        [JsonPropertyName("ground_truth")]
        public string? GroundTruth { get; set; }
        /// <summary>
        /// Name of the input field for the metrics to run.
        /// </summary>
        [JsonPropertyName("metrics")]
        public string? Metrics { get; set; }
    }
}
