using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Defines the properties of a resource.
    /// </summary>
    public class ResourceObjectIdProperties
    {
         /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        public required string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of properties.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = [];
    }
}
