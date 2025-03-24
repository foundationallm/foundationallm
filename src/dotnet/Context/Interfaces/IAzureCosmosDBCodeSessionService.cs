using FoundationaLLM.Common.Models.Context;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the interface for the Azure Cosmos DB service that services the Context API Code Session service.
    /// </summary>
    public interface IAzureCosmosDBCodeSessionService
    {
        /// <summary>
        /// Upserts a code session record in the Azure Cosmos DB.
        /// </summary>
        /// <param name="codeSessionRecord">The code session record to be upserted.</param>
        Task UpsertCodeSessionRecord(
            ContextCodeSessionRecord codeSessionRecord);

        /// <summary>
        /// Upserts a code session file upload record in the Azure Cosmos DB.
        /// </summary>
        /// <param name="codeSessionFileUploadRecord">The code session file upload record to be upserted.</param>
        Task UpsertCodeSessionFileUploadRecord(
            ContextCodeSessionFileUploadRecord codeSessionFileUploadRecord);

        /// <summary>
        /// Gets a code session record from the Azure Cosmos DB.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the code session.</param>
        /// <returns>The requests code session record.</returns>
        Task<ContextCodeSessionRecord?> GetCodeSessionRecord(
            string instanceId,
            string sessionId,
            string userPrincipalName);

        /// <summary>
        /// Gets a code session file upload record from the Azure Cosmos DB.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session.</param>
        /// <param name="operationId">The identifier of the code session file upload record.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the code session file upload.</param>
        /// <returns>The requested code session file upload record.</returns>
        Task<ContextCodeSessionFileUploadRecord?> GetCodeSessionFileUploadRecord(
            string instanceId,
            string sessionId,
            string operationId,
            string userPrincipalName);
    }
}
