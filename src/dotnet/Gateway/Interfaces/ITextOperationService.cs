using FoundationaLLM.Gateway.Models;

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
        /// <param name="textOperationRequest">The text operation request to execute.</param>
        /// <returns>The result of the operation.</returns>
        Task<InternalTextOperationResult> ExecuteTextOperation(
            InternalTextOperationRequest textOperationRequest);
    }
}
