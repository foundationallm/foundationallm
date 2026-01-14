using FoundationaLLM.Common.Constants.Context;
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
        /// Gets or sets the knowledge task associated with the query.
        /// </summary>
        /// <remarks>The value must be one of the <see cref="ContextKnowledgeTasks"/> values.</remarks>
        [JsonPropertyName("knowledge_task")]
        public required string KnowledgeTask { get; set; }

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
        /// Gets or sets the list of knowledge unit vector store filters.
        /// </summary>
        [JsonPropertyName("knowledge_unit_vector_store_filters")]
        public List<ContextKnowledgeUnitVectorStoreFilter> KnowledgeUnitVectorStoreFilters { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the response should be formatted.
        /// </summary>
        /// <remarks>When <see langword="true"/>, the response will be plain text. Otherwise, the response will be structured.</remarks>
        [JsonPropertyName("format_response")]
        public bool? FormatResponse { get; set; }

        /// <summary>
        /// Gets or sets the agent object identifier if the query is being made on behalf of an agent.
        /// </summary>
        [JsonPropertyName("agent_object_id")]
        public string? AgentObjectId { get; set; }
    }
}
