using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.LakeraGuard
{
    /// <summary>
    /// Prompt injection endpoint result.
    /// </summary>
    public class AnalyzePromptInjectionResult
    {
        /// <summary>
        /// The model identifier string of the model used for analysis.
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// A list of results with an object containing each category that the prompt injection endpoint supports.
        /// </summary>
        [JsonPropertyName("results")]
        public required List<PromptInjectionResult> Results { get; set; }
    }

    /// <summary>
    /// Internal prompt injection endpoint result.
    /// </summary>
    public class PromptInjectionResult
    {
        /// <summary>
        /// A dictionary of detectors that the endpoint analyzed with a boolean decision of whether the input contains the analyzed category.
        /// </summary>
        [JsonPropertyName("categories")]
        public required Dictionary<string, bool> Categories { get; set; }

        /// <summary>
        /// A dictionary of detectors that the endpoint analyzed with a floating point confidence score between 0 and 1.
        /// </summary>
        [JsonPropertyName("category_scores")]
        public required Dictionary<string, decimal> CategoryScores { get; set; }

        /// <summary>
        /// A boolean indicating whether any of the endpoint's categories triggered a positive result.
        /// </summary>
        [JsonPropertyName("flagged")]
        public required bool Flagged { get; set; }
    }
}
