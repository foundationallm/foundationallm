using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Requests;

/// <summary>
/// Represents an OpenAI chat completion request.
/// </summary>
public class OpenAIChatCompletionRequest
{
    /// <summary>
    /// The model to use for the completion.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// The messages to generate completions for.
    /// </summary>
    [JsonPropertyName("messages")]
    public required List<OpenAIMessage> Messages { get; set; }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// An alternative to sampling with temperature, called nucleus sampling.
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    /// The maximum number of tokens to generate.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// Up to 4 sequences where the API will stop generating further tokens.
    /// </summary>
    [JsonPropertyName("stop")]
    public object? Stop { get; set; } // string or string[]

    /// <summary>
    /// Whether to stream back partial progress.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    /// <summary>
    /// A list of tools the model may call.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<OpenAITool>? Tools { get; set; }

    /// <summary>
    /// Controls which (if any) tool is called by the model.
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; } // string or OpenAIToolChoice

    /// <summary>
    /// An object specifying the format that the model must output.
    /// </summary>
    [JsonPropertyName("response_format")]
    public OpenAIResponseFormat? ResponseFormat { get; set; }

    /// <summary>
    /// How many chat completion choices to generate for each input message.
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// A unique identifier representing your end-user.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }
}
