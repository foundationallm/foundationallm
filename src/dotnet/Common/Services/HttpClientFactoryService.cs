using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.API;
using System.Text.Json;

namespace FoundationaLLM.Common.Services
{
    /// <inheritdoc/>
    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICallContext _callContext;
        private readonly IDownstreamAPISettings _apiSettings;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>
        /// that allows access to <see cref="HttpClient"/> instances by name.</param>
        /// <param name="callContext">Stores a <see cref="UnifiedUserIdentity"/> object resolved from
        /// one or more services.</param>
        /// <param name="apiSettings">A <see cref="DownstreamAPISettings"/> class that
        /// contains the configured path to the desired API key.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(IHttpClientFactory httpClientFactory,
            ICallContext callContext,
            IDownstreamAPISettings apiSettings)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _callContext = callContext ?? throw new ArgumentNullException(nameof(callContext));
            _apiSettings = apiSettings ?? throw new ArgumentNullException(nameof(apiSettings));
        }

        /// <inheritdoc/>
        public HttpClient CreateClient(string clientName)
        {
            var httpClient = _httpClientFactory.CreateClient(clientName);
            _apiSettings.DownstreamAPIs.TryGetValue(clientName, out var settings);
            httpClient.Timeout = _defaultTimeout;

            if (settings != null)
            {
                // Override the default timeout if a value is provided.
                if (settings.Timeout.HasValue)
                {
                    httpClient.Timeout = settings.Timeout.Value;
                }

                // Add the API key header.
                if (!string.IsNullOrWhiteSpace(settings.APIKey))
                {
                    httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.APIKey, settings.APIKey);
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
