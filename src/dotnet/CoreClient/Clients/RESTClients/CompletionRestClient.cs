using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using System.ClientModel;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's orchestration endpoints.
    /// </summary>
    internal class CompletionRESTClient : CoreRESTClientBase, ICompletionRESTClient
    {
        private readonly string _instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionRESTClient"/> class with the specified HTTP client
        /// factory, token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public CompletionRESTClient(
            IHttpClientFactory httpClientFactory,
            TokenCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionRESTClient"/> class with the specified HTTP client
        /// factory, agent access token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The agent access token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public CompletionRESTClient(
            IHttpClientFactory httpClientFactory,
            ApiKeyCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<Message> GetChatCompletionAsync(CompletionRequest completionRequest)
        {
            var coreClient = await GetCoreClientAsync();
            var serializedRequest = JsonSerializer.Serialize(completionRequest, SerializerOptions);

            var responseMessage = await coreClient.PostAsync($"instances/{_instanceId}/completions",
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse =
                    JsonSerializer.Deserialize<Message>(responseContent, SerializerOptions);
                return completionResponse ?? throw new InvalidOperationException("The returned completion response is invalid.");
            }

            throw new Exception($"Failed to send completion request. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync($"instances/{_instanceId}/completions/agents");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var agents = JsonSerializer.Deserialize<IEnumerable<ResourceProviderGetResult<AgentBase>>>(responseContent, SerializerOptions);
                return agents ?? throw new InvalidOperationException("The returned agents are invalid.");
            }

            throw new Exception($"Failed to retrieve agents. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }
    }
}
