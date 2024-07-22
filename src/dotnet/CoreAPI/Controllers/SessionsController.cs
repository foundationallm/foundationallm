﻿using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for interacting with the Core service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Core Controller.
    /// </remarks>
    /// <param name="coreService">The Core service provides methods for managing chat
    /// sessions and messages, and for getting completions from the orchestrator.</param>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="SessionsController"/> type name.</param>
    [Authorize(Policy = "DefaultPolicy")]
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
        public async Task<IEnumerable<Session>> GetAllChatSessions(string instanceId) =>
            await _coreService.GetAllChatSessionsAsync(instanceId);

        /// <summary>
        /// Returns the chat messages related to an existing session.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="sessionId">The id of the session for which to retrieve chat messages.</param>
        [HttpGet("{sessionId}/messages", Name = "GetChatSessionMessages")]
        public async Task<IEnumerable<Message>> GetChatSessionMessages(string instanceId, string sessionId) =>
            await _coreService.GetChatSessionMessagesAsync(instanceId, sessionId);

        /// <summary>
        /// Rate an assistant message. This can be used to discover useful AI responses for training,
        /// discoverability, and other benefits down the road.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="messageId">The id of the message to rate.</param>
        /// <param name="sessionId">The id of the session to which the message belongs.</param>
        /// <param name="rating">The rating to assign to the message.</param>
        [HttpPost("{sessionId}/message/{messageId}/rate", Name = "RateMessage")]
        public async Task<Message> RateMessage(string instanceId, string messageId, string sessionId, bool? rating) =>
            await _coreService.RateMessageAsync(instanceId, messageId, sessionId, rating);

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
        [HttpPost(Name = "CreateNewChatSession")]
        public async Task<Session> CreateNewChatSession(string instanceId) =>
            await _coreService.CreateNewChatSessionAsync(instanceId);

        /// <summary>
        /// Rename the chat session.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="sessionId">The id of the session to rename.</param>
        /// <param name="newChatSessionName">The new name for the session.</param>
        [HttpPost("{sessionId}/rename", Name = "RenameChatSession")]
        public async Task<Session> RenameChatSession(string instanceId, string sessionId, string newChatSessionName) =>
            await _coreService.RenameChatSessionAsync(instanceId, sessionId, newChatSessionName);

        /// <summary>
        /// Delete a chat session and related messages.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="sessionId">The session id to delete.</param>
        [HttpDelete("{sessionId}", Name = "DeleteChatSession")]
        public async Task DeleteChatSession(string instanceId, string sessionId) =>
            await _coreService.DeleteChatSessionAsync(instanceId, sessionId);

        /// <summary>
        /// Generate a name for a chat message, based on the passed in prompt.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="sessionId">The id of the session for which to generate a name.</param>
        /// <param name="text">The prompt to use to generate the name.</param>
        [HttpPost("{sessionId}/generate-name", Name = "GenerateChatSessionName")]
        public async Task<Completion> GenerateChatSessionName(string instanceId, string sessionId, [FromBody] string text) =>
            await _coreService.GenerateChatSessionNameAsync(instanceId, sessionId, text);
    }
}
