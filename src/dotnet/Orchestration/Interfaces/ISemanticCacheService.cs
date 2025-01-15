using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Orchestration.Models;

namespace FoundationaLLM.Orchestration.Core.Interfaces
{
    /// <summary>
    /// Defines the interface for the semantic cache service.
    /// </summary>
    public interface ISemanticCacheService
    {
        /// <summary>
        /// Determines whether the semantic cache for the specified agent in the specified FoundationaLLM instance exists.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <returns><see langref="true"/> if the semantic cache for the specified agent exists, <see langref="false"/> otherwise.</returns>
        bool HasCacheForAgent(string instanceId, string agentName);

        /// <summary>
        /// Initializes the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="agentSettings">The <see cref="AgentSemanticCacheSettings"/> providing the agent's semantic cache settings.</param>
        /// <returns></returns>
        Task InitializeCacheForAgent(
            string instanceId,
            string agentName,
            AgentSemanticCacheSettings agentSettings);

        /// <summary>
        /// Resets the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <returns></returns>
        Task ResetCacheForAgent(string instanceId, string agentName);

        /// <summary>
        /// Tries to get a <see cref="CompletionResponse"/> from the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="completionRequest">The <see cref="CompletionRequest"/> for which to get the cache item.</param>
        /// <returns>A <see cref="CompletionResponse"/> if a match exists.</returns>
        Task<CompletionResponse?> GetCompletionResponseFromCache(
            string instanceId,
            string agentName,
            CompletionRequest completionRequest);

        /// <summary>
        /// Sets a <see cref="CompletionResponse"/> in the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="completionResponse">The <see cref="CompletionResponse"/> to be set in the agent's cache.</param>
        /// <returns></returns>
        Task SetCompletionResponseInCache(
            string instanceId,
            string agentName,
            CompletionResponse completionResponse);
    }
}
