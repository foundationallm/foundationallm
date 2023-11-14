using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for a prompt hub service
/// </summary>
public interface IPromptHubAPIService
{
    /// <summary>
    /// Gets the status of the Prompt Hub Service
    /// </summary>
    /// <returns></returns>
    Task<string> Status();

    /// <summary>
    /// Used to get prompts for a target agent and user context.
    /// </summary>
    /// <param name="agentName">Name of the agent for which to retrieve prompt values.</param>
    /// <param name="promptName">Name of the prompt for which to retrieve prompt values.</param>
    /// <returns>Returns a <see cref="PromptHubResponse"/> object containing the list of prompts for the specified agent.</returns>
    Task<PromptHubResponse> ResolveRequest(string agentName, string promptName = "default");
}
