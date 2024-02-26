using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Hubs
{
    /// <summary>
    /// Response from a Prompt Hub request.
    /// </summary>
    public record PromptHubResponse
    {
        /// <summary>
        /// The prompt metadata object returned from a Prompt Hub request.
        /// </summary>
        [JsonPropertyName("prompt")]
        public PromptMetadata? Prompt { get; set; }
    }

    /// <summary>
    /// PromptMetaData record
    /// </summary>
    public record PromptMetadata
    {
        /// <summary>
        /// Name of the prompt.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Text of the prompt prefix to be assigned to an agent.
        /// </summary>
        [JsonPropertyName("prompt_prefix")]
        public string? PromptPrefix { get; set; }

        /// <summary>
        /// Text of the prompt suffix to be assigned to an agent.
        /// </summary>
        [JsonPropertyName("prompt_suffix")]
        public string? PromptSuffix { get; set; }
    }
}
