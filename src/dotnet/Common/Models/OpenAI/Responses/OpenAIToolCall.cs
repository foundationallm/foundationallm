using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Responses;

/// <summary>
/// Represents a tool call made by the assistant.
/// </summary>
public class OpenAIToolCall
{
    /// <summary>
    /// The ID of the tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// The type of tool call. Currently only "function" is supported.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// The function that was called.
    /// </summary>
    [JsonPropertyName("function")]
    public required OpenAIToolCallFunction Function { get; set; }
}

/// <summary>
/// Represents the function details within a tool call.
/// </summary>
public class OpenAIToolCallFunction
{
    /// <summary>
    /// The name of the function that was called.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// The arguments passed to the function, as a JSON string.
    /// </summary>
    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }
}
