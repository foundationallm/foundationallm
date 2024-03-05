using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Base class for orchestration request objects.
    /// </summary>
    public class OrchestrationRequest
    {
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }
        
        /// <summary>
        /// Represent the input or user prompt.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public required string UserPrompt { get; set; }

        /// <summary>
        /// Settings for the orchestration request.
        /// </summary>
        [JsonPropertyName("settings")]
        public OrchestrationSettings? Settings { get; set; }
    }
}
