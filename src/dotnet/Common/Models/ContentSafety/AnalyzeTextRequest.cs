using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents a request to analyze text with Azure AI Content Safety.
    /// </summary>
    public class AnalyzeTextRequest
    {
        /// <summary>
        /// Gets or sets a text to be analyzed.
        /// </summary>
        [JsonPropertyName("text")]
        public required string Text { get; set; }
    }
}
