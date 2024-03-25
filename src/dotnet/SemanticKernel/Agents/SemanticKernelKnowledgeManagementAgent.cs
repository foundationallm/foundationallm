using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.SemanticKernel.Core.Agents
{
    /// <summary>
    /// The Knowledge Management agent.
    /// </summary>
    /// <param name="request">The <see cref="KnowledgeManagementCompletionRequest"/> to be processed by the agent.</param>
    public class SemanticKernelKnowledgeManagementAgent(
        LLMCompletionRequest request) : SemanticKernelAgentBase(request)
    {
    }
}
