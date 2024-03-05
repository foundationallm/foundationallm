using FoundationaLLM.Common.Models.Hubs;

namespace FoundationaLLM.Common.Interfaces;

/// <summary>
/// Interface for the AgentHub Service.
/// </summary>
public interface IAgentHubAPIService : IHubAPIService, ICacheControlAPIService
{
    /// <summary>
    /// Gets a set of agents from the Agent Hub based on the prompt and user context.
    /// </summary>
    /// <param name="userPrompt">The user prompt to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns></returns>
    Task<AgentHubResponse> ResolveRequest(string userPrompt, string sessionId);

    /// <summary>
    /// Gets the list with all the agent names and descriptions.
    /// </summary>
    /// <returns>A list of <see cref="AgentMetadata"/> objects containing the names and descriptions of the agents.</returns>
    Task<List<AgentMetadata>> ListAgents();
}
