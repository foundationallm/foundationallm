using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.CallRecords;

namespace FoundationaLLM.Context.Services.CosmosDB
{
    /// <summary>
    /// Provides the implementation for the Azure Cosmos DB code session service.
    /// </summary>
    /// <param name="azureCosmosDBServiceBase">The Azure Cosmos DB service that provides core database services.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class AzureCosmosDBCodeSessionService(
        IAzureCosmosDBServiceBase azureCosmosDBServiceBase,
        ILogger<AzureCosmosDBCodeSessionService> logger) : IAzureCosmosDBCodeSessionService
    {
        private readonly IAzureCosmosDBServiceBase _cosmosDB = azureCosmosDBServiceBase;
        private readonly ILogger<AzureCosmosDBCodeSessionService> _logger = logger;

        private const string SOFT_DELETE_RESTRICTION = "(not IS_DEFINED(c.deleted) OR c.deleted = false)";

        /// <inheritdoc/>
        public async Task UpsertCodeSessionRecord(ContextCodeSessionRecord codeSessionRecord) =>
            await _cosmosDB.UpsertItemAsync<ContextCodeSessionRecord>(
                codeSessionRecord.UPN,
                codeSessionRecord);

        /// <inheritdoc/>
        public async Task UpsertCodeSessionFileUploadRecord(ContextCodeSessionFileUploadRecord codeSessionFileUploadRecord) =>
            await _cosmosDB.UpsertItemAsync<ContextCodeSessionFileUploadRecord>(
                codeSessionFileUploadRecord.UPN,
                codeSessionFileUploadRecord);

        /// <inheritdoc/>
        public async Task<ContextCodeSessionRecord?> GetCodeSessionRecord(
            string instanceId,
            string sessionId,
            string userPrincipalName)
        {
            var select =
                $"SELECT * FROM c WHERE c.instance_id = @instanceId AND c.type = @type AND c.upn = @upn AND c.id = @sessionId AND {SOFT_DELETE_RESTRICTION}";
            var query = new QueryDefinition(select)
                    .WithParameter("@instanceId", instanceId)
                    .WithParameter("@type", ContextRecordTypeNames.CodeSessionRecord)
                    .WithParameter("@upn", userPrincipalName)
                    .WithParameter("@sessionId", sessionId);

            var results = await _cosmosDB.RetrieveItems<ContextCodeSessionRecord>(query);

            return results.Count == 1 ? results[0] : null;
        }

        /// <inheritdoc/>
        public async Task<ContextCodeSessionFileUploadRecord?> GetCodeSessionFileUploadRecord(
            string instanceId,
            string sessionId,
            string operationId,
            string userPrincipalName)
        {
            var select =
                $"SELECT * FROM c WHERE c.instance_id = @instanceId AND c.type = @type AND c.upn = @upn AND c.code_session_id = @sessionId AND c.id = @operationId AND {SOFT_DELETE_RESTRICTION}";
            var query = new QueryDefinition(select)
                    .WithParameter("@instanceId", instanceId)
                    .WithParameter("@type", ContextRecordTypeNames.CodeSessionFileUploadRecord)
                    .WithParameter("@upn", userPrincipalName)
                    .WithParameter("@sessionId", sessionId)
                    .WithParameter("@operationId", operationId);

            var results = await _cosmosDB.RetrieveItems<ContextCodeSessionFileUploadRecord>(query);

            return results.Count == 1
                ? results[0]
                : null;
        }
    }
}
