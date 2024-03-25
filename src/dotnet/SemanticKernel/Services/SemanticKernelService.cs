using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Agents;
using FoundationaLLM.SemanticKernel.Core.Interfaces;

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Processes requests targeting the Semantic Kernel agents.
    /// </summary>
    public class SemanticKernelService : ISemanticKernelService
    {
        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request) => request.Agent switch
        {
            KnowledgeManagementAgent => await (new SemanticKernelKnowledgeManagementAgent(request)).GetCompletion(),
            _ => throw new Exception($"The agent type {request.Agent.GetType()} is not supported.")
        };
}
}
