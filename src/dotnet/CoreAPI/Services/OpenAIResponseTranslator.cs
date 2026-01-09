using FoundationaLLM.Common.Models.OpenAI.Responses;
using FoundationaLLM.Common.Models.OpenAI.Shared;
using FoundationaLLM.Common.Models.Orchestration.Response;

namespace FoundationaLLM.Core.API.Services;

/// <summary>
/// Translates FoundationaLLM responses to OpenAI responses.
/// </summary>
public class OpenAIResponseTranslator : IOpenAIResponseTranslator
{
    /// <summary>
    /// Translates a FoundationaLLM completion response to an OpenAI chat completion response.
    /// </summary>
    public OpenAIChatCompletionResponse TranslateCompletionResponse(CompletionResponse completionResponse, string model)
    {
        var response = new OpenAIChatCompletionResponse
        {
            Id = $"chatcmpl-{completionResponse.OperationId}",
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = model,
            Choices = new List<OpenAIChatCompletionChoice>
            {
                new OpenAIChatCompletionChoice
                {
                    Index = 0,
                    Message = new Requests.OpenAIMessage
                    {
                        Role = OpenAIMessageRole.Assistant,
                        Content = ExtractCompletionContent(completionResponse)
                    },
                    FinishReason = DetermineFinishReason(completionResponse)
                }
            },
            Usage = new OpenAIUsage
            {
                PromptTokens = completionResponse.PromptTokens,
                CompletionTokens = completionResponse.CompletionTokens,
                TotalTokens = completionResponse.TotalTokens
            }
        };

        return response;
    }

    /// <summary>
    /// Extracts the completion content from a FoundationaLLM response.
    /// </summary>
    private string ExtractCompletionContent(CompletionResponse completionResponse)
    {
        // Prefer completion field, fall back to content array
        if (!string.IsNullOrWhiteSpace(completionResponse.Completion))
        {
            return completionResponse.Completion;
        }

        if (completionResponse.Content != null && completionResponse.Content.Count > 0)
        {
            // Extract text from content items
            var textParts = new List<string>();
            foreach (var item in completionResponse.Content)
            {
                if (item is TextMessageContentItem textItem && !string.IsNullOrWhiteSpace(textItem.Value))
                {
                    textParts.Add(textItem.Value);
                }
            }

            if (textParts.Count > 0)
            {
                return string.Join(" ", textParts);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Determines the finish reason from a FoundationaLLM response.
    /// </summary>
    private string? DetermineFinishReason(CompletionResponse completionResponse)
    {
        // Check for errors
        if (completionResponse.IsError == true || 
            (completionResponse.Errors != null && completionResponse.Errors.Length > 0))
        {
            return OpenAIFinishReason.Stop; // Default to stop on error
        }

        // If we have tool calls or other indicators, we could check here
        // For now, default to stop
        return OpenAIFinishReason.Stop;
    }
}
