using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Represents a request to upload files to a code session.
    /// </summary>
    public class CodeSessionFileUploadRequest
    {
        /// <summary>
        /// Gets or sets the list of file names to upload.
        /// </summary>
        [JsonPropertyName("file_names")]
        public required List<string> FileNames { get; set; }
    }
}
