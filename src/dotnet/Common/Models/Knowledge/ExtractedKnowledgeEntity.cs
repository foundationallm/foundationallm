using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents an extracted entity from a knowledge extraction process.
    /// </summary>
    public class ExtractedKnowledgeEntity
    {
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the entity.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of chunk ids associated with the entity.
        /// </summary>
        [JsonPropertyName("chunk_ids")]
        public List<string>? ChunkIds { get; set; }
    }
}
