using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Models.CustomContainer
{
    /// <summary>
    /// Represents the response for listing files.
    /// </summary>
    public class ListFilesResponse
    {
        /// <summary>
        /// Gets or sets the list of files.
        /// </summary>
        [JsonPropertyName("files")]
        public List<string> Files { get; set; } = [];
    }
}
