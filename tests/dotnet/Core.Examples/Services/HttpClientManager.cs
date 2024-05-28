using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <inheritdoc/>
    public class HttpClientManager(
        IHttpClientFactory httpClientFactory,
        IOptionsSnapshot<HttpClientOptions> httpClientOptions) : IHttpClientManager
    {
        /// <inheritdoc/>
        public async Task<HttpClient> GetHttpClientAsync(string apiType)
        {
            var httpClient = httpClientFactory.CreateClient(apiType);
            var options = httpClientOptions.Get(apiType);

            var scope = options.Scope;
            if (scope == null) return httpClient;
            // The scope needs to just be the base URI, not the full URI.
            scope = scope[..scope.LastIndexOf('/')];

            var credentials = DefaultAuthentication.AzureCredential;
            if (credentials == null) return httpClient;
            var tokenResult = await credentials.GetTokenAsync(
                new([scope]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            httpClient.Timeout = TimeSpan.FromSeconds(900);

            return httpClient;
        }
    }
}
