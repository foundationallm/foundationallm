using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents a request to shield a prompt with Azure AI Content Safety.
    /// </summary>
    public class ShieldPromptRequest
    {
        /// <summary>
        /// Gets or sets a text or message input provided by the user.
        /// This could be a question, command, or other form of text input.
        /// </summary>
        [JsonPropertyName("userPrompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// Gets or sets a list or collection of textual documents, articles,
        /// or other string-based content. Each element in the array is expected to be a string.
        /// </summary>
        [JsonPropertyName("documents")]
        public List<string> Documents { get; set; } = [];
    }
}
