using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a knowledge relationship embedding used in a knowledge graph.
    /// </summary>
    public class KnowledgeRelationshipEmbedding : KnowledgeRelationshipBase
    {
        /// <summary>
        /// Gets or sets the embedding of the summary description.
        /// </summary>
        [JsonPropertyName("summary_description_embedding")]
        [JsonPropertyOrder(1)]
        public float[]? SummaryDescriptionEmbedding { get; set; }
    }
}
