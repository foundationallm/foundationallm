using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the AgentHub Service.
/// </summary>
public interface IAgentHubAPIService
{
    /// <summary>
    /// Gets the status of the Agent Hub Service.
    /// </summary>
    /// <returns></returns>
    Task<string> Status();

    /// <summary>
    /// Gets a set of agents from the Agent Hub based on the prompt and user context.
    /// </summary>
    /// <param name="userPrompt">The user prompt to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns></returns>
    Task<AgentHubResponse> ResolveRequest(string userPrompt, string sessionId);
}
