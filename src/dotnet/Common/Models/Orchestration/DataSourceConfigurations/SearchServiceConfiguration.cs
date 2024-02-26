using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// Search Service configuration settings.
    /// </summary>
    public class SearchServiceConfiguration
    {
        /// <summary>
        /// The type of configuration. This value should not be changed.
        /// </summary>
        [JsonPropertyName("configuration_type")]
        public string ConfigurationType => "search_service";

        /// <summary>
        /// The endpoint of the Search Service
        /// </summary>
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }

        /// <summary>
        /// The name of key vault secret containing the access key
        /// </summary>
        [JsonPropertyName("key_secret")]
        public string? KeySecret { get; set; }

        /// <summary>
        /// The name of the index in Azure AI Search
        /// </summary>
        [JsonPropertyName("index_name")]
        public string? IndexName { get; set; }

        /// <summary>
        /// (Optional) The number of rows to return from the index search
        /// </summary>
        [JsonPropertyName("top_n")]
        public int? TopN { get; set; }

        /// <summary>
        /// (Optional) The name of the field to use for embedding in the Search Service.
        /// </summary>
        [JsonPropertyName("embedding_field_name")]
        public string? EmbeddingFieldName { get; set; }

        /// <summary>
        /// (Optional) The name of the field to use for raw text in the Search Service.
        /// </summary>
        [JsonPropertyName("text_field_name")]
        public string? TextFieldName { get; set; }
    }
}
