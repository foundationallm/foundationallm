using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a knowledge entity used in a knowledge graph.
    /// </summary>
    public class KnowledgeEntity
    {
        /// <summary>
        /// Gets or sets the position of the entity in the list of entities.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonPropertyOrder(0)]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(1)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonPropertyOrder(2)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of descriptions associated with the entity.
        /// </summary>
        [JsonPropertyName("descriptions")]
        [JsonPropertyOrder(3)]
        public List<string> Descriptions { get; set; }

        /// <summary>
        /// Gets or sets the list of text chunk identifiers associated with the entity.
        /// </summary>
        [JsonPropertyName("chunk_ids")]
        [JsonPropertyOrder(4)]
        public List<string> ChunkIds { get; set; }

        /// <summary>
        /// Gets or sets the summary description of the entity, which provides a concise overview.
        /// </summary>
        [JsonPropertyName("summary_description")]
        [JsonPropertyOrder(5)]
        public string? SummaryDescription { get; set; }

        /// <summary>
        /// Gets or sets the embedding of the summary description.
        /// </summary>
        [JsonPropertyName("summary_description_embedding")]
        [JsonPropertyOrder(6)]
        public float[]? SummaryDescriptionEmbedding { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeEntity"/> class.
        /// </summary>
        public KnowledgeEntity()
        {
            Type = null!;
            Name = null!;
            Descriptions = [];
            ChunkIds = [];  
        }
    }
}
