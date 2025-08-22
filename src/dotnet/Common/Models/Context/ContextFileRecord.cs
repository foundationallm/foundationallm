using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a file record.
    /// </summary>
    public class ContextFileRecord : ContextRecord
    {
        /// <summary>
        /// Gets or sets the type of the context record.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-99)]
        public override string Type { get; set; } =
            ContextRecordTypeNames.FileRecord;

        /// <summary>
        /// Gets or sets the conversation identifier.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        [JsonPropertyOrder(0)]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the agent name.
        /// </summary>
        [JsonPropertyName("agent_name")]
        [JsonPropertyOrder(1)]
        public string AgentName { get; set; }

        /// <summary>
        /// Gets or sets the FoundationaLLM object identifier of the file
        /// </summary>
        [JsonPropertyName("file_object_id")]
        [JsonPropertyOrder(2)]
        public string FileObjectId { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        [JsonPropertyName("file_name")]
        [JsonPropertyOrder(3)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        [JsonPropertyName("content_type")]
        [JsonPropertyOrder(4)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the file path on the storage account.
        /// </summary>
        [JsonPropertyName("file_path")]
        [JsonPropertyOrder(5)]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        [JsonPropertyName("file_size_bytes")]
        [JsonPropertyOrder(6)]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of processing that is required for the file.
        /// </summary>
        [JsonPropertyName("file_processing_type")]
        [JsonPropertyOrder(7)]
        public string FileProcessingType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextFileRecord"/> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ContextFileRecord()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            // Required for deserialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextFileRecord"/> class.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="origin">The origin of the record.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="fileName">The original name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="fileSizeBytes">The size of the file in bytes.</param>
        /// <param name="fileProcessingType">The type of processing that is required for the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <param name="metadata">Optional metadata dictionary associated with the context file record.</param>
        public ContextFileRecord(
            string instanceId,
            string origin,
            string? conversationId,
            string? agentName,
            string fileName,
            string contentType,
            long fileSizeBytes,
            string fileProcessingType,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata = null) : base(
                string.Empty,
                instanceId,
                origin,
                userIdentity,
                metadata)
        {
            var fileId = $"file-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
            var fileObjectId = $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Context}/files/{fileId}";

            var filePath = string.IsNullOrEmpty(conversationId)
                ? $"file/agents/{agentName}/{fileId}{Path.GetExtension(fileName)}"
                : $"file/users/{userIdentity.UPN!.NormalizeUserPrincipalName()}/{conversationId}/{fileId}{Path.GetExtension(fileName)}";

            Id = fileId;
            ConversationId = conversationId ?? "N/A";
            AgentName = agentName ?? "N/A";
            FileObjectId = fileObjectId;
            FileName = fileName;
            ContentType = contentType;
            FilePath = filePath;
            FileSizeBytes = fileSizeBytes;
            FileProcessingType = fileProcessingType;
        }
    }
}
