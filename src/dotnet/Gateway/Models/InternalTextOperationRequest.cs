using FoundationaLLM.Common.Models.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Represents internal text operation requests processed by the FoundationaLLM Gateway.
    /// </summary>
    public class InternalTextOperationRequest
    {
        /// <summary>
        /// The unique identifier of the internal request.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the account used for the text operation.
        /// </summary>
        [JsonPropertyName("account_name")]
        public required string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the name of the deployment used for the text operation.
        /// </summary>
        [JsonPropertyName("deployment_name")]
        public required string DeploymentName { get; set; }

        /// <summary>
        /// The name of the model used for the text operation.
        /// </summary>
        [JsonPropertyName("model_name")]
        public required string ModelName { get; set; }

        /// <summary>
        /// The version of the model used for the text operation.
        /// </summary>
        [JsonPropertyName("model_version")]
        public required string ModelVersion { get; set; }

        /// <summary>
        /// Gets or sets the number of dimensions of the embedding.
        /// </summary>
        /// <remarks>
        /// The value is only relevant for embedding operations (expected to be -1 for completion operations).
        /// </remarks>
        [JsonPropertyName("embedding_dimensions")]
        public int EmbeddingDimensions { get; set; }

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
        /// The details of the text operations from the text chunks.
        /// For each text operation id, holds the list of the positions of the text chunks from the current request.
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
