using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vector
{
    /// <summary>
    /// Represents a vector database resource.
    /// </summary>
    public class VectorDatabase : ResourceBase
    {
        /// <summary>
        /// Gets or sets the type of the vector database.
        /// </summary>
        [JsonPropertyName("database_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required VectorDatabaseType DatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the name of the vector database.
        /// </summary>
        [JsonPropertyName("database_name")]
        public required string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that stores the embeddings.
        /// </summary>
        [JsonPropertyName("embedding_property_name")]
        public required string EmbeddingPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that stores the content.
        /// </summary>
        [JsonPropertyName("content_property_name")]
        public required string ContentPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that stores the vector store identifier.
        /// </summary>
        [JsonPropertyName("vector_store_id_property_name")]
        public required string VectorStoreIdPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the metadata property that stores additional information about each item.
        /// </summary>
        [JsonPropertyName("metadata_property_name")]
        public required string MetadataPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the API Endpoint Configuration object identifier.
        /// </summary>
        [JsonPropertyName("api_endpoint_configuration_object_id")]
        public required string APIEndpointConfigurationObjectId { get; set; }
    }
}
