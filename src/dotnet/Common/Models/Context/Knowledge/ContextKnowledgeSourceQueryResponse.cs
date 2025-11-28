using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents the result of a knowledge source query.
    /// </summary>
    public class ContextKnowledgeSourceQueryResponse
    {
        /// <summary>
        /// Gets or sets the source of the response..
        /// </summary>
        [JsonPropertyName("source")]
        public required string Source { get; set; }

        /// <summary>
        /// Gets or sets the response from the vector store query.
        /// </summary>
        [JsonPropertyName("vector_store_response")]
        public ContextVectorStoreResponse? VectorStoreResponse { get; set; }

        /// <summary>
        /// Gets or sets the response from the knowledge graph query.
        /// </summary>
        [JsonPropertyName("knowledge_graph_response")]
        public ContextKnowledgeGraphResponse? KnowledgeGraphResponse { get; set; }

        /// <summary>
        /// Gets or sets the formatted text response.
        /// </summary>
        [JsonPropertyName("text_response")]
        public string? TextResponse { get; set; }

        /// <summary>
        /// Gets or sets the content references associated with the query response.
        /// </summary>
        [JsonPropertyName("content_references")]
        public List<Dictionary<string, object>>? ContentReferences { get; set; }
    }
}
