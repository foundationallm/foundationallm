using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Represents an attachment in a chat message or session.
    /// </summary>
    public class AttachmentDetail
    {
        /// <summary>
        /// The unique identifier of the attachment resource.
        /// </summary>
        [JsonPropertyName("objectId")]
        public string? ObjectId { get; set; }

        /// <summary>
        /// The attachment file name.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The mime content type of the attachment.
        /// </summary>
        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Creates an <see cref="AttachmentDetail"/> instance from an <see cref="AttachmentFile"/> instance.
        /// </summary>
        /// <param name="attachmentFile">The <see cref="AttachmentFile"/> used to initialize the instance.</param>
        /// <returns>The newly created <see cref="AttachmentDetail"/> instance.</returns>
        public static AttachmentDetail FromAttachmentFile(AttachmentFile attachmentFile) => new()
        {
            ObjectId = attachmentFile.ObjectId,
            DisplayName = !string.IsNullOrWhiteSpace(attachmentFile.DisplayName) ? attachmentFile.DisplayName : attachmentFile.OriginalFileName,
            ContentType = attachmentFile.ContentType
        };

        /// <summary>
        /// Creates an <see cref="AttachmentDetail"/> instance from a <see cref="ContextFileRecord"/> instance.
        /// </summary>
        /// <param name="contextFileRecord">The <see cref="ContextFileRecord"/> used to initialize the instance.</param>
        /// <returns>The newly created <see cref="AttachmentDetail"/> instance.</returns>
        public static AttachmentDetail FromContextFileRecord(ContextFileRecord contextFileRecord) =>
            contextFileRecord is null
            ? new()
            {
                ObjectId = "__unavailable__",
                DisplayName = "(unavailable)",
                ContentType = string.Empty
            }
            : new()
            {
                ObjectId = contextFileRecord.FileObjectId,
                DisplayName = contextFileRecord.FileName,
                ContentType = contextFileRecord.ContentType
            };
    }
}
