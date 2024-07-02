using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using System.Text.Json;

namespace FoundationaLLM.Common.Services
{
    /// <inheritdoc/>
    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICallContext _callContext;
        private readonly IDownstreamAPISettings _apiSettings;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="resourceProviderServices">A list of of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>
        /// that allows access to <see cref="HttpClient"/> instances by name.</param>
        /// <param name="callContext">Stores a <see cref="UnifiedUserIdentity"/> object resolved from
        /// one or more services.</param>
        /// <param name="apiSettings">A <see cref="DownstreamAPISettings"/> class that
        /// contains the configured path to the desired API key.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IHttpClientFactory httpClientFactory,
            ICallContext callContext,
            IDownstreamAPISettings apiSettings)
        {
            _resourceProviderServices = resourceProviderServices.ToDictionary(rps => rps.Name);
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _callContext = callContext ?? throw new ArgumentNullException(nameof(callContext));
            _apiSettings = apiSettings ?? throw new ArgumentNullException(nameof(apiSettings));
        }

        /// <inheritdoc/>
        public async Task<HttpClient> CreateClient(string clientName)
        {
            var httpClient = _httpClientFactory.CreateClient();

            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Configuration, out var configurationResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");

            var apiEndpoint = await configurationResourceProvider.GetResource<APIEndpoint>(
                $"/{ConfigurationResourceTypeNames.APIEndpoints}/{clientName}",
                _callContext.CurrentUserIdentity!);

            httpClient.Timeout = _defaultTimeout;

            if (apiEndpoint != null)
            {
                // Override the default URL if a value is provided.
                httpClient.BaseAddress = new Uri(apiEndpoint.Url);

                var urlException = apiEndpoint.UrlExceptions.Where(x => x.UserPrincipalName == _callContext.CurrentUserIdentity!.UPN).SingleOrDefault();
                if (urlException != null)
                    httpClient.BaseAddress = new Uri(urlException.Url);

                // Override the default timeout if a value is provided.
                httpClient.Timeout = TimeSpan.FromSeconds(apiEndpoint.TimeoutSeconds);

                // Add the API key header.
                if (!string.IsNullOrWhiteSpace(apiEndpoint.APIKey))
                {
                    httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.APIKey, apiEndpoint.APIKey);
                }
            }

            // Optionally add the user identity header.
            if (_callContext.CurrentUserIdentity != null)
            {
                var serializedIdentity = JsonSerializer.Serialize(_callContext.CurrentUserIdentity);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.UserIdentity, serializedIdentity);
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
