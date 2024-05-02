using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Orchestration.Core.Interfaces;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Base class for an orchestration involving a FoundationaLLM agent.
    /// </summary>
    /// <remarks>
    /// Constructor for the AgentBase class.
    /// </remarks>
    /// <param name="orchestrationService"></param>
    public class OrchestrationBase(ILLMOrchestrationService orchestrationService)
    {
        /// <summary>
        /// The orchestration service for the agent.
        /// </summary>
        protected readonly ILLMOrchestrationService _orchestrationService = orchestrationService;

        /// <summary>
        /// The call to execute a completion after the agent is configured.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        public virtual async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            await Task.CompletedTask;
            return null!;
        }
    }
}
