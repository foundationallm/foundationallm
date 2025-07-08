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

        /// <summary>
        /// Gets or sets the vector store identifier used to query the knowledge source.
        /// </summary>
        /// <remarks>
        /// This value is used only when the knowledge source does not have static vector store identier set.
        /// </remarks>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }

        /// <summary>
        /// Gets or sets the metadata filter used to filter the text chunks.
        /// </summary>
        [JsonPropertyName("metadata_filter")]
        public Dictionary<string, string>? MetadataFilter { get; set; }
    }
}
