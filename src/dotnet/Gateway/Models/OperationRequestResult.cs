using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Represents the result of a text operation request.
    /// </summary>
    public class OperationRequestResult
    {
        /// <summary>
        /// Gets or sets the list of text chunks that were successfully processed.
        /// </summary>
        public IList<TextChunk> TextChunks { get; set; } = [];

        /// <summary>
        /// Gets or sets the flag that indicates whether the request procesing has failed or not.
        /// </summary>
        public bool Failed { get; set; } = false;

        /// <summary>
        /// Gets or sets the error message when the request processing has failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of operation IDs that failed during the request processing.
        /// </summary>
        public List<string> FailedOperationIds { get; set; } = [];
    }
}
