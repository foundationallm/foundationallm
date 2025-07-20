using FoundationaLLM.Common.Models.Conversation;

namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's session endpoints.
    /// </summary>
    public interface ISessionRESTClient
    {
        /// <summary>
        /// Retrieves all chat sessions.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Conversation>> GetAllChatSessionsAsync();

        /// <summary>
        /// Sets the rating for a message.
        /// </summary>
        /// <param name="sessionId">The chat session ID that contains the message to rate.</param>
        /// <param name="messageId">The ID of the message to rate.</param>
        /// <param name="rating">The rating and optional comments to assign to the message.</param>
        Task RateMessageAsync(string sessionId, string messageId, MessageRatingRequest rating);

        /// <summary>
        /// Creates a new session with the specified name.
        /// </summary>
        /// <param name="chatSessionProperties">The session properties.</param>
        /// <returns>Returns the new Session ID.</returns>
        Task<string> CreateSessionAsync(ConversationProperties chatSessionProperties);

        /// <summary>
        /// Renames a chat session.
        /// </summary>
        /// <param name="sessionId">The chat session ID.</param>
        /// <param name="chatSessionProperties">The session properties.</param>
        /// <returns></returns>
        Task<string> RenameChatSession(string sessionId, ConversationProperties chatSessionProperties);

        /// <summary>
        /// Gets a completion prompt by session ID and completion prompt ID.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="completionPromptId"></param>
        /// <returns></returns>
        Task<CompletionPrompt> GetCompletionPromptAsync(string sessionId, string completionPromptId);

        /// <summary>
        /// Returns the chat messages related to an existing session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<IEnumerable<Message>> GetChatSessionMessagesAsync(string sessionId);

        /// <summary>
        /// Deletes a chat session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task DeleteSessionAsync(string sessionId);
    }
}
