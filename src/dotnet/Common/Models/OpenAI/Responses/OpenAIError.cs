using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Responses;

/// <summary>
/// Represents an OpenAI error response.
/// </summary>
public class OpenAIError
{
    /// <summary>
    /// The error message.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// The type of error.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// The parameter that caused the error (if applicable).
    /// </summary>
    [JsonPropertyName("param")]
    public string? Param { get; set; }

    /// <summary>
    /// The error code.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
