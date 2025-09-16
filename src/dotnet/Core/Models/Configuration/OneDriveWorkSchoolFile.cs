using System.Text.Json.Serialization;

namespace FoundationaLLM.Core.Models.Configuration
{
    /// <summary>
    /// Provides OneDrive for Work or School file properties.
    /// </summary>
    public class OneDriveWorkSchoolFile
    {
        /// <summary>
        /// The OneDrive file mime type.
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }
}
