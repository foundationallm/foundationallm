using FoundationaLLM.Common.Models.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides metrics related to text embedding requests submitted by the FoundationaLLM Gateway.
    /// </summary>
    public class GatewayTextOperationRequest
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
        /// Gets or sets the number of dimensions of the embedding.
        /// </summary>
        [JsonPropertyName("embedding_dimensions")]
        public int EmbeddingDimensions { get; set; }

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
        /// Gets the total number of tokens used in the request.
        /// </summary>
        [JsonPropertyName("tokens_count")]
        public int TokensCount =>
            TextChunks.Sum(tc => tc.TokensCount);

        /// <summary>
        /// Gets the total number of text chunks in the request.
        /// </summary>
        [JsonPropertyName("text_chunks_count")]
        public int TextChunksCount =>
            TextChunks.Count;

        /// <summary>
        /// The details of the embedding operations from the text chunks.
        /// For each embedding operation id, holds the list of the positions of the text chunks from the current request.
        /// </summary>
        [JsonPropertyName("operations_details")]
        public Dictionary<string, List<int>> OperationsDetails =>
            TextChunks
                .GroupBy(tc => tc.OperationId!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(tc => tc.Position).ToList());

        /// <summary>
        /// Gets or sets the list of text chunks from the current request.
        /// </summary>
        [JsonIgnore]
        public List<TextChunk> TextChunks { get; set; } = [];
    }
}
