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
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the target entity of the relationship.
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the short description of the relationship.
        /// </summary>
        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the relationship.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the strength of the relationship.
        /// </summary>
        [JsonPropertyName("strength")]
        public int Strength { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractedKnowledgeRelationship"/> class.
        /// </summary>
        public ExtractedKnowledgeRelationship()
        {
            Source = null!;
            Target = null!;
            ShortDescription = null!;
            Description = null!;
        }
    }
}
