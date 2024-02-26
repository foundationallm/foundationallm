using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    public class EmbeddingModel
    {
        /// <summary>
        /// The type of the embedding model.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// The provider of the emberdding model.
        /// </summary>
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        /// <summary>
        /// Name of the embedding model deployment.
        /// </summary>
        [JsonPropertyName("deployment")]
        public string? Deployment { get; set; }

        /// <summary>
        /// Name of the embedding model.
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// The chunk size to use when creating vectors from the embedding model.
        /// </summary>
        [JsonPropertyName("chunk_size")]
        public int ChunkSize { get; set; } = 1000;

        /// <summary>
        /// The endpoint to use to access the language model.
        /// </summary>
        [JsonPropertyName("api_endpoint")]
        public string? ApiEndpoint { get; set; }

        /// <summary>
        /// The API key of the language model endpoint to use to access the language model.
        /// </summary>
        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }

        /// <summary>
        /// API version of the language model endpoint.
        /// </summary>
        [JsonPropertyName("api_version")]
        public string? ApiVersion { get; set; }
    }
}
