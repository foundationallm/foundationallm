using Azure.Core;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Settings;
using System.ClientModel;
using System.Text.Json;

namespace FoundationaLLM.Client.Core.Clients.RESTClients
{
    internal class CoreRESTClientBase
    {
        /// <summary>
        /// Sets standard JSON serializer options.
        /// </summary>
        protected JsonSerializerOptions SerializerOptions { get; } =
            CommonJsonSerializerOptions.GetJsonSerializerOptions();
        private readonly TokenCredential? _tokenCredential;
        private readonly ApiKeyCredential? _apiKeyCredential;
        private readonly IHttpClientFactory _httpClientFactory;

        public CoreRESTClientBase(IHttpClientFactory httpClientFactory, TokenCredential tokenCredential)
        {
            _httpClientFactory = httpClientFactory;
            _tokenCredential = tokenCredential;
            _apiKeyCredential = null;
        }

        public CoreRESTClientBase(IHttpClientFactory httpClientFactory, ApiKeyCredential apiKeyCredential)
        {
            _httpClientFactory = httpClientFactory;
            _apiKeyCredential = apiKeyCredential;
            _tokenCredential = null;
        }

        /// <summary>
        /// Returns a new HttpClient configured with an authentication header that uses the supplied token.
        /// </summary>
        /// <returns></returns>
        protected async Task<HttpClient> GetCoreClientAsync()
        {
            var coreClient = _httpClientFactory.CreateClient(HttpClientNames.CoreAPI);

            if (_tokenCredential is not null)
            {
                var token = await _tokenCredential.GetTokenAsync(new TokenRequestContext([ScopeURIs.FoundationaLLM_Core]), default);
                coreClient.SetBearerToken(token.Token);
                return coreClient;
            }
            else if (_apiKeyCredential is not null)
            {
                _apiKeyCredential.Deconstruct(out string apiKey);
                coreClient.SetAgentAccessToken(apiKey);
                return coreClient;
            }
            else
            {
                throw new InvalidOperationException("No valid authentication credentials provided.");
            }
        }

        /// <summary>
        /// Returns a new HttpClient configured with a header that uses the supplied agent access token.
        /// </summary>
        /// <param name="agentAccessToken"></param>
        /// <returns></returns>
        protected HttpClient GetCoreClientAsync(string agentAccessToken)
        {
            var coreClient = _httpClientFactory.CreateClient(HttpClientNames.CoreAPI);
            coreClient.SetAgentAccessToken(agentAccessToken);
            return coreClient;
        }
    }
}
