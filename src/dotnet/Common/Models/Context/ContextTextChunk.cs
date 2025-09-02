using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a text chunk used to build context.
    /// </summary>
    public class ContextTextChunk
    {
        /// <summary>
        /// Gets or sets the relevance score of the text chunk.
        /// </summary>
        [JsonPropertyName("score")]
        public double? Score { get; set; }

        /// <summary>
        /// Gets or sets the text content of the text chunk.
        /// </summary>
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the text chunk.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = [];
    }
}
