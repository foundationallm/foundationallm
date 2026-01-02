using System.Text.Json.Serialization;
using FoundationaLLM.Common.Models.OpenAI.Responses;

namespace FoundationaLLM.Common.Models.OpenAI.Requests;

/// <summary>
/// Represents a message in an OpenAI chat completion request.
/// </summary>
public class OpenAIMessage
{
    /// <summary>
    /// The role of the message (system, user, assistant, or tool).
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    /// <summary>
    /// The content of the message. Can be a string or an array of content parts.
    /// </summary>
    [JsonPropertyName("content")]
    public object? Content { get; set; }

    /// <summary>
    /// The name of the author of this message (optional).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Tool calls made by the assistant (for assistant messages).
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<OpenAIToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// The ID of the tool call this message is responding to (for tool messages).
    /// </summary>
    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}
