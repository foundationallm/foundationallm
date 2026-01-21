using ConversationModels = FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Constants.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FoundationaLLM.Common.Authentication;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing sessions.
    /// </summary>
    /// <remarks>
    /// Constructor for the Sessions Controller.
    /// </remarks>
    /// <param name="coreService">The Core service provides methods for managing chat
    /// sessions and messages, and for getting completions from the orchestrator.</param>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="SessionsController"/> type name.</param>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [Authorize(
        AuthenticationSchemes = AgentAccessTokenDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.FoundationaLLMAgentAccessToken)]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class SessionsController(ICoreService coreService,
        ILogger<SessionsController> logger) : ControllerBase
    {
        private readonly ICoreService _coreService = coreService;
        private readonly ILogger<SessionsController> _logger = logger;

        /// <summary>
        /// Returns list of chat session ids and names.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        [HttpGet(Name = "GetAllChatSessions")]
        public async Task<IEnumerable<ConversationModels.Conversation>> GetAllChatSessions(string instanceId) =>
            await _coreService.GetAllConversationsAsync(instanceId);

        /// <summary>
        /// Get a single conversation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The identifier of the conversation to retrieve.</param>
        [HttpGet("{conversationId}", Name = "GetConversation")]
        public async Task<ConversationModels.Conversation> GetConversation(string instanceId, string conversationId) =>
            await _coreService.GetConversationAsync(instanceId, conversationId);

        /// <summary>
        /// Returns the messages for a conversation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The conversation identifier for which to retrieve messages.</param>
        [HttpGet("{sessionId}/messages", Name = "GetConversationMessages")]
        public async Task<IEnumerable<Message>> GetConversationMessages(string instanceId, string sessionId) =>
            await _coreService.GetConversationMessagesAsync(instanceId, sessionId);

        /// <summary>
        /// Returns the number of messages in a conversation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The conversation identifier for which to retrieve messages.</param>
        [HttpGet("{sessionId}/messagescount", Name = "GetConversationMessagesCount")]
        public async Task<int> GetConversationMessagesCount(string instanceId, string sessionId) =>
            await _coreService.GetConversationMessagesCountAsync(instanceId, sessionId);

        /// <summary>
        /// Rate an assistant message. This can be used to discover useful AI responses for training,
        /// discoverability, and other benefits down the road.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="messageId">The id of the message to rate.</param>
        /// <param name="sessionId">The id of the session to which the message belongs.</param>
        /// <param name="ratingRequest">The rating and optional comments to assign to the message.</param>
        [HttpPost("{sessionId}/message/{messageId}/rate", Name = "RateMessage")]
        public async Task RateMessage(string instanceId, string messageId, string sessionId, [FromBody] MessageRatingRequest ratingRequest) =>
            await _coreService.RateMessageAsync(instanceId, messageId, sessionId, ratingRequest);

        /// <summary>
        /// Returns the completion prompt for a given session and completion prompt id.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
        /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{sessionId}/completionprompts/{completionPromptId}", Name = "GetCompletionPrompt")]
        public async Task<CompletionPrompt> GetCompletionPrompt(string instanceId, string sessionId, string completionPromptId) =>
            await _coreService.GetCompletionPrompt(instanceId, sessionId, completionPromptId);

        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="chatSessionProperties">The session properties.</param>
        [HttpPost(Name = "CreateConversation")]
        public async Task<ConversationModels.Conversation> CreateConversation(string instanceId, [FromBody] ConversationProperties chatSessionProperties) =>
            await _coreService.CreateConversationAsync(instanceId, chatSessionProperties);

        /// <summary>
        /// Update the conversation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The identifier of the conversation to update.</param>
        /// <param name="conversationProperties">The conversation properties to be updated.</param>
        [HttpPost("{conversationId}/update", Name = "UpdateConversation")]
        public async Task<ConversationModels.Conversation> UpdateConversation(string instanceId, string conversationId, [FromBody] ConversationProperties conversationProperties) =>
            await _coreService.UpdateConversationAsync(instanceId, conversationId, conversationProperties);

        /// <summary>
        /// Delete a chat session and related messages.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="sessionId">The session id to delete.</param>
        [HttpDelete("{sessionId}", Name = "DeleteConversation")]
        public async Task DeleteConversation(string instanceId, string sessionId) =>
            await _coreService.DeleteConversationAsync(instanceId, sessionId);
    }
}
