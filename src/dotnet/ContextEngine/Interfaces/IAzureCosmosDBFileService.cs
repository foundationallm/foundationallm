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
        /// <param name="userPrincipalName">The user principal name of the user associated with the file record.</param>
        /// <param name="bypassOwnerCheck">Indicates whether to bypass the owner check for the file record.</param>
        /// <returns></returns>
        Task<ContextFileRecord> GetFileRecord(
            string instanceId,
            string fileId,
            string userPrincipalName,
            bool bypassOwnerCheck);

        /// <summary>
        /// Gets the file records matching the specified criteria.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The idenfier of the conversation to which the file is associated.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the file record.</param>
        /// <param name="bypassOwnerCheck">Indicates whether to bypass the owner check for the file records.</param>
        /// <returns></returns>
        Task<List<ContextFileRecord>> GetFileRecords(
            string instanceId,
            string conversationId,
            string fileName,
            string userPrincipalName,
            bool bypassOwnerCheck);

        /// <summary>
        /// Deletes the file record for a specified file identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The idenfier of the file.</param>
        /// <param name="userPrincipalName">The user principal name of the user associated with the file record.</param>
        /// <returns></returns>
        /// <remarks>Bypassing owner check cannot be performed here as we need the partition key (which is the UPN)
        /// to perform the delete operation.</remarks>
        Task DeleteFileRecord(
            string instanceId,
            string fileId,
            string userPrincipalName);
    }
}
