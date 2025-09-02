using FoundationaLLM.Common.Models.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a knowledge entity used in a knowledge graph.
    /// </summary>
    public class KnowledgeEntity : KnowledgeEntityBase
    {
        /// <summary>
        /// Gets or sets the list of descriptions associated with the entity.
        /// </summary>
        [JsonPropertyName("descriptions")]
        [JsonPropertyOrder(1)]
        public List<string> Descriptions { get; set; }

        /// <summary>
        /// Gets or sets the hash of the descriptions, used for change detection.
        /// </summary>
        [JsonPropertyName("descriptions_hash")]
        [JsonPropertyOrder(2)]
        public string DescriptionsHash { get; set; }

        /// <summary>
        /// Gets or sets the list of text chunk identifiers associated with the entity.
        /// </summary>
        [JsonPropertyName("chunk_ids")]
        [JsonPropertyOrder(3)]
        public List<string> ChunkIds { get; set; }

        /// <summary>
        /// Gets or sets the summary description of the entity, which provides a concise overview.
        /// </summary>
        [JsonPropertyName("summary_description")]
        [JsonPropertyOrder(4)]
        public string? SummaryDescription { get; set; }

        /// <summary>
        /// Gets a consolidated string of all descriptions, separated by new lines.
        /// </summary>
        [JsonIgnore]
        public string ConsolidatedDescriptions => string.Join("\n",
            Descriptions.Distinct());

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeEntity"/> class.
        /// </summary>
        public KnowledgeEntity()
        {
            DescriptionsHash = null!;
            Descriptions = [];
            ChunkIds = [];
        }
    }
}
