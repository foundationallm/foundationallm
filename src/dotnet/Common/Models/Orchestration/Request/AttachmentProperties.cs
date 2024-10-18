using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request
{
    /// <summary>
    /// Provides a set of properties that are used to define an attachment.
    /// </summary>
    public class AttachmentProperties
    {
        /// <summary>
        /// The original file name of the attachment.
        /// </summary>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <summary>
        /// The content type of the attachment.
        /// </summary>
        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }

        /// <summary>
        /// The name of the provider that is used to manage the attachment.
        /// </summary>
        /// <remarks>
        /// The following file providers are supported:
        /// <list type="bullet">
        /// <item>FoundationaLLM.Attachment</item>
        /// <item>FoundationaLLM.AzureOpenAI</item>
        /// </list>
        /// </remarks>
        [JsonPropertyName("provider")]
        public required string Provider { get; set; }

        /// <summary>
        /// The file name of the attachment that is stored by the provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the case of the FoundationaLLM.Attachment provider, this is the full path of the file in the storage account.
        /// </para>
        /// <para>
        /// In the case of the FoundationaLLM.AzureOpenAI provider, this is the OpenAI file ID.
        /// </para>
        /// </remarks>
        [JsonPropertyName("provider_file_name")]
        public required string ProviderFileName { get; set; }

        /// <summary>
        /// The name of the storage account that is used by the provider.
        /// </summary>
        [JsonPropertyName("provider_storage_account_name")]
        public string? ProviderStorageAccountName { get; set; }

    }
}
