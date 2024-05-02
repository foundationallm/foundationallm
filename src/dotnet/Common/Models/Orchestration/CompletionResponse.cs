using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// Response from a language model.
/// </summary>
public class CompletionResponse
{
    /// <summary>
    /// The completion response from the language model.
    /// </summary>
    [JsonPropertyName("completion")]
    public string Completion { get; set; }

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
    /// User prompt embedding.
    /// </summary>
    [JsonPropertyName("user_prompt_embedding")]
    public float[]? UserPromptEmbedding { get; set; }

    /// <summary>
    /// Initialize a completion response
    /// </summary>
    /// <param name="completion">The completion response from the language model.</param>
    /// <param name="userPrompt">The user prompt the language model responded to.</param>
    /// <param name="userPromptTokens">The number of tokens in the prompt.</param>
    /// <param name="responseTokens">The number of tokens in the completion.</param>
    /// <param name="userPromptEmbedding">User prompt embedding.</param>
    public CompletionResponse(string completion, string userPrompt, int userPromptTokens, int responseTokens,
        float[]? userPromptEmbedding)
    {
        Completion = completion;
        UserPrompt = userPrompt;
        PromptTokens = userPromptTokens;
        CompletionTokens = responseTokens;
        UserPromptEmbedding = userPromptEmbedding;
    }

    /// <summary>
    /// Initialize a completion response.
    /// </summary>
    public CompletionResponse()
    {
        Completion = string.Empty;
        UserPrompt = string.Empty;
    }
}
