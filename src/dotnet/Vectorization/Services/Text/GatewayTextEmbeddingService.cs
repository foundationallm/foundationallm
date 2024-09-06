using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Generates text embeddings by routing requests through the FoundationaLLM Gateway API.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="httpClientFactoryService">The HTTP Client Factory service.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class GatewayTextEmbeddingService(
        IOptions<InstanceSettings> instanceOptions,
        IHttpClientFactoryService httpClientFactoryService,
        ILoggerFactory loggerFactory) : ITextEmbeddingService
    {
        private readonly InstanceSettings _instanceSettings = instanceOptions.Value;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingsAsync(IList<TextChunk> textChunks, string modelName, bool Prioritized)
        {
            var gatewayClient = await GetGatewayServiceClientAsync();
            return await gatewayClient.StartEmbeddingOperation(_instanceSettings.Id, new TextEmbeddingRequest
            {
                EmbeddingModelName = modelName,
                TextChunks = textChunks,
                Prioritized = Prioritized

            });
        }          

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingsAsync(string operationId)
        {
            var gatewayClient = await GetGatewayServiceClientAsync();
            return await gatewayClient.GetEmbeddingOperationResult(_instanceSettings.Id, operationId);
        }          

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayServiceClient"/> class.
        /// </summary>
        /// <returns>New instance of a GatewayServiceClient.</returns>
        private async Task<GatewayServiceClient> GetGatewayServiceClientAsync()
        {
            var httpClient = await _httpClientFactoryService.CreateClient(
                                HttpClientNames.GatewayAPI,
                                DefaultAuthentication.ServiceIdentity!);
            var logger = _loggerFactory.CreateLogger<GatewayServiceClient>();
            var gatewayClient = new GatewayServiceClient(httpClient,logger);
            return gatewayClient;
        }
    }
}
