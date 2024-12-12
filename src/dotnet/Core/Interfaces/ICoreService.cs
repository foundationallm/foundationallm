using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Settings;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Contains methods for managing chat sessions and messages, and for getting completions from the
/// orchestrator.
/// </summary>
public interface ICoreService
{
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
    Task<Conversation> CreateConversationAsync(string instanceId, ChatSessionProperties chatSessionProperties);

    /// <summary>
    /// Rename the chat session from its default (eg., "New Chat") to the summary provided by OpenAI.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="sessionId">The session id to rename.</param>
    /// <param name="chatSessionProperties">The session properties.</param>
    Task<Conversation> RenameConversationAsync(string instanceId, string sessionId, ChatSessionProperties chatSessionProperties);

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
    /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
    /// <returns>A <see cref="ResourceProviderUpsertResult{T}"/> object with the FoundationaLLM.Attachment resource provider object id.</returns>
    Task<ResourceProviderUpsertResult<AttachmentFile>> UploadAttachment(
        string instanceId, string sessionId, AttachmentFile attachmentFile, string agentName, UnifiedUserIdentity userIdentity);

    /// <summary>
    /// Downloads an attachment.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="fileProvider">The name of the file provider.</param>
    /// <param name="fileId">The identifier of the file.</param>
    /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
    /// <returns>An <see cref="AttachmentFile"/> object with the properties and the content of the attachment.</returns>
    /// <remarks>
    /// The following file providers are supported:
    /// <list type="bullet">
    /// <item>FoundationaLLM.Attachments</item>
    /// <item>FoundationaLLM.AzureOpenAI</item>
    /// </list>
    /// </remarks>
    Task<AttachmentFile?> DownloadAttachment(
        string instanceId, string fileProvider, string fileId, UnifiedUserIdentity userIdentity);

    /// <summary>
    /// Deletes one or more attachments.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="resourcePaths">The list of resources to be deleted.</param>
    /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
    /// <returns>A dictionary with the delete operation result for each resource path.</returns>
    Task<Dictionary<string, ResourceProviderDeleteResult?>> DeleteAttachments(
        string instanceId, List<string> resourcePaths, UnifiedUserIdentity userIdentity);

    #endregion

    #region Conversation messages

    /// <summary>
    /// Returns the chat messages related to an existing session.
    /// </summary>
    /// <param name="instanceId">The instance id for which to retrieve chat messages.</param>
    /// <param name="sessionId">The session id for which to retrieve chat messages.</param>
    Task<List<Message>> GetChatSessionMessagesAsync(string instanceId, string sessionId);

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
    /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
    /// <returns>The file store configuration.</returns>
    Task<CoreConfiguration> GetCoreConfiguration(string instanceId, UnifiedUserIdentity userIdentity);

    #endregion
}
