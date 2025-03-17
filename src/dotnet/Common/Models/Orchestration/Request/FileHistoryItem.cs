using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request
{
    /// <summary>
    /// Represents a file history item in a conversation.
    /// </summary>
    public class FileHistoryItem
    {
        /// <summary>
        /// The order the file was uploaded in the current conversation.
        /// </summary>
        [JsonPropertyName("order")]
        public required int Order { get; set; }

        /// <summary>
        /// The original file name of the attachment.
        /// </summary>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <summary>
        /// The ObjectID of the file attachment resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        public required string ObjectId { get; set; }

        /// <summary>
        /// The file path of the attachment in storage.
        /// </summary>
        [JsonPropertyName("file_path")]
        public required string FilePath { get; set; }

        /// <summary>
        /// The content type of the attachment.
        /// </summary>
        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Creates an instance of FileHistoryItem based on an AttachmentFile.
        /// </summary>
        /// <param name="attachmentFile">The AttachmentFile resource.</param>
        /// <param name="Order">The order in which the file has appeared in the conversation.</param>
        /// <returns>The FileHistoryItem object based on the AttachmentFile.</returns>
        public static FileHistoryItem FromAttachmentFile(AttachmentFile attachmentFile, int Order) => new FileHistoryItem
        {
            Order = Order,
            OriginalFileName = attachmentFile.OriginalFileName,
            ObjectId = attachmentFile.ObjectId!,
            FilePath = attachmentFile.Path,
            ContentType = attachmentFile.ContentType
        };
    }
}
