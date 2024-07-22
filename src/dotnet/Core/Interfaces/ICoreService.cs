﻿using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Contains methods for managing chat sessions and messages, and for getting completions from the
/// orchestrator.
/// </summary>
public interface ICoreService
{
    /// <summary>
    /// Returns list of chat session ids and names.
    /// </summary>
    /// <param name="instanceId">The instance id for which to retrieve chat sessions.</param>
    Task<List<Session>> GetAllChatSessionsAsync(string instanceId);

    /// <summary>
    /// Returns the chat messages related to an existing session.
    /// </summary>
    /// <param name="instanceId">The instance id for which to retrieve chat messages.</param>
    /// <param name="sessionId">The session id for which to retrieve chat messages.</param>
    Task<List<Message>> GetChatSessionMessagesAsync(string instanceId, string sessionId);

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    /// <param name="instanceId">The instance Id.</param>
    Task<Session> CreateNewChatSessionAsync(string instanceId);

    /// <summary>
    /// Rename the chat session from its default (eg., "New Chat") to the summary provided by OpenAI.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="sessionId">The session id to rename.</param>
    /// <param name="newChatSessionName">The new name for the chat session.</param>
    Task<Session> RenameChatSessionAsync(string instanceId, string sessionId, string newChatSessionName);

    /// <summary>
    /// Delete a chat session and related messages.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="sessionId">The session id to delete.</param>
    Task DeleteChatSessionAsync(string instanceId, string sessionId);

    /// <summary>
    /// Receive a prompt from a user, retrieve the message history from the related session,
    /// generate a completion response, and log full completion results.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="completionRequest">The completion request.</param>
    Task<Completion> GetChatCompletionAsync(string instanceId, CompletionRequest completionRequest);

    /// <summary>
    /// Provides a completion for a user prompt, without a session.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="directCompletionRequest">The completion request.</param>
    Task<Completion> GetCompletionAsync(string instanceId, CompletionRequest directCompletionRequest);

    /// <summary>
    /// Generate a name for a chat message, based on the passed in prompt.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="sessionId">The session id to rename.</param>
    /// <param name="text">The text to use when generating the name.</param>
    Task<Completion> GenerateChatSessionNameAsync(string instanceId, string? sessionId, string? text);

    /// <summary>
    /// Rate an assistant message. This can be used to discover useful AI responses for training, discoverability, and other benefits down the road.
    /// </summary>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="id">The message id to rate.</param>
    /// <param name="sessionId">The session id to which the message belongs.</param>
    /// <param name="rating">The rating to assign to the message.</param>
    Task<Message> RateMessageAsync(string instanceId, string id, string sessionId, bool? rating);

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="instanceId">The instance Id.</param>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    Task<CompletionPrompt> GetCompletionPrompt(string instanceId, string sessionId, string completionPromptId);
}
