using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

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
            string sessionId,
            string userPrincipalName)
        {
            var select =
                $"SELECT * FROM c WHERE c.id = @sessionId AND c.type = @type AND c.upn = @upn AND {SOFT_DELETE_RESTRICTION}";
            var query = new QueryDefinition(select)
                    .WithParameter("@sessionId", sessionId)
                    .WithParameter("@type", ContextRecordTypeNames.CodeSessionRecord)
                    .WithParameter("@upn", userPrincipalName);

            var results = _cosmosDB.ContextContainer.GetItemQueryIterator<ContextCodeSessionRecord>(query);

            List<ContextCodeSessionRecord> output = [];
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync();
                output.AddRange(response);
            }

            return output.Count == 1 ? output[0] : null;
        }
    }
}
