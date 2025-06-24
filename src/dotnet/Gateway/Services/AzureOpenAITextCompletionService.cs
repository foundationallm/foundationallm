using Azure.AI.Inference;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using Microsoft.Extensions.Logging;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implementation of <see cref="ITextOperationService"/> using Azure OpenAI.
    /// </summary>
    public class AzureOpenAITextCompletionService : ITextOperationService
    {
        private readonly string _accountEndpoint;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly ChatCompletionsClient _azureAIInferenceClient;
        private readonly ILogger<AzureOpenAITextCompletionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAITextCompletionService"/> class.
        /// </summary>
        /// <param name="accountEndpoint">The endpoint of the Azure OpenAI service.</param>
        /// <param name="logger"></param>
        public AzureOpenAITextCompletionService(
            string accountEndpoint,
            ILogger<AzureOpenAITextCompletionService> logger)
        {
            _accountEndpoint = accountEndpoint;
            _azureAIInferenceClient = new ChatCompletionsClient(
                new Uri(_accountEndpoint),
                ServiceContext.AzureCredential,
                new AzureAIInferenceClientOptions()
                {
                    RetryPolicy = new RetryPolicy(1)
                });
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
        public async Task<TextOperationResult> ExecuteTextOperation(
            IList<TextChunk> textChunks,
            string deploymentName,
            bool prioritized,
            params object[] additionalParameters)
        {
            additionalParameters ??= [];

            if (additionalParameters.Length > 0)
                throw new GatewayException("The AzureOpenAITextCompletionService does not support additional parameters in the ExecuteTextOperation method.");

            return await GetCompletionsAsync(textChunks, deploymentName);
        }

        /// <inheritdoc/>
        private async Task<TextOperationResult> GetCompletionsAsync(IList<TextChunk> textChunks, string deploymentName)
        {
            // Priority not relevant as the text embedding context has already been queued at a higher level.
            try
            {
                var chatClient = _azureOpenAIClient.GetChatClient(deploymentName);
                var result = await chatClient.CompleteChatAsync.CompleteChat().GenerateEmbeddingsAsync(
                    textChunks.Select(tc => tc.Content!).ToList(),
                    embeddingOptions);

                var rawResponse = result.GetRawResponse();
                rawResponse.Headers.TryGetValue("x-ratelimit-limit-tokens", out var limitTokens);
                rawResponse.Headers.TryGetValue("x-ratelimit-remaining-tokens", out var remainingTokens);
                _logger.LogInformation("Rate limit tokens: {LimitTokens} - Remaining tokens: {RemainingTokens}",
                    string.IsNullOrWhiteSpace(limitTokens) ? "N/A" : limitTokens,
                    string.IsNullOrWhiteSpace(remainingTokens) ? "N/A" : remainingTokens);

                return new TextOperationResult
                {
                    InProgress = false,
                    TextChunks = [.. Enumerable.Range(0, result.Value.Count).Select(i =>
                    {
                        var textChunk = textChunks[i];
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
                {
                    rawResponse.Headers.TryGetValue("x-ratelimit-limit-tokens", out var limitTokens);
                    rawResponse.Headers.TryGetValue("x-ratelimit-remaining-tokens", out var remainingTokens);
                    _logger.LogInformation("Rate limit tokens: {LimitTokens} - Remaining tokens: {RemainingTokens}",
                        string.IsNullOrWhiteSpace(limitTokens) ? "N/A" : limitTokens,
                        string.IsNullOrWhiteSpace(remainingTokens) ? "N/A" : remainingTokens);
                }
                else
                    _logger.LogWarning("Response headers were not available.");

                return new TextOperationResult
                {
                    InProgress = false,
                    Failed = true,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating embeddings.");
                return new TextOperationResult
                {
                    InProgress = false,
                    Failed = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
