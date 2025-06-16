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
        /// Indicates whether the requests associated with the context should be prioritized.
        /// Example: Synchronous vectorization and user prompt embedding for completions.
        /// </summary>        
        public bool Prioritized { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of dimensions to be used for embedding.
        /// </summary>
        public int EmbeddingModelDimensions { get; set; } = 1536;

        /// <summary>
        /// Gets or sets the list of intermediate error messages encountered during the embedding operation.
        /// </summary>
        public List<string> IntermediateErrors { get; set; } = [];

        /// <summary>
        /// Sets a specified error message on the context of the embedding operation.
        /// </summary>
        /// <param name="errorMessage">The error message to be set.</param>
        public void SetIntermediateError(string errorMessage)
        {
            lock (_syncRoot)
            {
                IntermediateErrors.Add(errorMessage);

                if (IntermediateErrors.Count >= 3)
                {
                    Result.ErrorMessage = string.Join(string.Empty,
                        [
                            $"The embedding operation {Result.OperationId} encountered {IntermediateErrors.Count} errors and failed.",
                            $"The most recent error message was: {errorMessage}"
                        ]);
                    Result.Failed = true;
                    Result.InProgress = false;
                }
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
