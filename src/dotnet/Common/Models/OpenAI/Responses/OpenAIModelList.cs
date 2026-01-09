using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.OpenAI.Responses;

/// <summary>
/// Represents a list of OpenAI models.
/// </summary>
public class OpenAIModelList
{
    /// <summary>
    /// The object type, which is always "list".
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    /// <summary>
    /// The list of models.
    /// </summary>
    [JsonPropertyName("data")]
    public required List<OpenAIModel> Data { get; set; }
}
