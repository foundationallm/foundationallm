using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Requests;

/// <summary>
/// Represents a tool (function) definition in an OpenAI chat completion request.
/// </summary>
public class OpenAITool
{
    /// <summary>
    /// The type of tool. Currently only "function" is supported.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// The function definition.
    /// </summary>
    [JsonPropertyName("function")]
    public required OpenAIFunction Function { get; set; }
}

/// <summary>
/// Represents a function definition within a tool.
/// </summary>
public class OpenAIFunction
{
    /// <summary>
    /// The name of the function.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// A description of what the function does.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The parameters the function accepts, described as a JSON Schema object.
    /// </summary>
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }
}
