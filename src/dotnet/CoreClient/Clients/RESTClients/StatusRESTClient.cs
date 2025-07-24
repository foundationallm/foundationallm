using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Settings;
using System.ClientModel;
using System.Text.Json;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    internal class StatusRESTClient : CoreRESTClientBase, IStatusRESTClient
    {
        private readonly string _instanceId;
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusRESTClient"/> class with the specified HTTP client
        /// factory, token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public StatusRESTClient(
            IHttpClientFactory httpClientFactory,
            TokenCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusRESTClient"/> class with the specified HTTP client
        /// factory, agent access token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The agent access token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public StatusRESTClient(
            IHttpClientFactory httpClientFactory,
            ApiKeyCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetServiceStatusAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var response = await coreClient.GetAsync($"instances/{_instanceId}/status");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve service status. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var response = await coreClient.GetAsync($"instances/{_instanceId}/status/auth");

            return response.IsSuccessStatusCode;
        }
    }
}
