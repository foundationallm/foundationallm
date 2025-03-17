using System.Text.Json.Serialization;
using System.Collections.Generic;
using FoundationaLLM.Common.Constants.Agents;

namespace FoundationaLLM.Common.Models.Orchestration.Request;

/// <summary>
/// OpenAI-compatible completion request object.
/// </summary>
public class OpenAIFacadeCompletionRequest
{
    /// <summary>
    /// ID of the model to use.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The prompt to generate completions for.
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// The maximum number of tokens to generate in the completion.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// The suffix that comes after a completion of inserted text.
    /// </summary>
    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing
    /// frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they
    /// appear in the text so far, increasing the model's likelihood to talk about new topics.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// Converts the OpenAI facade completion request to the internal CompletionRequest format.
    /// </summary>
    /// <returns>A CompletionRequest object populated with the facade request data.</returns>
    public CompletionRequest ToCompletionRequest() => new()
    {
        AgentName = Model,  // Map the model name to our agent name
        UserPrompt = Prompt,  // Map to the required UserPrompt field
        Settings = new OrchestrationSettings
        {
            ModelParameters = new Dictionary<string, object>()
            {
                { ModelParametersKeys.MaxNewTokens, MaxTokens ?? 100 },
                { ModelParametersKeys.Temperature, Temperature ?? 0.7f },
                { ModelParametersKeys.FrequencyPenalty, FrequencyPenalty ?? 0.0f },
                { ModelParametersKeys.PresencePenalty, PresencePenalty ?? 0.0f }
            }
        }
    };
} 
