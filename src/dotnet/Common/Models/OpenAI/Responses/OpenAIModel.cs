using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Responses;

/// <summary>
/// Represents an OpenAI model.
/// </summary>
public class OpenAIModel
{
    /// <summary>
    /// The model identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// The object type, which is always "model".
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = "model";

    /// <summary>
    /// The Unix timestamp (in seconds) when the model was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// The organization that owns the model.
    /// </summary>
    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = "foundationallm";
}
