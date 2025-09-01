using FoundationaLLM.Common.Models.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a relationship between two knowledge entities in a knowledge graph.
    /// </summary>
    public class KnowledgeRelationship : KnowledgeRelationshipBase
    {
        /// <summary>
        /// Gets or sets the list of short descriptions associated with the relationship.
        /// </summary>
        [JsonPropertyName("short_descriptions")]
        [JsonPropertyOrder(1)]
        public List<string> ShortDescriptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of detailed descriptions associated with the relationship.
        /// </summary>
        [JsonPropertyName("descriptions")]
        [JsonPropertyOrder(2)]
        public List<string> Descriptions { get; set; } = [];
        /// <summary>
        /// Gets or sets the hash of the descriptions, used for change detection.
        /// </summary>
        [JsonPropertyName("descriptions_hash")]
        [JsonPropertyOrder(3)]
        public string DescriptionsHash { get; set; }

        /// <summary>
        /// Gets or sets the list of strengths associated with the relationship.
        /// </summary>
        [JsonPropertyName("strengths")]
        [JsonPropertyOrder(4)]
        public List<int> Strengths { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of chunk identifiers associated with the relationship.
        /// </summary>
        [JsonPropertyName("chunk_ids")]
        [JsonPropertyOrder(5)]
        public List<string> ChunkIds { get; set; } = [];

        /// <summary>
        /// Gets or sets the summary description of the relationship, which provides a concise overview.
        /// </summary>
        [JsonPropertyName("summary_description")]
        [JsonPropertyOrder(6)]
        public string? SummaryDescription { get; set; }

        /// <summary>
        /// Gets a consolidated string of all descriptions, separated by new lines.
        /// </summary>
        [JsonIgnore]
        public string ConsolidatedDescriptions => string.Join("\n",
            Descriptions.Distinct());

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeRelationship"/> class.
        /// </summary>
        public KnowledgeRelationship()
        {
            DescriptionsHash = null!;
            ShortDescriptions = [];
            Descriptions = [];
        }
    }
}
