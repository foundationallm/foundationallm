using System.Text.Json;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.State.Interfaces
{
    /// <summary>
    /// Provides methods for interacting with the Cosmos DB service.
    /// </summary>
    public interface IStateCosmosDBService
    {
        /// <summary>
        /// Retrieves all long-running operations.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<LongRunningOperation>> GetLongRunningOperations(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a long-running operation by its identifier.
        /// </summary>
        /// <param name="id">The long-running operation identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<LongRunningOperation> GetLongRunningOperation(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all log entries for a long-running operation.
        /// </summary>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<LongRunningOperationLogEntry>> GetLongRunningOperationLogEntries(string operationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the result of a long-running operation.
        /// </summary>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<JsonDocument?> GetLongRunningOperationResult(string operationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or updates a long-running operation and creates a log entry.
        /// </summary>
        /// <param name="operation">The long-running operation to insert or update.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<LongRunningOperation> UpsertLongRunningOperation(LongRunningOperation operation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or updates the result of a long-running operation.
        /// </summary>
        /// <param name="operationResult">The operation result to insert or update.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<object?> UpsertLongRunningOperationResult(dynamic operationResult, CancellationToken cancellationToken = default);
    }
}
