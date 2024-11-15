using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Interface for calling a downstream API.
    /// </summary>
    public interface IDownstreamAPIService
    {
        /// <summary>
        /// The name of the downstream API.
        /// </summary>
        public string APIName { get; }

        /// <summary>
        /// Gets a completion from the downstream API.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
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
        /// <returns>Returns a completion response</returns>
        Task<CompletionResponse> GetCompletionOperationResult(string instanceId, string operationId);
    }
}
