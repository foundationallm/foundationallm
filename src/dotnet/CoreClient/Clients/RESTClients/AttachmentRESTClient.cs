using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using System.ClientModel;
using System.Net.Http.Headers;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's Attachments endpoints.
    /// </summary>
    internal class AttachmentRESTClient : CoreRESTClientBase, IAttachmentRESTClient
    {
        private readonly string _instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentRESTClient"/> class with the specified HTTP client
        /// factory, token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public AttachmentRESTClient(
            IHttpClientFactory httpClientFactory,
            TokenCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentRESTClient"/> class with the specified HTTP client
        /// factory, agent access token credential, and FoundationaLLM instance identifier.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create HTTP client instances for making REST API calls.</param>
        /// <param name="credential">The agent access token credential used for authenticating requests to the REST API.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        public AttachmentRESTClient(
            IHttpClientFactory httpClientFactory,
            ApiKeyCredential credential,
            string instanceId) : base(httpClientFactory, credential) =>
            _instanceId = instanceId
                ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<string> UploadAttachmentAsync(Stream fileStream, string fileName, string contentType)
        {
            var coreClient = await GetCoreClientAsync();
            var content = new MultipartFormDataContent
            {
                { new StreamContent(fileStream)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue(contentType)
                    }
                }, "file", fileName }
            };

            var responseMessage = await coreClient.PostAsync($"instances/{_instanceId}/attachments/upload", content);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to upload attachment. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
            }

            return await responseMessage.Content.ReadAsStringAsync();
        }
    }
}
