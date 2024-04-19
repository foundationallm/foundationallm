using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// The result of a text embedding request.
    /// </summary>
    public class TextEmbeddingResult
    {
        /// <summary>
        /// Indicates whether the text embedding operation is still in progress.
        /// When true, the <see cref="OperationId"/> property contains an operation identifier.
        /// </summary>
        [JsonPropertyName("in_progress")]
        public bool InProgress { get; set; }

        /// <summary>
        /// Indicates whether the text embedding operation was cancelled due to an error.
        /// When true, the <see cref="ErrorMessage"/> property contains a message describing the error.
        /// </summary>
        [JsonPropertyName("cancelled")]
        public bool Cancelled { get; set; }

        /// <summary>
        /// The message describing the error that lead to the cancellation of the operation.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Optional operation identifier that can be used to retrieve the final result.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string? OperationId { get; set; }

        /// <summary>
        /// The list of <see cref="TextChunk"/> objects containing the embeddings.
        /// </summary>
        [JsonPropertyName("text_chunks")]
        public IList<TextChunk> TextChunks { get; set; } = [];

        /// <summary>
        /// The number of tokens used during the embedding operation.
        /// </summary>
        [JsonPropertyName("token_count")]
        public int TokenCount { get; set; }
    }
}
