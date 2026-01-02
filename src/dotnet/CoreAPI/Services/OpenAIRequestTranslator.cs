using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.OpenAI.Requests;
using FoundationaLLM.Common.Models.OpenAI.Shared;
using FoundationaLLM.Common.Models.Orchestration.Request;
using System.Text.Json;

namespace FoundationaLLM.Core.API.Services;

/// <summary>
/// Translates OpenAI requests to FoundationaLLM requests.
/// </summary>
public class OpenAIRequestTranslator : IOpenAIRequestTranslator
{
    /// <summary>
    /// Translates an OpenAI chat completion request to a FoundationaLLM completion request.
    /// </summary>
    public CompletionRequest TranslateChatCompletionRequest(OpenAIChatCompletionRequest openAIRequest, string instanceId)
    {
        var request = new CompletionRequest
        {
            OperationId = Guid.NewGuid().ToString().ToLower(),
            AgentName = ExtractAgentName(openAIRequest.Model),
            UserPrompt = ExtractUserPrompt(openAIRequest.Messages),
            MessageHistory = TranslateMessageHistory(openAIRequest.Messages),
            Settings = new OrchestrationSettings
            {
                ModelParameters = new Dictionary<string, object>()
            }
        };

        // Add model parameters
        if (openAIRequest.Temperature.HasValue)
        {
            request.Settings.ModelParameters![ModelParametersKeys.Temperature] = openAIRequest.Temperature.Value;
        }
        if (openAIRequest.TopP.HasValue)
        {
            request.Settings.ModelParameters![ModelParametersKeys.TopP] = openAIRequest.TopP.Value;
        }
        if (openAIRequest.MaxTokens.HasValue)
        {
            request.Settings.ModelParameters![ModelParametersKeys.MaxNewTokens] = openAIRequest.MaxTokens.Value;
        }
        // Note: PresencePenalty and FrequencyPenalty are OpenAI-specific and may not map directly
        // They could be added as custom parameters if the underlying model supports them

        // Handle stop sequences
        if (openAIRequest.Stop != null)
        {
            var stopSequences = openAIRequest.Stop switch
            {
                string str => new[] { str },
                JsonElement element when element.ValueKind == JsonValueKind.Array => 
                    element.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                _ => Array.Empty<string>()
            };
            
            if (stopSequences.Length > 0)
            {
                request.Settings.ModelParameters!["stop"] = stopSequences;
            }
        }

        return request;
    }

    /// <summary>
    /// Extracts the agent name from the OpenAI model identifier.
    /// For now, we use the model name directly as the agent name.
    /// In the future, this could be enhanced with a mapping configuration.
    /// </summary>
    private string ExtractAgentName(string model)
    {
        // Remove common OpenAI prefixes if present
        if (model.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase) ||
            model.StartsWith("foundationallm:", StringComparison.OrdinalIgnoreCase))
        {
            // For now, use the model name as-is
            // TODO: Implement model-to-agent mapping configuration
        }
        
        return model;
    }

    /// <summary>
    /// Extracts the user prompt from OpenAI messages.
    /// The user prompt is the content of the last user message.
    /// </summary>
    private string ExtractUserPrompt(List<OpenAIMessage> messages)
    {
        // Find the last user message
        var lastUserMessage = messages
            .Where(m => m.Role == OpenAIMessageRole.User)
            .LastOrDefault();

        if (lastUserMessage?.Content == null)
        {
            return string.Empty;
        }

        return ExtractContentAsString(lastUserMessage.Content);
    }

    /// <summary>
    /// Translates OpenAI messages to FoundationaLLM message history.
    /// </summary>
    private List<MessageHistoryItem> TranslateMessageHistory(List<OpenAIMessage> messages)
    {
        var history = new List<MessageHistoryItem>();

        foreach (var message in messages)
        {
            // Skip system messages (they're handled in agent configuration)
            if (message.Role == OpenAIMessageRole.System)
            {
                continue;
            }

            // Skip the last user message (it's the current prompt)
            if (message.Role == OpenAIMessageRole.User && message == messages.Last(m => m.Role == OpenAIMessageRole.User))
            {
                continue;
            }

            var content = ExtractContentAsString(message.Content);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var sender = message.Role switch
            {
                OpenAIMessageRole.User => "User",
                OpenAIMessageRole.Assistant => "Agent",
                OpenAIMessageRole.Tool => "Tool",
                _ => "User"
            };

            history.Add(new MessageHistoryItem(sender, content, null));
        }

        return history;
    }

    /// <summary>
    /// Extracts content as a string from an OpenAI message content object.
    /// </summary>
    private string ExtractContentAsString(object? content)
    {
        if (content == null)
        {
            return string.Empty;
        }

        if (content is string str)
        {
            return str;
        }

        if (content is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? string.Empty;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                // Extract text from content parts array
                var textParts = new List<string>();
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("type", out var typeElement))
                    {
                        var type = typeElement.GetString();
                        if (type == "text" && item.TryGetProperty("text", out var textElement))
                        {
                            textParts.Add(textElement.GetString() ?? string.Empty);
                        }
                    }
                }
                return string.Join(" ", textParts);
            }
        }

        return content.ToString() ?? string.Empty;
    }
}
