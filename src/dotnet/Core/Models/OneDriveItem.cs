using System.Text.Json.Serialization;

namespace FoundationaLLM.Core.Models
{
    /// <summary>
    /// OneDrive item object.
    /// </summary>
    public class OneDriveItem
    {
        /// <summary>
        /// The item unique identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// The id of the object that was created or updated.
        /// </summary>
        public string? ObjectId { get; set; }

        /// <summary>
        /// The item name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The file object.
        /// </summary>
        [JsonPropertyName("file")]
        public OneDriveFile? File { get; set; }

        /// <summary>
        /// The access token required to fetch the item contents.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    /// <summary>
    /// OneDrive file object.
    /// </summary>
    public class OneDriveFile
    {
        /// <summary>
        /// The OneDrive file mime type.
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }
}
