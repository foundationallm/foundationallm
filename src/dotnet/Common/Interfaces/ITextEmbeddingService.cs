using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides text embedding capabilities.
    /// </summary>
    public interface ITextEmbeddingService
    {
        /// <summary>
        /// Initializes the text embedding operation.
        /// Depending on the implementation, this can be an atomic operation or a long-running one.
        /// </summary>
        /// <param name="textChunks">The list of text chunks which need to be embedded.</param>
        /// <param name="deploymentName"> The name of the model deployment to use for embedding.</param>
        /// <param name="embeddingDimensions"> The number of dimensions for the embedding model.</param>
        /// <param name="prioritized">Indicates whether the request should be prioritized.</param>
        /// <returns>A <see cref="TextOperationResult"/> object containing the result of the text embedding operation.</returns>
        Task<TextOperationResult> GetEmbeddingsAsync(IList<TextChunk> textChunks, string deploymentName, int embeddingDimensions, bool prioritized);

        /// <summary>
        /// Retrieves the result of a long-running text embedding operation.
        /// </summary>
        /// <param name="operationId">The unique identifier of the long-running operation.</param>
        /// <returns>A <see cref="TextOperationResult"/> object containing the result of the text embedding operation.</returns>
        Task<TextOperationResult> GetEmbeddingsAsync(string operationId);
    }
}
