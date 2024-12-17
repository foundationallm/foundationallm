using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response;

/// <summary>
/// Encapsulates data about the sources used in building a completion response.
/// </summary>
public class ContentArtifact
{
    /// <summary>
    /// The index identifier of the document containing the source information.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The title of the document containing the source information.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The location of the source information.
    /// </summary>
    [JsonPropertyName("filepath")]
    public string? Filepath { get; set; }

    /// <summary>
    /// The source of the content.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Textual content.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// The type of the content.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The metadata associated with the content.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}
