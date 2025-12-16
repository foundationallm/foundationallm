using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Settings;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Contains methods for managing chat sessions and messages, and for getting completions from the
/// orchestrator.
/// </summary>
public interface ICoreService
{
    #region Agents

    /// <summary>
    /// Returns the list of agents available for the calling user.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync(string instanceId);

    #endregion

    #region Conversation management - FoundationaLLM.Conversation resource provider

    /// <summary>
    /// Returns list of chat session ids and names.
    /// </summary>
    /// <param name="instanceId">The instance id for which to retrieve chat sessions.</param>
    Task<List<Conversation>> GetAllConversationsAsync(string instanceId);

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    /// <param name="instanceId">The instance Id.</param>
    /// <param name="chatSessionProperties">The session properties.</param>
    Task<Conversation> CreateConversationAsync(string instanceId, ConversationProperties chatSessionProperties);

    /// <summary>
    /// Update the conversation.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="conversationId">The identifier of the conversation to rename.</param>
    /// <param name="conversationProperties">The conversation properties to update.</param>
    Task<Conversation> UpdateConversationAsync(string instanceId, string conversationId, ConversationProperties conversationProperties);

    /// <summary>
    /// Delete a chat session and related messages.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="sessionId">The session id to delete.</param>
    Task DeleteConversationAsync(string instanceId, string sessionId);

    #endregion

    #region Asynchronous completion operations

    /// <summary>
    /// Begins a completion operation.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
    /// <returns>Returns an <see cref="LongRunningOperation"/> object containing the OperationId and Status.</returns>
    Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest);

    /// <summary>
    /// Gets the status of a completion operation.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="operationId">The OperationId for which to retrieve the status.</param>
    /// <returns>Returns a <see cref="LongRunningOperation"/> object containing the OperationId, Status, and result.</returns>
    Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId);

    #endregion

    #region Synchronous completion operations

    /// <summary>
    /// Receive a prompt from a user, retrieve the message history from the related session,
    /// generate a completion response, and log full completion results.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="completionRequest">The completion request.</param>
    Task<Message> GetChatCompletionAsync(string instanceId, CompletionRequest completionRequest);

    /// <summary>
    /// Provides a completion for a user prompt, without a session.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="directCompletionRequest">The completion request.</param>
    Task<Message> GetCompletionAsync(string instanceId, CompletionRequest directCompletionRequest);

    #endregion

    #region Attachments

    /// <summary>
    /// Uploads an attachment.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="sessionId">The session id from which the attachment is uploaded.</param>
    /// <param name="attachmentFile">The <see cref="AttachmentFile"/> object containing the attachment file data.</param>
    /// <param name="agentName">The name of the agent.</param>
    /// <returns>A <see cref="ResourceProviderUpsertResult{T}"/> object with the FoundationaLLM.Attachment resource provider object id.</returns>
    Task<ResourceProviderUpsertResult<AttachmentFile>> UploadAttachment(
        string instanceId, string sessionId, AttachmentFile attachmentFile, string agentName);

    /// <summary>
    /// Downloads an attachment.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="fileProvider">The name of the file provider.</param>
    /// <param name="fileId">The identifier of the file.</param>
    /// <returns>An <see cref="AttachmentFile"/> object with the properties and the content of the attachment.</returns>
    /// <remarks>
    /// The following file providers are supported:
    /// <list type="bullet">
    /// <item>FoundationaLLM.Attachments</item>
    /// <item>FoundationaLLM.AzureOpenAI</item>
    /// </list>
    /// </remarks>
    Task<AttachmentFile?> DownloadAttachment(
        string instanceId, string fileProvider, string fileId);

    /// <summary>
    /// Deletes one or more attachments.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="resourcePaths">The list of resources to be deleted.</param>
    /// <returns>A dictionary with the delete operation result for each resource path.</returns>
    Task<Dictionary<string, ResourceProviderDeleteResult?>> DeleteAttachments(
        string instanceId, List<string> resourcePaths);

    #endregion

    #region Conversation messages

    /// <summary>
    /// Returns the messages for a conversation.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="sessionId">The conversation identifier for which to retrieve messages.</param>
    Task<List<Message>> GetConversationMessagesAsync(string instanceId, string sessionId);

    /// <summary>
    /// Returns the number of messages in a conversation.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="sessionId">The conversation identifier for which to retrieve messages.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of messages in the
    /// specified conversation.</returns>
    Task<int> GetConversationMessagesCountAsync(string instanceId, string sessionId);

    /// <summary>
    /// Rate an assistant message. This can be used to discover useful AI responses for training, discoverability, and other benefits down the road.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="id">The message id to rate.</param>
    /// <param name="sessionId">The session id to which the message belongs.</param>
    /// <param name="rating">The rating and optional comments to assign to the message.</param>
    Task RateMessageAsync(string instanceId, string id, string sessionId, MessageRatingRequest rating);

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="instanceId">The instance Id.</param>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    Task<CompletionPrompt> GetCompletionPrompt(string instanceId, string sessionId, string completionPromptId);

    #endregion

    #region Configuration

    /// <summary>
    /// Gets the file store configuration for the given instance.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <returns>The file store configuration.</returns>
    Task<CoreConfiguration> GetCoreConfiguration(string instanceId);

    #endregion
}
