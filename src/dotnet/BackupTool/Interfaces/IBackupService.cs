using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;

namespace BackupTool.Interfaces
{
    public interface IBackupService
    {
        /// <summary>
        /// Returns list of chat session ids.
        /// </summary>
        Task<List<Conversation>> GetAllConversationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns list of chat session messages.
        /// </summary>
        /// <param name="sessionId">The session id for which to retrieve chat session messages.</param>
        Task<List<Message>> GetConversationMessagesAsync(string sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns list of completion prompt responses.
        /// </summary>
        /// <param name="sessionId">The session id for which to retrieve completion prompt responses.</param>
        Task<List<CompletionPrompt>> GetCompletionPromptResponses(string sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns list of conversation mappings.
        /// </summary>
        /// <param name="sessionId">The session id for which to retrieve conversation mappings.</param>
        Task<List<AzureOpenAIConversationMapping>> GetConversationMappingResponses(string sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns list of file mappings.
        /// </summary>
        /// <param name="partitionKey">The partition key id for which to retrieve file mappings.</param>
        Task<List<AzureOpenAIFileMapping>> GetFileMappingResponses(string partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns an attachment.
        /// </summary>
        /// <param name="partitionKey">The partition key id for which to retrieve an attachment.</param>
        /// <param name="fileObjectId">The file object id for which to retrieve an attachment.</param>
        Task<AttachmentReference?> GetAttachment(string partitionKey, string fileObjectId, CancellationToken cancellationToken = default);
    }
}
