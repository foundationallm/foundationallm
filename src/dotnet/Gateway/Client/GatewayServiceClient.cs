using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Gateway.Client
{
    /// <summary>
    /// Provides methods to call the Gateway API service.
    /// </summary>
    public class GatewayServiceClient : IGatewayServiceClient
    {
        private readonly GatewayServiceSettings _settings;
        private readonly ICallContext _callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly ILogger<GatewayServiceClient> _logger;

        /// <summary>
        /// Creates a new instance of the Gateway API service.
        /// </summary>
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/> used to create the HTTP client.</param>
        /// <param name="options">The options object containing the <see cref="GatewayServiceSettings"/> object with the setting.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public GatewayServiceClient(
            ICallContext callContext,
            IHttpClientFactoryService httpClientFactoryService,
            IOptions<GatewayServiceSettings> options,
            ILogger<GatewayServiceClient> logger)
        {
            _callContext = callContext;
            _httpClientFactoryService = httpClientFactoryService;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<TextEmbeddingResult> GetEmbeddingOperationResult(string operationId)
        {
            var fallback = new TextEmbeddingResult
            {
                InProgress = false,
                OperationId = null
            };

            var client = await _httpClientFactoryService.CreateClient(HttpClients.GatewayAPI, _callContext.CurrentUserIdentity);
            var response = await client.GetAsync($"embeddings?operationId={operationId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var embeddingResult = JsonSerializer.Deserialize<TextEmbeddingResult>(responseContent);

                return embeddingResult ?? fallback;
            }

            return fallback;
        }

        public async Task<TextEmbeddingResult> StartEmbeddingOperation(TextEmbeddingRequest embeddingRequest)
        {
            var fallback = new TextEmbeddingResult
            {
                InProgress = false,
                OperationId = null
            };

            var client = await _httpClientFactoryService.CreateClient(HttpClients.GatewayAPI, _callContext.CurrentUserIdentity);
            var serializedRequest = JsonSerializer.Serialize(embeddingRequest);
            var response = await client.PostAsync("embeddings",
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var embeddingResult = JsonSerializer.Deserialize<TextEmbeddingResult>(responseContent);

                return embeddingResult ?? fallback;
            }

            return fallback;
        }
    }
}
