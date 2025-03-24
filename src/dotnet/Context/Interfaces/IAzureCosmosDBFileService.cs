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
        /// Gets the file records matching the specified criteria.
        /// </summary>
        /// <param name="conversationId">The idenfier of the conversation to which the file is associated.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the code session.</param>
        /// <returns></returns>
        Task<List<ContextFileRecord>> GetFileRecords(
            string conversationId,
            string fileName,
            string userPrincipalName);
    }
}
