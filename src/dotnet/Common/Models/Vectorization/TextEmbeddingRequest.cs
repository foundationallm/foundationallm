using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Models a request to embed a list of text chunks.
    /// </summary>
    public class TextEmbeddingRequest
    {
        /// <summary>
        /// The list of <see cref="TextChunk"/> objects containing the texts to embed.
        /// </summary>
        [JsonPropertyName("text_chunks")]
        public IList<TextChunk> TextChunks { get; set; } = [];

        /// <summary>
        /// The name of the embedding model to use.
        /// If not specified, a default embedding model should be used.
        /// </summary>
        [JsonPropertyName("embedding_model_name")]
        public string EmbeddingModelName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or set the number of dimensions for the embedding model.
        /// </summary>
        [JsonPropertyName("embedding_model_dimensions")]
        public int EmbeddingModelDimensions { get; set; } = 1536;

        /// <summary>
        /// Indicates whether the request should be prioritized.
        /// Example: Synchronous vectorization and user prompt embedding for completions.
        /// </summary>
        [JsonPropertyName("prioritized")]
        public bool Prioritized { get; set; } = false;
    }
}
