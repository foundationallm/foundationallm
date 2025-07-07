using FoundationaLLM.Common.Models.Knowledge;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents the result of a text chunk collection query.
    /// </summary>
    public class ContextTextChunkQueryResponse
    {
        /// <summary>
        /// Gets or sets a flag that indicates if the query was processed successfully.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message associated with the current operation or state.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of text chunks that are part of the result.
        /// </summary>
        [JsonPropertyName("text_chunks")]
        public List<ContextTextChunk> TextChunks { get; set; } = [];
    }
}
