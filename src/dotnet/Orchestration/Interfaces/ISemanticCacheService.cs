using FoundationaLLM.Common.Models.Authentication;
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
        /// <param name="currentUserIdentity">The <see cref="UnifiedUserIdentity"/> of the current user.</param>
        /// <returns></returns>
        Task InitializeCacheForAgent(
            string instanceId,
            string agentName,
            AgentSemanticCacheSettings agentSettings,
            UnifiedUserIdentity currentUserIdentity);

        /// <summary>
        /// Resets the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <returns></returns>
        Task ResetCacheForAgent(string instanceId, string agentName);

        /// <summary>
        /// Tries to get a cache item from the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="userPrompt">The user prompt for which a cache item is searched.</param>
        /// <param name="messageHistory">The message history associated with the user prompt.</param>
        /// <returns>A <see cref="SemanticCacheItem"/> if a match exists.</returns>
        Task<SemanticCacheItem?> GetCacheItem(
            string instanceId,
            string agentName,
            string userPrompt,
            List<string> messageHistory);

        /// <summary>
        /// Sets a cache item in the semantic cache for the specified agent in the specified FoundationaLLM instance.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="cacheItem">The <see cref="SemanticCacheItem"/> to be set in the agent's cache.</param>
        /// <returns></returns>
        Task SetCacheItem(
            string instanceId,
            string agentName,
            SemanticCacheItem cacheItem);
    }
}
