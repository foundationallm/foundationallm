using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Responses;

/// <summary>
/// Represents a streaming chunk in an OpenAI chat completion response.
/// </summary>
public class OpenAIChatCompletionChunk
{
    /// <summary>
    /// A unique identifier for the chat completion.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// The object type, which is always "chat.completion.chunk".
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = "chat.completion.chunk";

    /// <summary>
    /// The Unix timestamp (in seconds) of when the chunk was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// The model used for the chat completion.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// The list of completion choices in this chunk.
    /// </summary>
    [JsonPropertyName("choices")]
    public required List<OpenAIChatCompletionChoice> Choices { get; set; }

    /// <summary>
    /// This fingerprint represents the backend configuration that the model runs with.
    /// </summary>
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}
