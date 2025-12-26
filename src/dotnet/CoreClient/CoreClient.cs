using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using System.ClientModel;
using System.Text.Json;

namespace FoundationaLLM.Client.Core
{
    /// <inheritdoc/>
    public class CoreClient : ICoreClient
    {
        private readonly ICoreRESTClient _coreRestClient;

        /// <summary>
        /// Constructor for mocking. This does not initialize the clients.
        /// </summary>
        public CoreClient() =>
            _coreRestClient = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClient"/> class with
        /// the specified Core API URI and TokenCredential.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        public CoreClient(
            string coreUri,
            TokenCredential credential,
            string instanceId)
            : this(coreUri, credential, instanceId,  new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClient"/> class with
        /// the specified Core API URI, TokenCredential, and optional client settings.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public CoreClient(
            string coreUri,
            TokenCredential credential,
            string instanceId,
            APIClientSettings options) =>
            _coreRestClient = new CoreRESTClient(coreUri, credential, instanceId, options);

            /// <summary>
            /// Initializes a new instance of the <see cref="CoreClient"/> class with
            /// the specified Core API URI and API key credential.
            /// </summary>
            /// <param name="coreUri">The base URI of the Core API.</param>
            /// <param name="credential">An <see cref="ApiKeyCredential"/> containing a valid
            /// agent access token.</param>
            /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
            /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
            /// (FoundationaLLM:Instance:Id key)</param>
        public CoreClient(
            string coreUri,
            ApiKeyCredential credential,
            string instanceId)
            : this(coreUri, credential, instanceId, new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClient"/> class with
        /// the specified Core API URI, API key credential, and optional client settings.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">An <see cref="ApiKeyCredential"/> containing a valid
        /// agent access token.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public CoreClient(
            string coreUri,
            ApiKeyCredential credential,
            string instanceId,
            APIClientSettings options) =>
            _coreRestClient = new CoreRESTClient(coreUri, credential, instanceId, options);

        /// <inheritdoc/>
        public async Task<string> CreateChatSessionAsync(ConversationProperties chatSessionProperties)
        {
            if (string.IsNullOrWhiteSpace(chatSessionProperties.Name))
                throw new ArgumentException("A session name must be provided when creating a new session.");

            var sessionId = await _coreRestClient.Sessions.CreateSessionAsync(chatSessionProperties);
            return sessionId;
        }

        /// <summary>
        /// Rates a message in a session.
        /// </summary>
        /// <param name="sessionId">The session ID.</param>
        /// <param name="messageId">The message ID.</param>
        /// <param name="rating">The rating request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RateMessageAsync(string sessionId, string messageId, MessageRatingRequest rating)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("A session ID must be provided when rating a message.");
            if (string.IsNullOrWhiteSpace(messageId))
                throw new ArgumentException("A message ID must be provided when rating a message.");
            if (rating == null)
                throw new ArgumentException("A rating must be provided when rating a message.");
            await _coreRestClient.Sessions.RateMessageAsync(sessionId, messageId, rating);
        }

        /// <inheritdoc/>
        public async Task<Message> GetCompletionWithSessionAsync(string? sessionId, ConversationProperties? chatSessionProperties,
            string userPrompt, string agentName)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                if (chatSessionProperties == null)
                {
                    throw new ArgumentException(
                        "The completion request must contain a session name if no session Id is provided. " +
                        "A new session will be created with the provided session name.");
                }

                sessionId = await CreateChatSessionAsync(chatSessionProperties);
            }

            var orchestrationRequest = new CompletionRequest
            {
                OperationId = Guid.NewGuid().ToString(),
                AgentName = agentName,
                SessionId = sessionId,
                UserPrompt = userPrompt
            };

            if (chatSessionProperties is not null
                && !string.IsNullOrWhiteSpace(chatSessionProperties.Metadata))
                orchestrationRequest.Metadata =
                    JsonSerializer.Deserialize<Dictionary<string, object>>(chatSessionProperties.Metadata);

            return await GetCompletionWithSessionAsync(orchestrationRequest);
        }

        /// <inheritdoc/>
        public async Task<Message> GetCompletionWithSessionAsync(CompletionRequest completionRequest)
        {
            if (string.IsNullOrWhiteSpace(completionRequest.SessionId) ||
                string.IsNullOrWhiteSpace(completionRequest.AgentName) ||
                string.IsNullOrWhiteSpace(completionRequest.UserPrompt))
            {
                throw new ArgumentException("The completion request must contain a SessionID, AgentName, and UserPrompt at a minimum.");
            }

            var completion = await GetCompletionAsync(completionRequest);
            return completion;
        }

        /// <inheritdoc/>
        public async Task<Message> GetCompletionAsync(string userPrompt, string agentName)
        {
            var completionRequest = new CompletionRequest
            {
                OperationId = Guid.NewGuid().ToString(),
                AgentName = agentName,
                UserPrompt = userPrompt
            };

            return await GetCompletionAsync(completionRequest);
        }

        /// <inheritdoc/>
        public async Task<Message> GetCompletionAsync(CompletionRequest completionRequest)
        {
            if (string.IsNullOrWhiteSpace(completionRequest.AgentName) ||
                string.IsNullOrWhiteSpace(completionRequest.UserPrompt))
            {
                throw new ArgumentException("The completion request must contain an AgentName and UserPrompt at a minimum.");
            }

            var completion = await _coreRestClient.Completions.GetChatCompletionAsync(completionRequest);
            return completion;
        }

        /// <inheritdoc/>
        public async Task<Message> AttachFileAndAskQuestionAsync(Stream fileStream, string fileName, string contentType,
            string agentName, string question, bool useSession, string? sessionId, ConversationProperties? chatSessionProperties)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            var objectId = await _coreRestClient.Attachments.UploadAttachmentAsync(fileStream, fileName, contentType);

            if (useSession)
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    if (chatSessionProperties == null)
                    {
                        throw new ArgumentException(
                            "The completion request must contain a session name if no session Id is provided. " +
                            "A new session will be created with the provided session name.");
                    }

                    sessionId = await CreateChatSessionAsync(chatSessionProperties);
                }

                var orchestrationRequest = new CompletionRequest
                {
                    OperationId = Guid.NewGuid().ToString(),
                    AgentName = agentName,
                    SessionId = sessionId,
                    UserPrompt = question,
                    Attachments = [objectId]
                };
                var sessionCompletion = await GetCompletionAsync(orchestrationRequest);

                return sessionCompletion;
            }

            // Use the orchestrated completion request to ask a question about the file.
            var completionRequest = new CompletionRequest
            {
                OperationId = Guid.NewGuid().ToString(),
                AgentName = agentName,
                UserPrompt = question,
                Attachments = [objectId]
            };
            var completion = await _coreRestClient.Completions.GetChatCompletionAsync(completionRequest);

            return completion;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetChatSessionMessagesAsync(string sessionId) => await _coreRestClient.Sessions.GetChatSessionMessagesAsync(sessionId);

        /// <inheritdoc/>
        public async Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync()
        {
            var agents = await _coreRestClient.Completions.GetAgentsAsync();
            return agents;
        }

        /// <inheritdoc/>
        public async Task DeleteSessionAsync(string sessionId) => await _coreRestClient.Sessions.DeleteSessionAsync(sessionId);
    }
}
