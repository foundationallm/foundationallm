using FoundationaLLM.Common.Models.Orchestration.Response.OpenAI;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response;

/// <summary>
/// Represents usage statistics for the completion request.
/// </summary>
public class OpenAIFacadeUsage
{
    /// <summary>
    /// Number of tokens in the prompt.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the completion.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total number of tokens used (prompt + completion).
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// OpenAI-compatible completion response object.
/// </summary>
public class OpenAIFacadeCompletionResponse
{
    /// <summary>
    /// The ID of the completion.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The object type (always "text_completion" for completions).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = "text_completion";

    /// <summary>
    /// The Unix timestamp (in seconds) of when the completion was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// The model used for completion.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The list of generated completions.
    /// </summary>
    [JsonPropertyName("choices")]
    public List<OpenAIFacadeChoice> Choices { get; set; } = [];

    /// <summary>
    /// Usage statistics for the completion request.
    /// </summary>
    [JsonPropertyName("usage")]
    public OpenAIFacadeUsage Usage { get; set; } = new();

    /// <summary>
    /// Creates an OpenAIFacadeCompletionResponse from a CompletionResponse.
    /// </summary>
    /// <param name="response">The internal completion response.</param>
    /// <returns>An OpenAI-compatible completion response.</returns>
    public static OpenAIFacadeCompletionResponse FromCompletionResponse(CompletionResponse response)
    {
        return new OpenAIFacadeCompletionResponse
        {
            Id = response.OperationId,
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = response.AgentName ?? string.Empty,
            Choices = new List<OpenAIFacadeChoice>
            {
                new OpenAIFacadeChoice
                {
                    Text = (response.Content[0] as OpenAITextMessageContentItem)?.Value,
                    Index = 0,
                    FinishReason = "stop" // Default to "stop" as we don't currently track the reason
                }
            },
            Usage = new OpenAIFacadeUsage
            {
                PromptTokens = response.PromptTokens,
                CompletionTokens = response.CompletionTokens,
                TotalTokens = response.PromptTokens + response.CompletionTokens
            }
        };
    }
} 
