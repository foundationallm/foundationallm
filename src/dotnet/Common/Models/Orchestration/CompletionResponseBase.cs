using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Base model for a response from a language model.
    /// </summary>
    public class CompletionResponseBase
    {
        /// <summary>
        /// The Operation ID identifying the completion request.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string OperationId { get; set; }

        /// <summary>
        /// The completion response from the language model.
        /// </summary>
        [JsonPropertyName("completion")]
        public string Completion { get; set; }

        /// <summary>
        /// Content returned from the Assistants API.
        /// </summary>
        [JsonPropertyName("content")]
        public List<MessageContentItemBase>? Content { get; set; }

        /// <summary>
        /// The citations used in building the completion response.
        /// </summary>
        [JsonPropertyName("citations")]
        public Citation[]? Citations { get; set; }

        /// <summary>
        /// The user prompt the language model responded to.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string UserPrompt { get; set; }

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
        /// The total number of tokens.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens => PromptTokens + CompletionTokens;

        /// <summary>
        /// The total cost of executing the completion operation.
        /// </summary>
        [JsonPropertyName("total_cost")]
        public float TotalCost { get; set; } = 0.0f;

        /// <summary>
        /// Deleted flag used for soft delete.
        /// </summary>
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }
    }
}
