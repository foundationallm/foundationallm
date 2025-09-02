using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a knowledge entity embedding used in a knowledge graph.
    /// </summary>
    public class KnowledgeEntityEmbedding : KnowledgeEntityBase
    {
        /// <summary>
        /// Gets or sets the embedding of the knowledge entity.
        /// </summary>
        [JsonPropertyName("summary_description_embedding")]
        [JsonPropertyOrder(1)]
        public float[]? SummaryDescriptionEmbedding { get; set; }
    }
}
