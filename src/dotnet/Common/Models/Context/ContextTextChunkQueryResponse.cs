using FoundationaLLM.Common.Models.Knowledge;

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
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the list of text chunks that are part of the result.
        /// </summary>
        public List<ContextTextChunk> TextChunks { get; set; } = [];
    }
}
