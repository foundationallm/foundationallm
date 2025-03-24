using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Context.Services.CosmosDB
{
    /// <summary>
    /// Provides the implementation for the Azure Cosmos DB file service.
    /// </summary>
    /// <param name="azureCosmosDBServiceBase">The Azure Cosmos DB service that provides core database services.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class AzureCosmosDBFileService(
        IAzureCosmosDBServiceBase azureCosmosDBServiceBase,
        ILogger<AzureCosmosDBFileService> logger) : IAzureCosmosDBFileService
    {
        private readonly IAzureCosmosDBServiceBase _cosmosDB = azureCosmosDBServiceBase;
        private readonly ILogger<AzureCosmosDBFileService> _logger = logger;

        private const string SOFT_DELETE_RESTRICTION = "(not IS_DEFINED(c.deleted) OR c.deleted = false)";

        /// <inheritdoc/>
        public async Task UpsertFileRecord(ContextFileRecord fileRecord) =>
            await _cosmosDB.UpsertItemAsync<ContextFileRecord>(fileRecord.UPN, fileRecord);

        /// <inheritdoc/>
        public async Task<ContextFileRecord> GetFileRecord(
            string instanceId,
            string fileId,
            string userPrincipalName)
        {
            var select =
                $"SELECT * FROM c WHERE c.instance_id = @instanceId AND c.type = @type AND c.upn = @upn AND c.id = @fileId  AND {SOFT_DELETE_RESTRICTION}";
            var query = new QueryDefinition(select)
                    .WithParameter("@instanceId", instanceId)
                    .WithParameter("@type", ContextRecordTypeNames.FileRecord)
                    .WithParameter("@upn", userPrincipalName)
                    .WithParameter("@fileId", fileId);

            var results = await _cosmosDB.RetrieveItems<ContextFileRecord>(query);

            if (results.Count == 0)
                throw new Exception($"File record with id {fileId} not found.");

            return results.First();
        }

        /// <inheritdoc/>
        public async Task<List<ContextFileRecord>> GetFileRecords(
            string instanceId,
            string conversationId,
            string fileName,
            string userPrincipalName)
        {
            var select =
                $"SELECT * FROM c WHERE c.instance_id = @instanceId AND c.type = @type AND c.upn = @upn AND c.conversation_id = @conversationId AND c.file_name = @fileName  AND {SOFT_DELETE_RESTRICTION} ORDER BY c.created_at DESC";
            var query = new QueryDefinition(select)
                    .WithParameter("@instanceId", instanceId)
                    .WithParameter("@type", ContextRecordTypeNames.FileRecord)
                    .WithParameter("@upn", userPrincipalName)
                    .WithParameter("@conversationId", conversationId)
                    .WithParameter("@fileName", fileName);

            var results = await _cosmosDB.RetrieveItems<ContextFileRecord>(query);

            return results;
        }
    }
}
