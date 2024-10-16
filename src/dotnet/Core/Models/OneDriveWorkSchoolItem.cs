using System.Text.Json.Serialization;

namespace FoundationaLLM.Core.Models
{
    /// <summary>
    /// OneDriveWorkSchool object.
    /// </summary>
    public class OneDriveWorkSchoolItem
    {
        /// <summary>
        /// The item unique identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// The drive unique identifier.
        /// </summary>
        [JsonPropertyName("driveId")]
        public string? DriveId { get; set; }

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
        /// The OneDrive file mime type.
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        /// <summary>
        /// The access token required to fetch the item contents.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
