using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a relationship between two knowledge entities in a knowledge graph.
    /// </summary>
    public class KnowledgeRelationship
    {
        /// <summary>
        /// Gets or sets the source entity of the relationship.
        /// </summary>
        [JsonPropertyName("source")]
        [JsonPropertyOrder(1)]
        public required string Source { get; set; }

        /// <summary>
        /// Gets or sets the type of the source entity.
        /// </summary>
        [JsonPropertyName("source_type")]
        [JsonPropertyOrder(2)]
        public required string SourceType { get; set; }

        /// <summary>
        /// Gets or sets the target entity of the relationship.
        /// </summary>
        [JsonPropertyName("target")]
        [JsonPropertyOrder(3)]
        public required string Target { get; set; }

        /// <summary>
        /// Gets or sets the type of the target entity.
        /// </summary>
        [JsonPropertyName("target_type")]
        [JsonPropertyOrder(4)]
        public required string TargetType { get; set; }

        /// <summary>
        /// Gets or sets the list of short descriptions associated with the relationship.
        /// </summary>
        [JsonPropertyName("short_descriptions")]
        [JsonPropertyOrder(5)]
        public required List<string> ShortDescriptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of detailed descriptions associated with the relationship.
        /// </summary>
        [JsonPropertyName("descriptions")]
        [JsonPropertyOrder(6)]
        public required List<string> Descriptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of strengths associated with the relationship.
        /// </summary>
        [JsonPropertyName("strengths")]
        [JsonPropertyOrder(7)]
        public List<int> Strengths { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of chunk identifiers associated with the relationship.
        /// </summary>
        [JsonPropertyName("chunk_ids")]
        [JsonPropertyOrder(7)]
        public List<string> ChunkIds { get; set; } = [];

        /// <summary>
        /// Gets or sets the summary description of the relationship, which provides a concise overview.
        /// </summary>
        [JsonPropertyName("summary_description")]
        [JsonPropertyOrder(8)]
        public string? SummaryDescription { get; set; }
    }
}
