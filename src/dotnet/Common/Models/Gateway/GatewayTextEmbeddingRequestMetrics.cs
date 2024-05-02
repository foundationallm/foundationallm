using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Gateway
{
    /// <summary>
    /// Provides metrics related to text embedding requests submitted by the FoundationaLLM Gateway.
    /// </summary>
    public class GatewayTextEmbeddingRequestMetrics
    {
        /// <summary>
        /// The unique identifier of the request.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// The name of the account used for text embedding.
        /// </summary>
        [JsonPropertyName("account_name")]
        public required string AccountName { get; set; }

        /// <summary>
        /// The name of the embedding model used for text embedding.
        /// </summary>
        [JsonPropertyName("model_name")]
        public required string ModelName { get; set; }

        /// <summary>
        /// The version of the embedding model used for text embedding.
        /// </summary>
        [JsonPropertyName("model_version")]
        public required string ModelVersion { get; set; }

        /// <summary>
        /// The start timestamp of the current token rate window.
        /// </summary>
        [JsonPropertyName("token_rate_window_start")]
        public DateTime TokenRateWindowStart { get; set; }

        /// <summary>
        /// The start timestamp of the current request rate window.
        /// </summary>
        [JsonPropertyName("request_rate_window_start")]
        public DateTime RequestRateWindowStart { get; set; }

        /// <summary>
        /// The cummulated number of tokens for the current token rate window.
        /// Includes all tokens used so far in the current token rate window.
        /// </summary>
        [JsonPropertyName("token_rate_window_token_count")]
        public int TokenRateWindowTokenCount { get; set; }

        /// <summary>
        /// The cummulated number of requests for the current request rate window.
        /// Includes all calls performed so far in the current call rate window.
        /// </summary>
        [JsonPropertyName("request_rate_window_request_count")]
        public int RequestRateWindowRequestCount { get; set; }

        /// <summary>
        /// The toal number of tokens used in the current request.
        /// </summary>
        [JsonPropertyName("current_request_token_count")]
        public int CurrentRequestTokenCount { get; set; }

        /// <summary>
        /// The number of text chunks in the current request.
        /// </summary>
        [JsonPropertyName("current_text_chunk_count")]
        public int CurrentTextChunkCount { get; set; }

        /// <summary>
        /// The details of the embedding operations from the text chunks.
        /// For each embedding operation id, holds the list of the positions of the text chunks from the current request.
        /// </summary>
        [JsonPropertyName("operations_details")]
        public Dictionary<string, List<int>> OperationsDetails { get; set; } = [];
    }
}
