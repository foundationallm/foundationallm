using Newtonsoft.Json;

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
        [JsonProperty("session_id")]
        public string? SessionId { get; set; }
        
        /// <summary>
        /// Represent the input or user prompt.
        /// </summary>
        [JsonProperty("user_prompt")]
        public string? UserPrompt { get; set; }
    }
}
