using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Response;
using System.ClientModel;
using System.Reflection.Metadata;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Represents a client for interacting with the FoundationaLLM State API service.
    /// </summary>
    public interface IStateServiceClient
    {
        /// <summary>
        /// Creates a new long-running operation in the state service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <param name="cancellationToken">The cancellation token notifiying a request to cancel the operation creation.</param>
        /// <returns></returns>
        Task<ClientResult<LongRunningOperation>> CreateOperation(
           string instanceId,
           string operationId,
           UnifiedUserIdentity userIdentity,
           CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status of an existing long-running operation in the state service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="status">The status of the operation.</param>
        /// <param name="statusMessage">The message describing the status of the operation.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <param name="cancellationToken">The cancellation token notifiying a request to cancel the operation status update.</param>
        /// <returns></returns>
        Task<ClientResult<LongRunningOperation>> UpdateOperation(
            string instanceId,
            string operationId,
            OperationStatus status,
            string statusMessage,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the current result of a long-running operation in the state service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="completionResponse">The completion response that is the current result of the operation.</param>
        /// <param name="cancellationToken">The cancellation token notifiying a request to cancel the operation result update.</param>
        /// <returns></returns>
        Task UpdateOperationResult(
            string instanceId,
            string operationId,
            CompletionResponse completionResponse,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status and the current result of an existing long-running operation in the state service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="status">The status of the operation.</param>
        /// <param name="statusMessage">The message describing the status of the operation.</param>
        /// <param name="resultMessage">The message describing the current result of the operation.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <param name="cancellationToken">The cancellation token notifiying a request to cancel the operation status update.</param>
        /// <returns></returns>
        Task<ClientResult<LongRunningOperation>> UpdateOperationWithTextResult(
            string instanceId,
            string operationId,
            OperationStatus status,
            string statusMessage,
            string resultMessage,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken = default);
    }
}
