using System.Text.Json.Serialization;

namespace FoundationaLLM.Core.Models
{
    /// <summary>
    /// OneDrive item.
    /// </summary>
    public class OneDriveItem
    {
        /// <summary>
        /// The item unique identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public required string ItemId { get; set; }

        /// <summary>
        /// The access token required to fetch the item contents.
        /// </summary>
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }
}
