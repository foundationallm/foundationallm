using System.Text;
using System.Text.Json;
using Azure.Core;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Settings;
using Microsoft.Graph.Models;

namespace FoundationaLLM.Client.Management.Clients.RESTClients
{
    internal class IdentityRESTClient : ManagementRESTClientBase, IIdentityRESTClient
    {
        private readonly string _instanceId;
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        internal IdentityRESTClient(
            IHttpClientFactory httpClientFactory,
            TokenCredential credential,
            string instanceId)
            : base(httpClientFactory, credential) =>
            _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<IEnumerable<Group>> RetrieveGroupsAsync(ObjectQueryParameters parameters)
        {
            var managementClient = await GetManagementClientAsync();
            var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            var response = await managementClient.PostAsync($"instances/{_instanceId}/groups/retrieve", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Group>>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve groups. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<Group> GetGroupAsync(string groupId)
        {
            var managementClient = await GetManagementClientAsync();
            var response = await managementClient.GetAsync($"instances/{_instanceId}/groups/{groupId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Group>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve group. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> RetrieveUsersAsync(ObjectQueryParameters parameters)
        {
            var managementClient = await GetManagementClientAsync();
            var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            var response = await managementClient.PostAsync($"instances/{_instanceId}/users/retrieve", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<User>>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve users. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<User> GetUserAsync(string userId)
        {
            var managementClient = await GetManagementClientAsync();
            var response = await managementClient.GetAsync($"instances/{_instanceId}/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve user. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DirectoryObject>> RetrieveObjectsByIdsAsync(ObjectQueryParameters parameters)
        {
            var managementClient = await GetManagementClientAsync();
            var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            var response = await managementClient.PostAsync($"instances/{_instanceId}/objects/retrievebyids", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<DirectoryObject>>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve objects by IDs. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }
    }
}
