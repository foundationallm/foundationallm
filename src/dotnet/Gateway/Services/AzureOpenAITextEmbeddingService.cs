using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Gateway;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models;
using Microsoft.Extensions.Logging;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implementation of <see cref="ITextEmbeddingService"/> using Azure OpenAI.
    /// </summary>
    public class AzureOpenAITextEmbeddingService : ITextOperationService
    {
        private readonly string _accountEndpoint;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly ILogger<AzureOpenAITextEmbeddingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAITextEmbeddingService"/> class.
        /// </summary>
        /// <param name="accountEndpoint">The endpoint of the Azure OpenAI service.</param>
        /// <param name="logger"></param>
        public AzureOpenAITextEmbeddingService(
            string accountEndpoint,
            ILogger<AzureOpenAITextEmbeddingService> logger)
        {
            _accountEndpoint = accountEndpoint;
            _azureOpenAIClient = new AzureOpenAIClient(
                new Uri(_accountEndpoint),
                ServiceContext.AzureCredential,
                new AzureOpenAIClientOptions()
                {
                    NetworkTimeout = TimeSpan.FromSeconds(120),
                    RetryPolicy = new ClientRetryPolicy(1)
                });
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<InternalTextOperationResult> ExecuteTextOperation(
            InternalTextOperationRequest textOperationRequest)
        {
            if (!textOperationRequest.ModelParameters.TryGetValue(
                        TextOperationModelParameterNames.EmbeddingDimensions,
                        out object? embeddingDimensionsObject)
                    || embeddingDimensionsObject is not int embeddingDimensions)
                throw new GatewayException("The EmbeddingDimensions model parameter is missing.");

            try
            {
                var embeddingClient = _azureOpenAIClient.GetEmbeddingClient(
                    textOperationRequest.DeploymentName);
                OpenAI.Embeddings.EmbeddingGenerationOptions? embeddingOptions =
                    embeddingDimensions == -1
                        ? null
                        : new OpenAI.Embeddings.EmbeddingGenerationOptions
                            {
                                Dimensions = embeddingDimensions
                            };
                var result = await embeddingClient.GenerateEmbeddingsAsync(
                    textOperationRequest.TextChunks.Select(tc => tc.Content!).ToList(),
                    embeddingOptions);

                var rawResponse = result.GetRawResponse();
                rawResponse.LogRateLimitHeaders(textOperationRequest.Id, _logger, LogLevel.Debug);

                return new InternalTextOperationResult
                {
                    TextChunks = [.. Enumerable.Range(0, result.Value.Count).Select(i =>
                    {
                        var textChunk = textOperationRequest.TextChunks[i];
                        textChunk.Embedding = new Embedding(result.Value[i].ToFloats());
                        return textChunk;
                    })]
                };
            }
            catch (ClientResultException ex) when (ex.Status == 429)
            {
                _logger.LogWarning(ex, "Rate limit exceeded while generating embeddings.");

                var rawResponse = ex.GetRawResponse();
                if (rawResponse != null)
                    rawResponse.LogRateLimitHeaders(textOperationRequest.Id, _logger, LogLevel.Warning);
                else
                    _logger.LogWarning("Response headers were not available.");

                return new InternalTextOperationResult
                {
                    Failed = true,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating embeddings.");
                return new InternalTextOperationResult
                {
                    Failed = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
