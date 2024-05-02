using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Represents a chunk of text produced by a text splitter.
    /// </summary>
    public class TextChunk
    {
        /// <summary>
        /// The identifier of the operation the text chunk belongs to. Can be null if the chunk is not associated with an operation.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string? OperationId { get; set; }

        /// <summary>
        /// The position of the text chunk in the content it belongs to.
        /// </summary>
        [JsonPropertyName("position")]
        public int Position { get; set; }

        /// <summary>
        /// The text content of the chunk.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// The embedding of the text content.
        /// </summary>
        [JsonPropertyName("embedding")]
        [JsonConverter(typeof(Embedding.JsonConverter))]
        public Embedding? Embedding { get; set; }

        /// <summary>
        /// The size of the chunk in tokens.
        /// </summary>
        [JsonPropertyName("tokens_count")]
        public int TokensCount { get; set; }
    }
}
