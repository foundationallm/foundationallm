using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Graph.Privacy;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Maintains the context for an embedding operation.
    /// </summary>
    public class EmbeddingOperationContext
    {
        private readonly object _syncRoot = new();

        /// <summary>
        /// The original <see cref="TextEmbeddingRequest"/> that triggered the embedding operation.
        /// </summary>
        public required TextEmbeddingRequest Request { get; set; }

        /// <summary>
        /// The <see cref="TextEmbeddingResult"/> holding the result of the embedding operation.
        /// </summary>
        public required TextEmbeddingResult Result { get; set; }

        public void SetError(string errorMessage)
        {
            lock (_syncRoot)
            {
                Result.ErrorMessage = errorMessage;
                Result.Cancelled = true;
                Result.InProgress = false;
            }
        }

        public void SetComplete()
        {
            lock ( _syncRoot)
            {
                Result.InProgress = false;
            }
        }

        public void SetEmbeddings(IList<TextChunk> textChunks)
        {
            lock(_syncRoot)
            {
                foreach (var textChunk in  textChunks)
                {
                    Result.TextChunks[textChunk.Position - 1].Embedding = textChunk.Embedding;
                }
            }
        }
    }
}
