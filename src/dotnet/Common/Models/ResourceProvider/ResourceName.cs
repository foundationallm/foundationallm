using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProvider
{
    /// <summary>
    /// A named resource with a type.
    /// </summary>
    public class ResourceName
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonPropertyOrder(-5)]
        public required string Name { get; set; }
        /// <summary>
        /// The type of the resource.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-100)]
        public virtual string? Type { get; set; }
    }
}
