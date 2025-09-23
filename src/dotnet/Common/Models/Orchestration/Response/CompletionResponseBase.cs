using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response
{
    /// <summary>
    /// Base model for a response from a language model.
    /// </summary>
    public class CompletionResponseBase
    {
        /// <summary>
        /// The unigue identifier for the completion response, same as the operation id.
        /// </summary>
        /// <remarks>
        /// This property is provided for convenience when storing completion responses in a database where an "id" field is expected.
        /// </remarks>
        [JsonPropertyName("id")]
        public string Id => OperationId;

        /// <summary>
        /// The Operation ID identifying the completion request.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string OperationId { get; set; }

        /// <summary>
        /// The completion response from the language model.
        /// </summary>
        [JsonPropertyName("completion")]
        public string? Completion { get; set; }

        /// <summary>
        /// Content returned from the Assistants API.
        /// </summary>
        [JsonPropertyName("content")]
        public List<MessageContentItemBase>? Content { get; set; }

        /// <summary>
        /// A list of results from the analysis.
        /// </summary>
        [JsonPropertyName("analysis_results")]
        public List<AnalysisResult>? AnalysisResults { get; set; }

        /// <summary>
        /// The sources used in building the completion response.
        /// </summary>
        [JsonPropertyName("content_artifacts")]
        public ContentArtifact[]? ContentArtifacts { get; set; }

        /// <summary>
        /// The user prompt the language model responded to.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string UserPrompt { get; set; }

        /// <summary>
        /// The user prompt rewrite.
        /// </summary>
        [JsonPropertyName("user_prompt_rewrite")]
        public string? UserPromptRewrite { get; set; }

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

        /// <summary>
        /// An array of error messages that occurred during the completion operation.
        /// </summary>
        [JsonPropertyName("errors")]
        public string[]? Errors { get; set; }

        /// <summary>
        /// Whether the errors collection actually represent a full error condition or not.
        /// </summary>
        [JsonPropertyName("is_error")]
        public bool? IsError { get; set; }
    }
}
