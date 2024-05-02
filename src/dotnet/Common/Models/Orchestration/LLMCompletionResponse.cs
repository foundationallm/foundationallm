using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// LLMOrchestrationCompletionResponse class
    /// </summary>
    public class LLMCompletionResponse
    {
        /// <summary>
        /// The completion response from the orchestration engine.
        /// </summary>
        [JsonPropertyName("completion")]
        public string? Completion { get; set; }

        /// <summary>
        /// The citations used in building the completion response.
        /// </summary>
        [JsonPropertyName("citations")]
        public Citation[]? Citations { get; set; }

        /// <summary>
        /// The prompt received from the user.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// The full prompt composed by the LLM.
        /// </summary>
        [JsonPropertyName("full_prompt")]
        public string? FullPrompt { get; set; }

        /// <summary>
        /// The prompt template used by the LLM.
        /// </summary>
        [JsonPropertyName("prompt_template")]
        public string? PromptTemplate { get; set; }

        /// <summary>
        /// The name of the FoundationaLLM agent.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public string? AgentName { get; set; }

        /// <summary>
        /// The number of tokens in the prompt.
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; } = 0;

        /// <summary>
        /// The number of tokens in the completion.
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; } = 0;

        /// <summary>
        /// Total tokens used.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; } = 0;

        /// <summary>
        /// Total cost of the completion execution.
        /// </summary>
        [JsonPropertyName("total_cost")]
        public float TotalCost { get; set; } = 0f;
    }
}
