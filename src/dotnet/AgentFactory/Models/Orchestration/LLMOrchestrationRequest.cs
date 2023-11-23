using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration
{
    /// <summary>
    /// Base LLM orchestration request
    /// </summary>
    public class LLMOrchestrationRequest
    {
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonProperty("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Prompt entered by the user.
        /// </summary>
        [JsonProperty("user_prompt")]
        public string? UserPrompt { get; set; }
    }
}
