using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Orchestration.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service
/// </summary>
public interface IOrchestrationService
{
    /// <summary>
    /// Status value to return when the APIs status endpoint is called.
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);
}
