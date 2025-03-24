using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides the result of uploading files to a code session.
    /// </summary>
    public class CodeSessionFileUploadResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the file upload operation
        /// </summary>
        [JsonPropertyName("operation_id")]
        public required string OperationId { get; set; }

        /// <summary>
        /// Gets or sets the dictionary containing the success status of each file upload operation.
        /// </summary>
        [JsonPropertyName("file_upload_success")]
        public Dictionary<string, bool> FileUploadSuccess { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether all files were uploaded successfully.
        /// </summary>
        [JsonPropertyName("all_files_uploaded")]
        public bool AllFilesUploaded { get; set; }
    }
}
