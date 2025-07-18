using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Gateway;
using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods to call the Gateway API service.
    /// </summary>
    public class GatewayServiceClient
    {
        private readonly HttpClient _gatewayAPIHttpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the Gateway API service.
        /// </summary>
        /// <param name="gatewayAPIHttpClient">An <see cref="HttpClient"/> configured to call the Gateway API.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public GatewayServiceClient(
            HttpClient gatewayAPIHttpClient,
            ILogger logger)
        {
            _gatewayAPIHttpClient = gatewayAPIHttpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TextOperationResult> GetEmbeddingOperationResult(string instanceId, string operationId)
        {
            var fallback = new TextOperationResult
            {
                InProgress = false,
                OperationId = null
            };

            var response = await _gatewayAPIHttpClient.GetAsync($"instances/{instanceId}/embeddings?operationId={operationId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var embeddingResult = JsonSerializer.Deserialize<TextOperationResult>(responseContent);

                return embeddingResult ?? fallback;
            }

            return fallback;
        }

        /// <inheritdoc/>
        public async Task<TextOperationResult> GetCompletionOperationResult(string instanceId, string operationId)
        {
            var fallback = new TextOperationResult
            {
                InProgress = false,
                OperationId = null
            };

            var response = await _gatewayAPIHttpClient.GetAsync($"instances/{instanceId}/completions?operationId={operationId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var completionResult = JsonSerializer.Deserialize<TextOperationResult>(responseContent);

                return completionResult ?? fallback;
            }

            return fallback;
        }

        /// <inheritdoc/>
        public async Task<TextOperationResult> StartEmbeddingOperation(string instanceId, TextEmbeddingRequest embeddingRequest)
        {
            var fallback = new TextOperationResult
            {
                InProgress = false,
                OperationId = null
            };

            var serializedRequest = JsonSerializer.Serialize(embeddingRequest);
            var response = await _gatewayAPIHttpClient.PostAsync($"instances/{instanceId}/embeddings",
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var embeddingResult = JsonSerializer.Deserialize<TextOperationResult>(responseContent);

                return embeddingResult ?? fallback;
            }

            return fallback;
        }

        /// <inheritdoc/>
        public async Task<TextOperationResult> StartCompletionOperation(string instanceId, TextCompletionRequest completionRequest)
        {
            var fallback = new TextOperationResult
            {
                InProgress = false,
                OperationId = null
            };

            var serializedRequest = JsonSerializer.Serialize(completionRequest);
            var response = await _gatewayAPIHttpClient.PostAsync($"instances/{instanceId}/completions",
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var completionResult = JsonSerializer.Deserialize<TextOperationResult>(responseContent);

                return completionResult ?? fallback;
            }

            return fallback;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> CreateAgentCapability(string instanceId, string capabilityCategory, string capabilityName, Dictionary<string, object>? parameters = null)
        {
            var serializedRequest = JsonSerializer.Serialize(new AgentCapabilityRequest
            {
                CapabilityCategory = capabilityCategory,
                CapabilityName = capabilityName,
                Parameters = parameters ?? []
            });
            var response = await _gatewayAPIHttpClient.PostAsync($"instances/{instanceId}/agentcapabilities",
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

                if (responseObject == null)
                    throw new GatewayException("The Gateway API returned an invalid response.");

                return responseObject;
            }

            throw new GatewayException($"The Gateway API returned an error status code ({response.StatusCode}) while processing the agent capability request.");
        }
    }
}
