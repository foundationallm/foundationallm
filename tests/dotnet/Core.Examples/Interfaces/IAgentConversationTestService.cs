using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Core.Examples.Models;

namespace FoundationaLLM.Core.Examples.Interfaces;

public interface IAgentConversationTestService
{
    /// <summary>
    /// Runs a complete conversation with an agent using the Core API and a chat session.
    /// To specify an existing session, provide the session ID. Otherwise, a new session will be created.
    /// This method sends user prompts to the agent and returns the conversation history.
    /// After the conversation is complete, the session is deleted unless you provided an existing
    /// session ID.
    /// </summary>
    /// <param name="agentName">The name of the agent to use.</param>
    /// <param name="userPrompts">The user prompts to send to the agent for the conversation.</param>
    /// <param name="sessionId">Specifies an existing session ID to use for the test. If the value is null,
    /// a new session is created then deleted as part of the test run.</param>
    /// <param name="createAgent">If true, the test run will create an agent and its dependencies before the
    /// run and delete these resources when the run completes. This is a potentially destructive action. Do
    /// not enable this option if you wish to use pre-existing resources, such as the default FoundationaLLM agent.</param>
    /// <returns></returns>
    Task<IEnumerable<Message>> RunAgentConversationWithSession(string agentName,
        List<string> userPrompts, string? sessionId = null, bool createAgent = false, string? indexingProfileName = null,
                string? textEmbeddingProfileName = null, string? textPartitioningProfileName = null);

    /// <summary>
    /// Runs a single completion with an agent using the Core API and a chat session.
    /// To specify an existing session, provide the session ID. Otherwise, a new session will be created.
    /// This method sends a user prompt to the agent and returns the completion response.
    /// After the conversation is complete, the session is deleted unless you provided an existing
    /// session ID.
    /// </summary>
    /// <param name="agentName">The name of the agent to use.</param>
    /// <param name="userPrompt">The user prompt to send to the agent.</param>
    /// <param name="sessionId">Specifies an existing session ID to use for the test. If the value is null,
    /// a new session is created then deleted as part of the test run.</param>
    /// <param name="createAgent">If true, the test run will create an agent and its dependencies before the
    /// run and delete these resources when the run completes. This is a potentially destructive action. Do
    /// not enable this option if you wish to use pre-existing resources, such as the default FoundationaLLM agent.</param>
    /// <returns></returns>
    Task<Completion> RunAgentCompletionWithSession(string agentName,
        string userPrompt, string? sessionId = null, bool createAgent = false);

    /// <summary>
    /// Runs a single completion with an agent using the Core API without a chat session (sessionless).
    /// This method sends a user prompt to the agent and returns the completion response.
    /// </summary>
    /// <param name="agentName">The name of the agent to use.</param>
    /// <param name="userPrompt">The user prompt to send to the agent.</param>
    /// <param name="createAgent">If true, the test run will create an agent and its dependencies before the
    /// run and delete these resources when the run completes. This is a potentially destructive action. Do
    /// not enable this option if you wish to use pre-existing resources, such as the default FoundationaLLM agent.</param>
    /// <returns></returns>
    Task<Completion> RunAgentCompletionWithNoSession(string agentName,
        string userPrompt, bool createAgent = false);

    /// <summary>
    /// Runs a single completion with an agent using the Core API and a chat session, then measures the quality
    /// of the completion with Azure AI Studio.
    /// To specify an existing session, provide the session ID. Otherwise, a new session will be created.
    /// After the quality measurement job is submitted, the session is deleted unless you provided an existing
    /// session ID.
    /// </summary>
    /// <param name="agentName">The name of the agent to use.</param>
    /// <param name="userPrompt">The user prompt to send to the agent.</param>
    /// <param name="expectedCompletion">The expected agent completion used for quality measurements.</param>
    /// <param name="sessionId">Specifies an existing session ID to use for the test. If the value is null,
    /// a new session is created then deleted as part of the test run.</param>
    /// <param name="createAgent">If true, the test run will create an agent and its dependencies before the
    /// run and delete these resources when the run completes. This is a potentially destructive action. Do
    /// not enable this option if you wish to use pre-existing resources, such as the default FoundationaLLM agent.</param>
    /// <returns></returns>
    Task<CompletionQualityMeasurementOutput> RunAgentCompletionWithQualityMeasurements(string agentName,
        string userPrompt, string expectedCompletion, string? sessionId = null, bool createAgent = false);
}