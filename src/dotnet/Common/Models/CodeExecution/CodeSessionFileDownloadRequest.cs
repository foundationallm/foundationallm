using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Represents a request to download newly created files from a code session.
    /// </summary>
    public class CodeSessionFileDownloadRequest
    {
        /// <summary>
        /// Gets or sets the file upload operation identifier.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public required string OperationId { get; set; }
    }
}
