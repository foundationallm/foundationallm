using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for a prompt hub service
/// </summary>
public interface IPromptHubAPIService : IHubAPIService
{
    /// <summary>
    /// Used to get prompts for a target agent and user context.
    /// </summary>
    /// <param name="promptContainer">The prompt container from which prompt values will be retrieved.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="promptName">Name of the prompt for which to retrieve prompt values.</param>
    /// <returns>Returns a <see cref="PromptHubResponse"/> object containing the list of prompts for the specified agent.</returns>
    Task<PromptHubResponse> ResolveRequest(string promptContainer, string sessionId, string promptName = "default");
}
