using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response;

/// <summary>
/// Encapsulates data about the sources used in building a completion response.
/// </summary>
public class Citation
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
    /// The source of the source information. Typically a tool name.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// The content of the citation
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// The format of the citation, such as html or markdown.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// The type of the citation. Specific to the source.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The supporting metadata of the citation.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}
