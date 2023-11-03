using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Contains methods for interacting with the Gatekeeper API.
/// </summary>
public interface IGatekeeperAPIService
{
    /// <summary>
    /// Requests a completion from the downstream APIs via the Gatekeeper API.
    /// </summary>
    /// <param name="completionRequest"></param>
    /// <returns></returns>
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);
    /// <summary>
    /// Requests a summary from the downstream APIs via the Gatekeeper API.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<string> GetSummary(string content);
    /// <summary>
    /// Adds the entity to the orchestrator's memory used by the RAG service.
    /// </summary>
    /// <param name="item">The entity to add to memory.</param>
    /// <param name="itemName">The type name of the entity.</param>
    /// <param name="vectorizer">The embedded entity and its vector.</param>
    /// <returns></returns>
    Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);
    /// <summary>
    /// Removes the entity from the orchestrator's memory used by the RAG service.
    /// </summary>
    /// <param name="item">The entity to remove from memory.</param>
    /// <returns></returns>
    Task RemoveMemory(object item);
}