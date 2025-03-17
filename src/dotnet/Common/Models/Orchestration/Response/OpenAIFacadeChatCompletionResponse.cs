using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response;

/// <summary>
/// OpenAI-compatible chat completion response object.
/// </summary>
public class OpenAIFacadeChatCompletionResponse
{
    /// <summary>
    /// The ID of the chat completion.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The object type (always "chat.completion" for chat completions).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = "chat.completion";

    /// <summary>
    /// The Unix timestamp (in seconds) of when the chat completion was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// The model used for chat completion.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The list of generated chat completions.
    /// </summary>
    [JsonPropertyName("choices")]
    public List<OpenAIFacadeChatChoice> Choices { get; set; } = [];

    /// <summary>
    /// Usage statistics for the chat completion request.
    /// </summary>
    [JsonPropertyName("usage")]
    public OpenAIFacadeUsage Usage { get; set; } = new();

    /// <summary>
    /// Creates an OpenAIFacadeChatCompletionResponse from a CompletionResponse.
    /// </summary>
    /// <param name="response">The internal completion response.</param>
    /// <returns>An OpenAI-compatible chat completion response.</returns>
    public static OpenAIFacadeChatCompletionResponse FromCompletionResponse(CompletionResponse response)
    {
        return new OpenAIFacadeChatCompletionResponse
        {
            Id = response.OperationId,
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = response.AgentName ?? string.Empty,
            Choices = new List<OpenAIFacadeChatChoice>
            {
                new OpenAIFacadeChatChoice
                {
                    Index = 0,
                    Message = new OpenAIFacadeChatMessage
                    {
                        Content = response.Completion
                    },
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
