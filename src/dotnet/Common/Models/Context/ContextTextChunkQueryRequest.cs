using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a request to query a collection of text chunks.
    /// </summary>
    public class ContextTextChunkQueryRequest
    {
        /// <summary>
        /// Gets or sets the user prompt used to query the knowledge graph.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of text chunks included in the query result.
        /// </summary>
        [JsonPropertyName("text_chunks_max_count")]
        public int TextChunksMaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum similarity measure threshold used to identify relevant text chunks.
        /// </summary>
        [JsonPropertyName("text_chunks_similarity_threshold")]
        public float TextChunksSimilarityThreshold { get; set; }
    }
}
