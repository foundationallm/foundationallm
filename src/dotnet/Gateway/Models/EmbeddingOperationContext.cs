using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Maintains the context for an embedding operation.
    /// </summary>
    public class EmbeddingOperationContext
    {
        private readonly object _syncRoot = new();

        /// <summary>
        /// The list of <see cref="TextChunk"/> objects which provide the input to the embedding operation.
        /// </summary>
        public required IList<TextChunk> InputTextChunks { get; set; } = [];

        /// <summary>
        /// The <see cref="TextEmbeddingResult"/> holding the result of the embedding operation.
        /// </summary>
        public required TextEmbeddingResult Result { get; set; }

        /// <summary>
        /// Sets a specified error message on the context of the embedding operation.
        /// </summary>
        /// <param name="errorMessage">The error message to be set.</param>
        public void SetError(string errorMessage)
        {
            lock (_syncRoot)
            {
                Result.ErrorMessage = errorMessage;
                Result.Failed = true;
                Result.InProgress = false;
            }
        }

        /// <summary>
        /// Marks the embedding operation as complete.
        /// </summary>
        public void SetComplete()
        {
            lock ( _syncRoot)
            {
                Result.InProgress = false;
            }
        }

        /// <summary>
        /// Sets the embeddings for a specified set of position.
        /// If all positions have non-null embeddings, marks the operation as complete.
        /// </summary>
        /// <param name="textChunks">A list of <see cref="TextChunk"/> objects containing positions and their associated embeddings.</param>
        public void SetEmbeddings(IList<TextChunk> textChunks)
        {
            lock(_syncRoot)
            {
                foreach (var textChunk in  textChunks)
                {
                    Result.TextChunks[textChunk.Position - 1].Embedding = textChunk.Embedding;
                }

                if (Result.TextChunks.All(tc => tc.Embedding != null))
                    Result.InProgress = false;
            }
        }
    }
}
