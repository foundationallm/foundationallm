using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a vector store filter for knowledge units.
    /// </summary>
    public class ContextKnowledgeUnitVectorStoreFilter
    {
        /// <summary>
        /// Gets or sets the identifier of the knowledge unit to query.
        /// </summary>
        [JsonPropertyName("knowledge_unit_id")]
        public required string KnowledgeUnitId { get; set; }

        /// <summary>
        /// Gets or sets the vector store identifier used to query the knowledge unit.
        /// </summary>
        /// <remarks>
        /// This value is used only when the knowledge unit does not have a static vector store identier set.
        /// </remarks>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }

        /// <summary>
        /// Gets or sets the metadata filter used to provide additional filtering in the vector store queries.
        /// </summary>
        [JsonPropertyName("vector_store_metadata_filter")]
        public Dictionary<string, object>? VectorStoreMetadataFilter { get; set; }
    }
}
