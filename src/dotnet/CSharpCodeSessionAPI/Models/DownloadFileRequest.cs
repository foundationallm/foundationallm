using System.Text.Json.Serialization;

namespace FoundationaLLM.CSharpCodeSession.API.Models
{
    /// <summary>
    /// Represents a request to download a file.
    /// </summary>
    public class DownloadFileRequest
    {
        /// <summary>
        /// Gets or sets the file name to be downloaded.
        /// </summary>
        [JsonPropertyName("file_name")]
        public required string FileName { get; set; }
    }
}
