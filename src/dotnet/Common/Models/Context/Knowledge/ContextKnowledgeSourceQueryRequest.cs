using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to query a collection of text chunks.
    /// </summary>
    public class ContextKnowledgeSourceQueryRequest
    {
        /// <summary>
        /// Gets or sets the user prompt used to query the knowledge graph.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of text chunks included in the query result.
        /// </summary>
        [JsonPropertyName("text_chunks_max_count")]
        public int TextChunksMaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum similarity measure threshold used to identify relevant text chunks.
        /// </summary>
        [JsonPropertyName("text_chunks_similarity_threshold")]
        public float TextChunksSimilarityThreshold { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates whether semantic ranking should be used or not.
        /// </summary>
        [JsonPropertyName("use_semantic_ranking")]
        public bool UseSemanticRanking { get; set; }

        /// <summary>
        /// Gets or sets the knowledge graph query parameters.
        /// </summary>
        [JsonPropertyName("knowledge_graph_query")]
        public ContextKnowledgeGraphQuery? KnowledgeGraphQuery { get; set; }
    }
}
