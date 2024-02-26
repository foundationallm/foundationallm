using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProvider
{
    /// <summary>
    /// Basic properties for all resources.
    /// </summary>
    public class ResourceBase : ResourceName
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        [JsonPropertyOrder(-4)]
        public string? ObjectId { get; set; }

        /// <summary>
        /// The description of the resource.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonPropertyOrder(-3)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether the resource has been logically deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        [JsonPropertyOrder(-3)]
        public bool Deleted { get; set; } = false;
    }
}
