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
        /// Gets or sets the embedding model used for the knowledge unit.
        /// </summary>
        [JsonPropertyName("embedding_model")]
        public required string EmbeddingModel { get; set; }

        /// <summary>
        /// Gets or sets the number of dimensions for the embeddings used in the knowledge unit.
        /// </summary>
        [JsonPropertyName("embedding_dimensions")]
        public required int EmbeddingDimensions { get; set; }

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
        /// Gets or sets the list of metadata properties managed by the vector database.
        /// </summary>
        /// <remarks>
        /// Must be a comma-separated list of metadata property names and types to be indexed.
        /// The name and type must be separated by '|'.
        /// For Azure AI Search databases, valid types are 'Edm.String', 'Edm.Int32', 'Edm.Int64','Edm.Single','Edm.Double','Edm.Boolean','Edm.DateTimeOffset'.
        /// </remarks>
        [JsonPropertyName("metadata_properties")]
        public required string MetadataProperties {  get; set; }

        /// <summary>
        /// Gets or sets the API Endpoint Configuration object identifier.
        /// </summary>
        [JsonPropertyName("api_endpoint_configuration_object_id")]
        public required string APIEndpointConfigurationObjectId { get; set; }
    }
}
