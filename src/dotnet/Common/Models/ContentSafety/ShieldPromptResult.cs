using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Prompt shield result.
    /// </summary>
    public class ShieldPromptResult
    {
        /// <summary>
        /// Contains analysis results for the user prompt.	
        /// </summary>
        [JsonPropertyName("userPromptAnalysis")]
        public ShieldPromptDetails? UserPromptAnalysis { get; set; }

        /// <summary>
        /// Contains a list of analysis results for each document provided.
        /// </summary>
        [JsonPropertyName("documentsAnalysis")]
        public List<ShieldPromptDetails>? DocumentsAnalysis { get; set; }
    }

    /// <summary>
    /// Contains analysis results for the user prompt.
    /// </summary>
    public class ShieldPromptDetails
    {
        /// <summary>
        /// Indicates whether a User Prompt attack (for example, malicious input, security threat) has been detected in the user prompt.
        /// </summary>
        [JsonPropertyName("attackDetected")]
        public bool AttackDetected { get; set; }
    }
}
