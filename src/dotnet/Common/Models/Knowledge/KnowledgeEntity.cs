using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a knowledge entity used in a knowledge graph.
    /// </summary>
    public class KnowledgeEntity
    {
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(1)]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonPropertyOrder(2)]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of descriptions associated with the entity.
        /// </summary>
        [JsonPropertyName("descriptions")]
        [JsonPropertyOrder(3)]
        public required List<string> Descriptions { get; set; }

        /// <summary>
        /// Gets or sets the list of text chunk identifiers associated with the entity.
        /// </summary>
        [JsonPropertyName("chunk_ids")]
        [JsonPropertyOrder(4)]
        public required List<string> ChunkIds { get; set; }

        /// <summary>
        /// Gets or sets the summary description of the entity, which provides a concise overview.
        /// </summary>
        [JsonPropertyName("summary_description")]
        [JsonPropertyOrder(5)]
        public string? SummaryDescription { get; set; }
    }
}
