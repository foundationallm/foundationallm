using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.State.Interfaces;
using FoundationaLLM.State.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.State.Services
{
    /// <summary>
    /// Provides methods for managing state for long-running operations.
    /// </summary>
    /// <param name="options">Provides the options with the <see cref="StateServiceSettings"/> settings for configuration.</param>
    /// <param name="cosmosDbService">Provides methods to interact with Cosmos DB.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class StateService(
        IOptions<StateServiceSettings> options,
        IStateCosmosDBService cosmosDbService,
        ILogger<StateService> logger) : IStateService
    {
        /// <inheritdoc/>
        public async Task<List<LongRunningOperation>> GetLongRunningOperations()
        {
            logger.LogInformation("Getting long running operations.");
            return await cosmosDbService.GetLongRunningOperations();
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> GetLongRunningOperation(string id)
        {
            logger.LogInformation("Getting long running operation with ID: {id}", id);
            return await cosmosDbService.GetLongRunningOperation(id);
        }

        /// <inheritdoc/>
        public async Task<List<LongRunningOperationLogEntry>> GetLongRunningOperationLogEntries(string operationId)
        {
            logger.LogInformation("Getting long running operation log entries for operation ID: {operationId}", operationId);
            return await cosmosDbService.GetLongRunningOperationLogEntries(operationId);
        }

        /// <inheritdoc/>
        public async Task<JsonDocument?> GetLongRunningOperationResult(string operationId)
        {
            logger.LogInformation("Getting long running operation result for operation ID: {operationId}", operationId);
            return await cosmosDbService.GetLongRunningOperationResult(operationId);
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> CreateLongRunningOperation(string operationId, string upn)
        {
            logger.LogInformation("Creating long running operation.");
            var operation = new LongRunningOperation
            {
                Status = OperationStatus.InProgress,
                StatusMessage = "Operation started.",
                OperationId = operationId,
                UPN = upn
            };
            return await cosmosDbService.UpsertLongRunningOperation(operation);
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> UpsertLongRunningOperation(LongRunningOperation operation)
        {
            logger.LogInformation("Upserting long running operation.");
            return await cosmosDbService.UpsertLongRunningOperation(operation);
        }

        /// <inheritdoc/>
        public async Task<object?> UpsertLongRunningOperationResult(dynamic operationResult)
        {
            logger.LogInformation("Upserting long running operation result.");

            var operationResultDict = operationResult as IDictionary<string, object>;

            if (operationResultDict == null)
            {
                throw new ArgumentException("The operation result must be an ExpandoObject.");
            }

            if (!operationResultDict.ContainsKey("id") || string.IsNullOrEmpty(operationResultDict["id"]?.ToString()))
            {
                operationResultDict["id"] = Guid.NewGuid().ToString();
            }

            return await cosmosDbService.UpsertLongRunningOperationResult(operationResult);
        }
    }
}
