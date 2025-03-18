using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request;

/// <summary>
/// OpenAI-compatible chat completion request object.
/// </summary>
public class OpenAIFacadeChatCompletionRequest
{
    /// <summary>
    /// ID of the model to use.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The messages to generate chat completions for.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<OpenAIFacadeChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// The maximum number of tokens to generate.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing
    /// frequency in the text so far.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they
    /// appear in the text so far.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// Converts the OpenAI facade chat completion request to the internal CompletionRequest format.
    /// </summary>
    /// <returns>A CompletionRequest object populated with the facade request data.</returns>
    public CompletionRequest ToCompletionRequest()
    {
        // Combine all messages into a single prompt, maintaining conversation context
        var combinedPrompt = string.Join("\n", Messages.Select(m => $"{m.Role}: {m.Content}"));

        return new CompletionRequest
        {
            AgentName = Model,
            UserPrompt = combinedPrompt,
            SessionId = Guid.NewGuid().ToString(), // Chat completions should use session ID
            Settings = new OrchestrationSettings
            {
                ModelParameters = new Dictionary<string, object>()
                {
                    { ModelParametersKeys.MaxTokens, MaxTokens! },
                    { ModelParametersKeys.Temperature, Temperature! },
                    { ModelParametersKeys.FrequencyPenalty, FrequencyPenalty! },
                    { ModelParametersKeys.PresencePenalty, PresencePenalty! }
                }
                
            }
        };
    }
} 
