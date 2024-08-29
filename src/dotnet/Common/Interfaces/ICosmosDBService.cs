using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Conversation;

namespace FoundationaLLM.Common.Interfaces;

/// <summary>
/// Contains methods for accessing Azure Cosmos DB for NoSQL.
/// </summary>
public interface ICosmosDBService
{
    /// <summary>
    /// Gets a list of all current chat sessions.
    /// </summary>
    /// <param name="type">The session type to return.</param>
    /// <param name="upn">The user principal name used for retrieving
    /// sessions for the signed in user.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>List of distinct chat session items.</returns>
    Task<List<Conversation>> GetSessionsAsync(string type, string upn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all current chat messages for a specified session identifier.
    /// </summary>
    /// <param name="sessionId">Chat session identifier used to filter messages.</param>
    /// <param name="upn">The user principal name used for retrieving the messages for
    /// the signed in user.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>List of chat message items for the specified session.</returns>
    Task<List<Message>> GetSessionMessagesAsync(string sessionId, string upn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a point read to retrieve a single chat session item.
    /// </summary>
    /// <returns>The chat session item.</returns>
    Task<Conversation> GetSessionAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    /// <param name="session">Chat session item to create.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Newly created chat session item.</returns>
    Task<Conversation> InsertSessionAsync(Conversation session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new chat message.
    /// </summary>
    /// <param name="message">Chat message item to create.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Newly created chat message item.</returns>
    Task<Message> InsertMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing chat message.
    /// </summary>
    /// <param name="message">Chat message item to update.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Revised chat message item.</returns>
    Task<Message> UpdateMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a message's rating through a patch operation.
    /// </summary>
    /// <param name="id">The message id.</param>
    /// <param name="sessionId">The message's partition key (session id).</param>
    /// <param name="rating">The rating to replace.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Revised chat message item.</returns>
    Task<Message> UpdateMessageRatingAsync(string id, string sessionId, bool? rating, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing chat session.
    /// </summary>
    /// <param name="session">Chat session item to update.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Revised created chat session item.</returns>
    Task<Conversation> UpdateSessionAsync(Conversation session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a session's name through a patch operation.
    /// </summary>
    /// <param name="id">The session id.</param>
    /// <param name="sessionName">The session's new name.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Revised chat session item.</returns>
    Task<Conversation> UpdateSessionNameAsync(string id, string sessionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch create or update chat messages and session.
    /// </summary>
    /// <param name="messages">Chat message and session items to create or replace.</param>
    Task UpsertSessionBatchAsync(params dynamic[] messages);

    /// <summary>
    /// Create or update a user session from the passed in Session object.
    /// </summary>
    /// <param name="session">The chat session item to create or replace.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task UpsertUserSessionAsync(Conversation session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch deletes an existing chat session and all related messages.
    /// </summary>
    /// <param name="sessionId">Chat session identifier used to flag messages and sessions for deletion.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    Task DeleteSessionAndMessagesAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all documents retrieved by Vector Search.
    /// </summary>
    /// <param name="vectorDocuments">List string of JSON documents from vector search results</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    Task<string> GetVectorSearchDocumentsAsync(List<DocumentVector> vectorDocuments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId);

    /// <summary>
    /// Returns the user profile for a given user via their UPN.
    /// </summary>
    /// <param name="upn">The user principal name used for retrieving the messages for
    /// the signed in user.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task<UserProfile> GetUserProfileAsync(string upn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a user profile.
    /// </summary>
    /// <param name="userProfile">The user profile to upsert.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task UpsertUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
}
