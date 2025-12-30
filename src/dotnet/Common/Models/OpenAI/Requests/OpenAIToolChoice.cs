using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Requests;

/// <summary>
/// Represents tool choice configuration in an OpenAI chat completion request.
/// </summary>
public class OpenAIToolChoice
{
    /// <summary>
    /// The type of tool choice. Can be "none", "auto", or "required".
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// The function to call (when type is "function").
    /// </summary>
    [JsonPropertyName("function")]
    public OpenAIToolChoiceFunction? Function { get; set; }
}

/// <summary>
/// Represents a function choice within tool choice.
/// </summary>
public class OpenAIToolChoiceFunction
{
    /// <summary>
    /// The name of the function to call.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
