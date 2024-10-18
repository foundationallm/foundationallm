using System.Text.Json;
using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Settings;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    internal class StatusRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential,
        string instanceId) : CoreRESTClientBase(httpClientFactory, credential), IStatusRESTClient
    {
        private readonly string _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

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
