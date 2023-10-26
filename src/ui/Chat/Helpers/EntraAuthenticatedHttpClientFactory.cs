using FoundationaLLM.Common.Interfaces;
using Microsoft.Identity.Abstractions;

namespace FoundationaLLM.Chat.Helpers
{
    /// <summary>
    /// A helper class for producing authenticated HttpClient instances utilizing Entra authentication.
    /// </summary>
    public class EntraAuthenticatedHttpClientFactory : IAuthenticatedHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public EntraAuthenticatedHttpClientFactory(
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance from <see cref="IHttpClientFactory"/> with an
        /// authorization header for the current user.
        /// </summary>
        /// <param name="clientName">The named <see cref="HttpClient"/> client configuration.</param>
        /// <param name="scopes">List of permissions to request from the service.</param>
        /// <returns></returns>
        public async Task<HttpClient> CreateClientAsync(string clientName, string scopes)
        {
            var client = _httpClientFactory.CreateClient(clientName);
            using var scope = _serviceProvider.CreateScope();
            var authorizationHeaderProvider = scope.ServiceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
            string accessToken = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(new[] { scopes });
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            return client;
        }
    }
}
