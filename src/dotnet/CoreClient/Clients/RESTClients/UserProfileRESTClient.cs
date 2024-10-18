using System.Text.Json;
using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's user profile endpoints.
    /// </summary>
    internal class UserProfileRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential,
        string instanceId) : CoreRESTClientBase(httpClientFactory, credential), IUserProfileRESTClient
    {
        private readonly string _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));

        /// <inheritdoc/>
        public async Task<IEnumerable<UserProfile>> GetUserProfilesAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync($"instances/{_instanceId}/userprofiles");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var userProfiles = JsonSerializer.Deserialize<IEnumerable<UserProfile>>(responseContent, SerializerOptions);
                return userProfiles ?? throw new InvalidOperationException("The returned user profiles are invalid.");
            }

            throw new Exception($"Failed to retrieve user profiles. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }
    }
}
