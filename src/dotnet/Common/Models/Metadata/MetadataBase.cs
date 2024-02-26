using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Metadata
{
    /// <summary>
    /// Metadata model base class.
    /// </summary>
    public class MetadataBase
    {
        /// <summary>
        /// Name property.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Type property.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Description property.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
