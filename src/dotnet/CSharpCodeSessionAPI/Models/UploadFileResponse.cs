using System.Text.Json.Serialization;

namespace FoundationaLLM.CSharpCodeSession.API.Models
{
    /// <summary>
    /// Response model for file upload.
    /// </summary>
    public class UploadFileResponse
    {
        /// <summary>
        /// Gets or sets the status of the upload operation.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "success";

        /// <summary>
        /// Gets or sets the name of the file that was uploaded.
        /// </summary>
        [JsonPropertyName("file_name")]
        public required string FileName { get; set; }
    }
}
