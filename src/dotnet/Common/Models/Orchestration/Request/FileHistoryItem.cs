using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Context;
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
        /// Indicates if the file is an attachment to the current message.
        /// </summary>
        [JsonPropertyName("current_message_attachment")]
        public required bool CurrentMessageAttachment { get; set; }

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
        /// The provider that manages the file.
        /// </summary>
        [JsonPropertyName("secondary_provider")]
        public string? SecondaryProvider { get; set; } = null;

        /// <summary>
        /// The identifier of the file attachment resource in the secondary provider.
        /// </summary>
        [JsonPropertyName("secondary_provider_object_id")]
        public string? SecondaryProviderObjectId { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the content of the file should be embedded in the request.
        /// </summary>
        [JsonPropertyName("embed_content_in_request")]
        public required bool EmbedContentInRequest { get; set; } 

        /// <summary>
        /// Creates an instance of FileHistoryItem based on an AttachmentFile.
        /// </summary>
        /// <param name="attachmentFile">The AttachmentFile resource.</param>
        /// <param name="Order">The order in which the file has appeared in the conversation.</param>
        /// <param name="currentMessageAttachment"> Indicates if the file is an attachment to the current message.</param>
        /// <returns>The FileHistoryItem object based on the AttachmentFile.</returns>
        public static FileHistoryItem FromAttachmentFile(AttachmentFile attachmentFile, int Order, bool currentMessageAttachment) =>
            new()
            {
                Order = Order,
                CurrentMessageAttachment = currentMessageAttachment,
                OriginalFileName = attachmentFile.OriginalFileName,
                ObjectId = attachmentFile.ObjectId!,
                FilePath = attachmentFile.Path,
                ContentType = attachmentFile.ContentType,
                SecondaryProvider = attachmentFile.SecondaryProvider,
                SecondaryProviderObjectId = attachmentFile.SecondaryProviderObjectId,
                EmbedContentInRequest = false
            };

        /// <summary>
        /// Creates an instance of FileHistoryItem based on a ContextFileRecord. Context files are managed entirely by FoundationaLLM.
        /// </summary>
        /// <param name="fileRecord">The ContextFileRecord to convert to FileHistoryItem.</param>
        /// <param name="order">The order in which the file appeared in the conversation.</param>
        /// <param name="currentMessageAttachment"> Indicates if the file is an attachment to the current message.</param>
        /// <param name="allowContentEmbeddingInRequest"> Indicates if the content of the file is allowed be embedded in the request.</param>
        /// <returns>The FileHistoryItem object based on the ContextFileRecord.</returns>
        /// <remarks>
        /// Content embedding in the request is only allowed for files with FileProcessingType of CompletionRequestContext.
        /// When <paramref name="allowContentEmbeddingInRequest"/> is false, the content will not be embedded regardless of the file processing type.
        /// This case handles files that are part of the message history but not directly attached to the current message.
        /// </remarks>
        public static FileHistoryItem FromContextFileRecord(
            ContextFileRecord fileRecord,
            int order,
            bool currentMessageAttachment,
            bool allowContentEmbeddingInRequest) =>
            new()
            {
                Order = order,
                CurrentMessageAttachment = currentMessageAttachment,
                OriginalFileName = fileRecord.FileName,
                ObjectId = fileRecord.FileObjectId,
                FilePath = fileRecord.FilePath,
                ContentType = fileRecord.ContentType,
                EmbedContentInRequest =
                    allowContentEmbeddingInRequest
                    && (fileRecord.FileProcessingType == FileProcessingTypes.CompletionRequestContext)
            };  
    }
}
