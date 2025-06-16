using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Represents the result of procedding an embedding request.
    /// </summary>
    public class EmbeddingRequestResult
    {
        /// <summary>
        /// Gets or sets the list of text chunks that were successfully processed.
        /// </summary>
        public IList<TextChunk> TextChunks { get; set; } = [];

        /// <summary>
        /// Gets or sets the flag that indicates whether the embedding request procesing has failed or not.
        /// </summary>
        public bool Failed { get; set; } = false;

        /// <summary>
        /// Gets or sets the error message if the embedding request processing has failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of operation IDs that failed during the embedding request processing.
        /// </summary>
        public List<string> FailedOperationIds { get; set; } = [];
    }
}
