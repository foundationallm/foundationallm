using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Requests;

/// <summary>
/// Represents response format configuration in an OpenAI chat completion request.
/// </summary>
public class OpenAIResponseFormat
{
    /// <summary>
    /// The type of response format. Can be "text" or "json_object".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    /// <summary>
    /// JSON schema for structured output (when type is "json_object").
    /// </summary>
    [JsonPropertyName("json_schema")]
    public object? JsonSchema { get; set; }
}
