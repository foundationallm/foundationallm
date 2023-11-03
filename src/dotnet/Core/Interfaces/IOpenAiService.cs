namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// This service is provided as an example, it is not used by the main RAG flow.
/// </summary>
public interface IOpenAiService
{
    /// <summary>
    /// Gets the maximum number of tokens to limit chat conversation length.
    /// </summary>
    int MaxConversationBytes { get; }

    /// <summary>
    /// Sends a prompt to the deployed OpenAI embeddings model and returns an array of vectors as a response.
    /// </summary>
    /// <param name="input">The input for which to create embeddings.</param>
    /// <param name="sessionId">Chat session identifier for the current conversation.</param>
    /// <returns>Response from the OpenAI model as an array of vectors along with tokens for the prompt and response.</returns>
    Task<(float[] response, int responseTokens)> GetEmbeddingsAsync(dynamic input, string sessionId);

    /// <summary>
    /// Sends a prompt to the deployed OpenAI embeddings model and returns an array of vectors as a response.
    /// </summary>
    /// <param name="input">The input for which to create embeddings.</param>
    /// <returns>Response from the OpenAI model as an array of vectors along with tokens for the prompt and response.</returns>
    Task<(float[] response, int responseTokens)> GetEmbeddingsAsync(dynamic input);

    /// <summary>
    /// Sends a prompt to the deployed OpenAI LLM model and returns the response.
    /// </summary>
    /// <param name="sessionId">Chat session identifier for the current conversation.</param>
    /// <param name="userPrompt">Prompt message to send to the deployment.</param>
    /// <param name="documents">Documents used for the data context.</param>
    /// <returns>Response from the OpenAI model along with tokens for the prompt and response.</returns>
    Task<(string response, int promptTokens, int responseTokens)> GetChatCompletionAsync(string sessionId, string userPrompt, string documents);

    /// <summary>
    /// Sends the existing conversation to the OpenAI model and returns a two word summary.
    /// </summary>
    /// <param name="sessionId">Chat session identifier for the current conversation.</param>
    /// <param name="userPrompt">The first User Prompt and Completion to send to the deployment.</param>
    /// <returns>Summarization response from the OpenAI model deployment.</returns>
    Task<string> SummarizeAsync(string sessionId, string userPrompt);
}