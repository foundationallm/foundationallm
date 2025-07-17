using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents the response from a context vector store query.
    /// </summary>
    public class ContextVectorStoreResponse
    {
        /// <summary>
        /// Gets or sets the list of text chunks that are part of the result.
        /// </summary>
        [JsonPropertyName("text_chunks")]
        public List<ContextTextChunk> TextChunks { get; set; } = [];
    }
}
