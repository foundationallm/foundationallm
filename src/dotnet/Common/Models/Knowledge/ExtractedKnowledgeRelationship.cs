using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents an extracted relationship from a knowledge extraction process.
    /// </summary>
    public class ExtractedKnowledgeRelationship
    {
        /// <summary>
        /// Gets or sets the source entity of the relationship.
        /// </summary>
        [JsonPropertyName("source")]
        public required string Source { get; set; }

        /// <summary>
        /// Gets or sets the target entity of the relationship.
        /// </summary>
        [JsonPropertyName("target")]
        public required string Target { get; set; }

        /// <summary>
        /// Gets or sets the short description of the relationship.
        /// </summary>
        [JsonPropertyName("short_description")]
        public required string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the relationship.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the strength of the relationship.
        /// </summary>
        [JsonPropertyName("strength")]
        public int Strength { get; set; }
    }
}
