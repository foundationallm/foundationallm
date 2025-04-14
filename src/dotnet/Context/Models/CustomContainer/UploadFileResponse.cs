using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Models.CustomContainer
{
    /// <summary>
    /// Response model for file upload.
    /// </summary>
    public class UploadFileResponse : StatusResponse
    {
        /// <summary>
        /// Gets or sets the name of the file that was uploaded.
        /// </summary>
        [JsonPropertyName("file_name")]
        public required string FileName { get; set; }
    }
}
