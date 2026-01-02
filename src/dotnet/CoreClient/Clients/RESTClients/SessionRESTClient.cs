using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Conversation;
using System.ClientModel;
using System.Net.Http.Json;
using System.Text.Json;
using Message = FoundationaLLM.Common.Models.Conversation.Message;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's session endpoints.
    /// </summary>
    internal class SessionRESTClient : CoreRESTClientBase, ISessionRESTClient
    {
        private readonly string _instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionRESTClient"/> class with the specified HTTP client
        /// factory, token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public SessionRESTClient(
            IHttpClientFactory httpClientFactory,
            TokenCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionRESTClient"/> class with the specified HTTP client
        /// factory, agent access token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The agent access token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public SessionRESTClient(
            IHttpClientFactory httpClientFactory,
            ApiKeyCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<string> CreateSessionAsync(ConversationProperties chatSessionProperties)
        {
            var coreClient = await GetCoreClientAsync();
            var responseSession = await coreClient.PostAsync(
                $"instances/{_instanceId}/sessions",
                JsonContent.Create(chatSessionProperties));

            if (responseSession.IsSuccessStatusCode)
            {
                var responseContent = await responseSession.Content.ReadAsStringAsync();
                var sessionResponse = JsonSerializer.Deserialize<Conversation>(responseContent, SerializerOptions);
                if (sessionResponse?.SessionId != null)
                {
                    return sessionResponse.SessionId;
                }
            }

            throw new Exception($"Failed to create a new chat session. Status code: {responseSession.StatusCode}. Reason: {responseSession.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<string> RenameChatSession(string sessionId, ConversationProperties chatSessionProperties)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("A session ID must be provided when renaming a session.");
            }
            var coreClient = await GetCoreClientAsync();
            var response = await coreClient.PostAsync(
                $"instances/{_instanceId}/sessions/{sessionId}/rename",
                JsonContent.Create(chatSessionProperties));

            if (response.IsSuccessStatusCode)
            {
                return chatSessionProperties.Name ?? string.Empty;
            }

            throw new Exception($"Failed to rename chat session. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<CompletionPrompt> GetCompletionPromptAsync(string sessionId, string completionPromptId)
        {
            if (string.IsNullOrWhiteSpace(completionPromptId))
            {
                throw new ArgumentException(
                    "A completion prompt ID must be provided when retrieving a completion prompt.");
            }
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("A session ID must be provided when retrieving a completion prompt.");
            }
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync($"instances/{_instanceId}/sessions/{sessionId}/completionprompts/{completionPromptId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionPrompt =
                    JsonSerializer.Deserialize<CompletionPrompt>(responseContent, SerializerOptions);
                return completionPrompt ?? throw new InvalidOperationException("The returned completion prompt is invalid.");
            }

            throw new Exception($"Failed to get completion prompt. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetChatSessionMessagesAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) {
                throw new ArgumentException("A session ID must be provided when retrieving chat messages.");
            }
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync($"instances/{_instanceId}/sessions/{sessionId}/messages");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var messages = JsonSerializer.Deserialize<IEnumerable<Message>>(responseContent, SerializerOptions);
                foreach (Message message in messages!)
                {
                    if (message.Text == null && message.Content != null && message.Content.Count > 0)
                    {
                        message.Text = message.Content.First().Value!;
                    }
                }
                return messages ?? throw new InvalidOperationException("The returned messages are invalid.");
            }

            throw new Exception($"Failed to get chat session messages. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Conversation>> GetAllChatSessionsAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync($"instances/{_instanceId}/sessions");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var sessions = JsonSerializer.Deserialize<IEnumerable<Conversation>>(responseContent, SerializerOptions);
                return sessions ?? throw new InvalidOperationException("The returned sessions are invalid.");
            }

            throw new Exception($"Failed to retrieve chat sessions. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task RateMessageAsync(string sessionId, string messageId, MessageRatingRequest rating)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("A session ID must be provided when rating a message.");
            }
            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException("A message ID must be provided when rating a message.");
            }
            if (rating == null)
            {
                throw new ArgumentException("A rating must be provided when rating a message.");
            }
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.PostAsync($"instances/{_instanceId}/sessions/{sessionId}/message/{messageId}/rate",
                JsonContent.Create(rating));

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to rate message. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
            }
        }

        /// <inheritdoc/>
        public async Task DeleteSessionAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("A session ID must be provided when deleting a session.");
            }
            var coreClient = await GetCoreClientAsync();
            await coreClient.DeleteAsync($"instances/{_instanceId}/sessions/{sessionId}");
        }

        public Task RateMessageAsync(string sessionId, string messageId, bool rating) => throw new NotImplementedException();
    }
}
