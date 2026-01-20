using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to check the validity of a vector store identifier
    /// in the context of a vector database.
    /// </summary>
    public class CheckVectorStoreIdRequest
    {
        /// <summary>
        /// Gets or sets the vector database object identifier.
        /// </summary>
        [JsonPropertyName("vector_database_object_id")]
        public required string VectorDataBaseObjectId { get; set; }

        /// <summary>
        /// Gets or sets the vector store identifier.
        /// </summary>
        [JsonPropertyName("vector_store_id")]
        public required string VectorStoreId { get; set; }
    }
}
