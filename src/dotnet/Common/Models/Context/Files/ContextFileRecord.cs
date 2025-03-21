using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a file record.
    /// </summary>
    public class ContextFileRecord
    {
        /// <summary>
        /// Gets or sets the Cosmos DB identifier.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonPropertyOrder(-6)]
        public string Id { get; set; } = Guid.NewGuid().ToString().ToLowerInvariant();

        /// <summary>
        /// Gets or sets the type of the context record.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-5)]
        public string Type { get; set; } = "file-record";

        /// <summary>
        /// Gets or sets the FoundationaLLM instance identifier.
        /// </summary>
        [JsonPropertyName("instance_id")]
        [JsonPropertyOrder(-4)]
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the conversation identifier.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        [JsonPropertyOrder(-3)]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the user prinicpal name of the user that owns the file.
        /// </summary>
        [JsonPropertyName("upn")]
        [JsonPropertyOrder(-2)]
        public string UPN { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        [JsonPropertyName("file_id")]
        [JsonPropertyOrder(-1)]
        public string FileId { get; set; }

        /// <summary>
        /// Gets or sets the FoundationaLLM object identifier of the file
        /// </summary>
        [JsonPropertyName("file_object_id")]
        [JsonPropertyOrder(0)]
        public string FileObjectId { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        [JsonPropertyName("file_name")]
        [JsonPropertyOrder(1)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        [JsonPropertyName("content_type")]
        [JsonPropertyOrder(2)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the file path on the storage account.
        /// </summary>
        [JsonPropertyName("file_path")]
        [JsonPropertyOrder(3)]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        [JsonPropertyName("file_size_bytes")]
        [JsonPropertyOrder(4)]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the time of the file creation.
        /// </summary>
        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(5)]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the flag that indicates if the file was deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        [JsonPropertyOrder(6)]
        public bool Deleted { get; set; }

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
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="fileName">The original name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="fileSizeBytes">The size of the file in bytes.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        public ContextFileRecord(
            string instanceId,
            string conversationId,
            string fileName,
            string contentType,
            long fileSizeBytes,
            UnifiedUserIdentity userIdentity)
        {
            var fileId = $"file-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
            var fileObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.ContextAPI/files/{fileId}";
            var filePath = $"{userIdentity.UPN!.NormalizeUserPrincipalName()}/{conversationId}/{fileId}{Path.GetExtension(fileName)}";

            InstanceId = instanceId;
            ConversationId = conversationId;
            UPN = userIdentity.UPN!.ToLowerInvariant();
            FileId = fileId;
            FileObjectId = fileObjectId;
            FileName = fileName;
            ContentType = contentType;
            FilePath = filePath;
            FileSizeBytes = fileSizeBytes;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
