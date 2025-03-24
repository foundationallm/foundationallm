using FoundationaLLM.Common.Models.Context;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the interface for the Azure Cosmos DB service that services the Context API File service.
    /// </summary>
    public interface IAzureCosmosDBFileService
    {
        /// <summary>
        /// Upserts a file record in the Azure Cosmos DB.
        /// </summary>
        /// <param name="fileRecord">The file record to be upserted.</param>
        Task UpsertFileRecord(
            ContextFileRecord fileRecord);

        /// <summary>
        /// Gets the file record for a specified file identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The idenfier of the file.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the code session.</param>
        /// <returns></returns>
        Task<ContextFileRecord> GetFileRecord(
            string instanceId,
            string fileId,
            string userPrincipalName);

        /// <summary>
        /// Gets the file records matching the specified criteria.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The idenfier of the conversation to which the file is associated.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the code session.</param>
        /// <returns></returns>
        Task<List<ContextFileRecord>> GetFileRecords(
            string instanceId,
            string conversationId,
            string fileName,
            string userPrincipalName);
    }
}
