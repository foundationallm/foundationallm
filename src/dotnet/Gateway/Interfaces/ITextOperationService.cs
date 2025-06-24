using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Extensions.ObjectPool;

namespace FoundationaLLM.Gateway.Interfaces
{
    /// <summary>
    /// Defines the interface for text operation services, such as text embedding or completion services.
    /// </summary>
    public interface ITextOperationService
    {
        /// <summary>
        /// Executes a text operation (e.g., embedding or completion) on the provided text chunks.
        /// </summary>
        /// <param name="textChunks">The list of text chunks used as input for the operation.</param>
        /// <param name="deploymentName">The model deployment name to use.</param>
        /// <param name="prioritized">Indicates whether this operation execution must pe prioritized.</param>
        /// <param name="additionalParameters">Additional parameters that are specific to the individual operation.</param>
        /// <returns>The result of the operation.</returns>
        Task<TextOperationResult> ExecuteTextOperation(
            IList<TextChunk> textChunks,
            string deploymentName,
            bool prioritized,
            params object[] additionalParameters);
    }
}
