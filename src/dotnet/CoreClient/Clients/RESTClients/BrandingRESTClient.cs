using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Branding;
using System.ClientModel;
using System.Text.Json;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's Branding endpoints.
    /// </summary>
    internal class BrandingRESTClient : CoreRESTClientBase, IBrandingRESTClient
    {
        private readonly string _instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrandingRESTClient"/> class with the specified HTTP client
        /// factory, token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public BrandingRESTClient(
            IHttpClientFactory httpClientFactory,
            TokenCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <summary>
        /// Initializes a new instance of the <see cref="BrandingRESTClient"/> class with the specified HTTP client
        /// factory, agent access token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The agent access token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public BrandingRESTClient(
            IHttpClientFactory httpClientFactory,
            ApiKeyCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<ClientBrandingConfiguration> GetBrandingAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync($"instances/{_instanceId}/branding");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var branding = JsonSerializer.Deserialize<ClientBrandingConfiguration>(responseContent, SerializerOptions);
                return branding ?? throw new InvalidOperationException("The returned branding information is invalid.");
            }

            throw new Exception($"Failed to retrieve branding information. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }
    }
}
