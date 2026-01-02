using System.Text.Json;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.State.Interfaces
{
    /// <summary>
    /// Provides methods for managing state for long-running operations.
    /// </summary>
    public interface IStateService
    {
        /// <summary>
        /// Retrieves all long-running operations.
        /// </summary>
        /// <returns></returns>
        Task<List<LongRunningOperation>> GetLongRunningOperations();

        /// <summary>
        /// Retrieves a long-running operation by its identifier.
        /// </summary>
        /// <param name="id">The long-running operation identifier.</param>
        /// <returns></returns>
        Task<LongRunningOperation> GetLongRunningOperation(string id);

        /// <summary>
        /// Retrieves all log entries for a long-running operation.
        /// </summary>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <returns></returns>
        Task<List<LongRunningOperationLogEntry>> GetLongRunningOperationLogEntries(string operationId);

        /// <summary>
        /// Retrieves the result of a long-running operation.
        /// </summary>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <returns></returns>
        Task<JsonDocument?> GetLongRunningOperationResult(string operationId);

        /// <summary>
        /// Inserts or updates a long-running operation and creates a log entry.
        /// </summary>
        /// <param name="operation">The long-running operation to insert or update.</param>
        /// <returns></returns>
        Task<LongRunningOperation> UpsertLongRunningOperation(LongRunningOperation operation);

        /// <summary>
        /// Creates a new long-running operation with default values.
        /// </summary>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <param name="upn">The User Principal Name (UPN) of the user who triggered the long-running operation.</param>
        /// <returns></returns>
        Task<LongRunningOperation> CreateLongRunningOperation(string operationId, string upn);

        /// <summary>
        /// Inserts or updates the result of a long-running operation.
        /// </summary>
        /// <param name="operationResult">The operation result to insert or update.</param>
        /// <returns></returns>
        Task<object?> UpsertLongRunningOperationResult(dynamic operationResult);
    }
}
