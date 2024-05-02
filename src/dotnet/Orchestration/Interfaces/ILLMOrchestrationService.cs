using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Orchestration.Core.Interfaces
{
    /// <summary>
    /// LLM Orchestration Service interface
    /// </summary>
    public interface ILLMOrchestrationService
    {
        /// <summary>
        /// Flag indicating if the orchestration service has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Method for retrieving a completion from the orchestration service.
        /// </summary>
        /// <param name="request">Hub populated request object containing agent, prompt, language model, and data source information</param>
        /// <returns></returns>
        Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request);
    }
}
