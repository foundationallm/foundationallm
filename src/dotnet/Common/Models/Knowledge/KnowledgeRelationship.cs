using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a relationship between two knowledge entities in a knowledge graph.
    /// </summary>
    public class KnowledgeRelationship
    {
        /// <summary>
        /// Gets or sets the position of the relationship in the list of relationships.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonPropertyOrder(0)]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the source entity of the relationship.
        /// </summary>
        [JsonPropertyName("source")]
        [JsonPropertyOrder(1)]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the type of the source entity.
        /// </summary>
        [JsonPropertyName("source_type")]
        [JsonPropertyOrder(2)]
        public string SourceType { get; set; }

        /// <summary>
        /// Gets or sets the target entity of the relationship.
        /// </summary>
        [JsonPropertyName("target")]
        [JsonPropertyOrder(3)]
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the type of the target entity.
        /// </summary>
        [JsonPropertyName("target_type")]
        [JsonPropertyOrder(4)]
        public string TargetType { get; set; }

        /// <summary>
        /// Gets or sets the list of short descriptions associated with the relationship.
        /// </summary>
        [JsonPropertyName("short_descriptions")]
        [JsonPropertyOrder(5)]
        public List<string> ShortDescriptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of detailed descriptions associated with the relationship.
        /// </summary>
        [JsonPropertyName("descriptions")]
        [JsonPropertyOrder(6)]
        public List<string> Descriptions { get; set; } = [];

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

        /// <summary>
        /// Gets or sets the embedding of the summary description.
        /// </summary>
        [JsonPropertyName("summary_description_embedding")]
        [JsonPropertyOrder(9)]
        public float[]? SummaryDescriptionEmbedding { get; set; }

        /// <summary>
        /// Gets the unique identifier for the source, combining the source type and source value.
        /// </summary>
        [JsonIgnore]
        public string SourceUniqueId => $"{SourceType}:{Source}";

        /// <summary>
        /// Gets the unique identifier for the target, combining the target type and target value.
        /// </summary>
        [JsonIgnore]
        public string TargetUniqueId => $"{TargetType}:{Target}";

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeRelationship"/> class.
        /// </summary>
        public KnowledgeRelationship()
        {
            Source = null!;
            SourceType = null!;
            Target = null!;
            TargetType = null!;
            ShortDescriptions = [];
            Descriptions = [];
        }
    }
}
