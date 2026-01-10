using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents a request to scan content for prompt injection attacks.
    /// </summary>
    public class ContentShieldRequest
    {
        /// <summary>
        /// Gets or sets the text content to analyze for prompt injection.
        /// Use this for single text scanning.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets an optional context description for the analysis.
        /// This provides additional context about the content being scanned.
        /// </summary>
        [JsonPropertyName("context")]
        public string? Context { get; set; }

        /// <summary>
        /// Gets or sets a list of documents to analyze for prompt injection.
        /// Use this for batch document scanning.
        /// </summary>
        [JsonPropertyName("documents")]
        public List<ContentSafetyDocument>? Documents { get; set; }
    }
}
