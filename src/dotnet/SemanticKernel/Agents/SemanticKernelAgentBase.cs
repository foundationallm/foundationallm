using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Exceptions;
using Microsoft.SemanticKernel;

namespace FoundationaLLM.SemanticKernel.Core.Agents
{
    /// <summary>
    /// Implements the base functionality for a Semantic Kernel agent.
    /// </summary>
    /// <param name="request">The <see cref="LLMCompletionRequest"/> being processed by the agent.</param>
    public class SemanticKernelAgentBase(
        LLMCompletionRequest request)
    {
        private readonly LLMCompletionRequest _request = request;

        public async Task<LLMCompletionResponse> GetCompletion() => throw new NotImplementedException();

        private void ValidateRequest()
        {
            if (_request.Agent == null)
                throw new SemanticKernelException("The Agent property of the completion request cannot be null.");

            if (_request.Agent.OrchestrationSettings == null)
                throw new SemanticKernelException("The OrchestrationSettings property of the agent cannot be null.");

            if (_request.Agent.OrchestrationSettings.EndpointConfiguration == null)
                throw new SemanticKernelException("The EndpointConfiguration property of the agent's OrchestrationSettings property cannot be null.");

            if (_request.Agent.OrchestrationSettings.EndpointConfiguration.TryGetValue(EndpointConfigurationKeys.Provider, out var llmProvider) == false)
                throw new SemanticKernelException("The KernelConfiguration property of the agent's OrchestrationSettings property cannot be null.");

            if (!LanguageModelProviders.All.Contains(llmProvider))
                throw new SemanticKernelException($"The specified LLM provider '{llmProvider}' is not supported.");
        }
    }
}
