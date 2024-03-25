using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces
{
    /// <summary>
    /// Defines methods for processing requests targeting the Semantic Kernel agents.
    /// </summary>
    public interface ISemanticKernelService
    {
        /// <summary>
        /// Gets a completion using Semantic Kernel agents.
        /// </summary>
        /// <param name="request">The <see cref="LLMCompletionRequest"/> containing the details of the completion request.</param>
        /// <returns>A <see cref="LLMCompletionResponse"/> with the results of the completion.</returns>
        Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request);
    }
}
