using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces
{
    public interface ILegacyAgentPlugin
    {
        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        Task<LLMCompletionResponse> GetCompletion(LegacyCompletionRequest request);
    }
}
