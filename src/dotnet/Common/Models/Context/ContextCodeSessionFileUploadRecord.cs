using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a file record.
    /// </summary>
    public class ContextCodeSessionFileUploadRecord : ContextRecord
    {
        /// <summary>
        /// Gets or sets the type of the context record.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-99)]
        public override string Type { get; set; } =
            ContextRecordTypeNames.CodeSessionFileUploadRecord;

        /// <summary>
        /// Gets or sets the code session identifier
        /// </summary>
        [JsonPropertyName("code_session_id")]
        [JsonPropertyOrder(0)]
        public string CodeSessionId { get; set; }

        /// <summary>
        /// Gets or sets the dictionary with the file upload success status for each file.
        /// </summary>
        [JsonPropertyName("file_upload_success")]
        [JsonPropertyOrder(1)]
        public Dictionary<string, bool> FileUploadSuccess { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCodeSessionFileUploadRecord"/> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ContextCodeSessionFileUploadRecord()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            // Required for deserialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCodeSessionFileUploadRecord"/> class.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="codeSessionId">The code session unique identifier.</param>
        /// <param name="fileUploadSuccess">The dictionary with the file upload success status for each file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        public ContextCodeSessionFileUploadRecord(
            string instanceId,
            string codeSessionId,
            Dictionary<string, bool> fileUploadSuccess,
            UnifiedUserIdentity userIdentity) : base(
                Guid.NewGuid().ToString().ToLowerInvariant(),
                instanceId,
                userIdentity)
        {
            CodeSessionId = codeSessionId;
            FileUploadSuccess = fileUploadSuccess;
        }
    }
}
