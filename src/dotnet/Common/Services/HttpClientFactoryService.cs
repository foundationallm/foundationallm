using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Common.Services
{
    /// <inheritdoc/>
    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICallContext _callContext;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="resourceProviderServices">A list of of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>.</param>
        /// <param name="callContext">Stores a <see cref="UnifiedUserIdentity"/> object resolved from one or more services.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IHttpClientFactory httpClientFactory,
            ICallContext callContext)
        {
            _resourceProviderServices = resourceProviderServices.ToDictionary(rps => rps.Name);
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _callContext = callContext ?? throw new ArgumentNullException(nameof(callContext));
        }

        /// <inheritdoc/>
        public async Task<HttpClient> CreateClient(string clientName)
        {
            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Configuration, out var configurationResourceProvider))
                throw new Exception($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");

            var apiEndpoint = await configurationResourceProvider.GetResource<APIEndpoint>(
                $"/{ConfigurationResourceTypeNames.APIEndpoints}/{clientName}",
                _callContext.CurrentUserIdentity);

            if (apiEndpoint == null)
                throw new Exception($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} did not load the {clientName} endpoint settings.");

            var httpClient = _httpClientFactory.CreateClient(clientName);

            // Set the default timeout.
            httpClient.Timeout = TimeSpan.FromSeconds(apiEndpoint.TimeoutSeconds);

            // Set the default URL.
            httpClient.BaseAddress = new Uri(apiEndpoint.Url);

            // Override the default URL if an exception is provided.
            if (_callContext.CurrentUserIdentity != null)
            {
                var urlException = apiEndpoint.UrlExceptions.Where(x => x.UserPrincipalName == _callContext.CurrentUserIdentity.UPN).SingleOrDefault();
                if (urlException != null)
                    httpClient.BaseAddress = new Uri(urlException.Url);

                // Add the user identity header.
                var serializedIdentity = JsonSerializer.Serialize(_callContext.CurrentUserIdentity);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.UserIdentity, serializedIdentity);
            }

            // Add the API key header.
            if (apiEndpoint.AuthenticationType == AuthenticationTypes.APIKey
                && !string.IsNullOrWhiteSpace(apiEndpoint.APIKey))
            {
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.APIKey, apiEndpoint.APIKey);
            }

            // Add the authorization header.
            if (apiEndpoint.AuthenticationType == AuthenticationTypes.AzureIdentity
                && !string.IsNullOrWhiteSpace(apiEndpoint.Scope))
            {
                var credentials = DefaultAuthentication.AzureCredential;
                var tokenResult = await credentials!.GetTokenAsync(
                    new([apiEndpoint.Scope]),
                    default);

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            }

            return httpClient;
        }

        /// <inheritdoc/>
        public HttpClient CreateUnregisteredClient(TimeSpan? timeout = null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = timeout ?? _defaultTimeout;
            return httpClient;
        }
    }
}
