using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to query a knowledge source.
    /// </summary>
    public class ContextKnowledgeSourceQueryRequest
    {
        /// <summary>
        /// Gets or sets the user prompt used to query the knowledge source.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public required string UserPrompt { get; set; }

        /// <summary>
        /// Gets or sets the vector store query parameters.
        /// </summary>
        [JsonPropertyName("vector_store_query")]
        public ContextVectorStoreQuery? VectorStoreQuery { get; set; }

        /// <summary>
        /// Gets or sets the knowledge graph query parameters.
        /// </summary>
        [JsonPropertyName("knowledge_graph_query")]
        public ContextKnowledgeGraphQuery? KnowledgeGraphQuery { get; set; }

        /// <summary>
        /// Gets or sets the vector store identifier used to query the knowledge source.
        /// </summary>
        /// <remarks>
        /// This value is used only when the knowledge source does not have a static vector store identier set.
        /// </remarks>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }

        /// <summary>
        /// Gets or sets the metadata filter used to provide additional filtering in the vector store queries.
        /// </summary>
        [JsonPropertyName("vector_store_metadata_filter")]
        public Dictionary<string, object>? VectorStoreMetadataFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response should be formatted.
        /// </summary>
        /// <remarks>When <see langword="true"/>, the response will be plain text. Otherwise, the response will be structured.</remarks>
        [JsonPropertyName("format_response")]
        public bool? FormatResponse { get; set; }
    }
}
