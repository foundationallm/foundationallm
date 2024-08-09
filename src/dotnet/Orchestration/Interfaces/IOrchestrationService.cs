﻿using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Orchestration.Core.Interfaces;

/// <summary>
/// Interface for the Orchestration Service
/// </summary>
public interface IOrchestrationService
{
    /// <summary>
    /// Get the aggredated status of all orchestration services.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <returns>The status of the orchestration service.</returns>
    Task<ServiceStatusInfo> GetStatus(string instanceId);

    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="completionRequest">The completion request.</param>
    /// <returns>The completion response.</returns>
    Task<CompletionResponse> GetCompletion(string instanceId, CompletionRequest completionRequest);

    /// <summary>
    /// Begins a completion operation.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
    /// <returns>Returns an <see cref="LongRunningOperation"/> object containing the OperationId and Status.</returns>
    Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest);

    /// <summary>
    /// Gets the status of a completion operation.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="operationId">The OperationId for which to retrieve the status.</param>
    /// <returns>Returns an <see cref="LongRunningOperation"/> object containing the OperationId and Status.</returns>
    Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId);

    /// <summary>
    /// Gets a completion operation from the Orchestration service.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="operationId">The ID of the operation to retrieve.</param>
    /// <returns>Returns a <see cref="CompletionResponse" /> object.</returns>
    Task<CompletionResponse> GetCompletionOperationResult(string instanceId, string operationId);
}
