using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents the response from a content shield/prompt injection detection scan.
    /// </summary>
    public class ContentShieldResponse
    {
        /// <summary>
        /// Gets or sets a flag indicating whether the analysis was executed successfully.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the content is considered safe.
        /// </summary>
        [JsonPropertyName("safeContent")]
        public bool SafeContent { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether a prompt injection attack was detected.
        /// </summary>
        [JsonPropertyName("promptInjectionDetected")]
        public bool PromptInjectionDetected { get; set; }

        /// <summary>
        /// Gets or sets the details about the analysis result, such as the reason
        /// why content was considered unsafe or error messages if the analysis failed.
        /// </summary>
        [JsonPropertyName("details")]
        public string? Details { get; set; }

        /// <summary>
        /// Gets or sets the list of document identifiers that were found to be unsafe.
        /// This is populated when batch document scanning is performed.
        /// </summary>
        [JsonPropertyName("unsafeDocumentIds")]
        public List<int>? UnsafeDocumentIds { get; set; }

        /// <summary>
        /// Gets or sets the detailed analysis results for each processed document.
        /// This is populated when batch document scanning is performed.
        /// </summary>
        [JsonPropertyName("documentResults")]
        public Dictionary<int, ContentSafetyAnalysisResult>? DocumentResults { get; set; }
    }
}
