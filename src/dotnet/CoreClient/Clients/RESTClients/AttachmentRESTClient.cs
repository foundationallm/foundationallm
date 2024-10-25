using System.Net.Http.Headers;
using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's Attachments endpoints.
    /// </summary>
    internal class AttachmentRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential,
        string instanceId) : CoreRESTClientBase(httpClientFactory, credential), IAttachmentRESTClient
    {
        private readonly string _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));

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
