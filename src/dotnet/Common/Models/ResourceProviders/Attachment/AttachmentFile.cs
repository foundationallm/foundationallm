using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Attachment
{
    /// <summary>
    /// Attachment resource.
    /// </summary>
    public class AttachmentFile : ResourceBase
    {
        /// <summary>
        /// The type of the resource.
        /// </summary>
        [JsonIgnore]
        public override string? Type { get; set; } = nameof(AttachmentFile);

        /// <summary>
        /// File stream of the attachment contents.
        /// </summary>
        [JsonPropertyName("content")]
        public BinaryData? Content { get; set; }

        /// <summary>
        /// The mime content type of the attachment.
        /// </summary>
        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Path for the attachment, starting with the container name.
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = "";

        /// <summary>
        /// The original name of the file (as it was uploaded).
        /// </summary>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the secondary provider of the attachment.
        /// </summary>
        /// <remarks>
        /// The only secondary provider currently supported is FoundationaLLM.AzureOpenAI.
        /// </remarks>
        [JsonPropertyName("secondary_provider")]
        public string? SecondaryProvider { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the secondary provider of the attachment.
        /// </summary>
        /// <remarks>
        /// The only secondary provider currently supported is FoundationaLLM.AzureOpenAI.
        /// In this case, the object identifier is the Azure OpenAI file identifier.
        /// </remarks>
        [JsonPropertyName("secondary_provider_object_id")]
        public string? SecondaryProviderObjectId { get; set; }

    }
}
