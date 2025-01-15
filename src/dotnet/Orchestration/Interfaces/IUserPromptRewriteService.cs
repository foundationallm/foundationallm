using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Orchestration.Core.Interfaces
{
    /// <summary>
    /// Defines the interface for the user prompt rewrite service.
    /// </summary>
    public interface IUserPromptRewriteService
    {
        /// <summary>
        /// Determines whether user prompt rewrite is configured for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <returns><see langref="true"/> if the user prompt rewrite for the specified agent is configured, <see langref="false"/> otherwise.</returns>
        bool HasUserPromptRewriterForAgent(string instanceId, string agentName);

        /// <summary>
        /// Initializes user prompt rewrite for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="agentSettings">The <see cref="AgentUserPromptRewriteSettings"/> providing the agent's user prompt rewrite settings.</param>
        /// <returns></returns>
        Task InitializeUserPromptRewriterForAgent(
            string instanceId,
            string agentName,
            AgentUserPromptRewriteSettings agentSettings);

        /// <summary>
        /// Rewrites the user prompt to a form that can be used by the AI model.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="completionRequest">The <see cref="CompletionRequest"/> for which to rewrite the user prompt.</param>
        /// <returns>The rewritten user prompt.</returns>
        Task RewriteUserPrompt(
            string instanceId,
            string agentName,
            CompletionRequest completionRequest);
    }
}
