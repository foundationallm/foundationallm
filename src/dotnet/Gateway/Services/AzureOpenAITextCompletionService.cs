using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Constants;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implementation of <see cref="ITextOperationService"/> using Azure OpenAI.
    /// </summary>
    public class AzureOpenAITextCompletionService : ITextOperationService
    {
        private readonly string _accountEndpoint;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        //private readonly ChatCompletionsClient _azureAIInferenceClient;
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
            //_azureAIInferenceClient = new ChatCompletionsClient(
            //    new Uri(_accountEndpoint),
            //    ServiceContext.AzureCredential,
            //    new AzureAIInferenceClientOptions()
            //    {
            //        RetryPolicy = new RetryPolicy(1)
            //    });
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
            if (textOperationRequest.TextChunks.Count != 1)
                throw new GatewayException("The AzureOpenAITextCompletionService only supports a single text chunk for completion operations.");

            try
            {
                var chatClient = _azureOpenAIClient.GetChatClient(
                    textOperationRequest.DeploymentName);

                var chatCompletionOptions = new ChatCompletionOptions();

                if (textOperationRequest.ModelParameters.TryGetValue(
                        TextOperationContextPropertyNames.MaxOutputTokenCount,
                        out object? maxOutputTokenCountObject)
                    && maxOutputTokenCountObject is JsonElement maxOutputTokenCount)
                    chatCompletionOptions.MaxOutputTokenCount = maxOutputTokenCount.GetInt32();

                if (textOperationRequest.ModelParameters.TryGetValue(
                        TextOperationContextPropertyNames.Temperature,
                        out object? temperatureObject)
                    && temperatureObject is JsonElement temperature)
                    chatCompletionOptions.Temperature = (float)temperature.GetDouble();

                if (textOperationRequest.ModelParameters.TryGetValue(
                        TextOperationContextPropertyNames.TopP,
                        out object? topPObject)
                    && topPObject is JsonElement topP)
                    chatCompletionOptions.TopP = (float)topP.GetDouble();

                var result = await chatClient.CompleteChatAsync(
                    [new UserChatMessage(textOperationRequest.TextChunks[0].Content)],
                    chatCompletionOptions);

                var rawResponse = result.GetRawResponse();
                rawResponse.LogRateLimitHeaders(textOperationRequest.Id, _logger, LogLevel.Debug);

                textOperationRequest.TextChunks[0].Completion = result.Value.Content[0].Text;
                return new InternalTextOperationResult
                {
                    TextChunks = [textOperationRequest.TextChunks[0]]
                };
            }
            catch (ClientResultException ex) when (ex.Status == 429)
            {
                _logger.LogWarning(ex, "Rate limit exceeded while generating completions.");

                var rawResponse = ex.GetRawResponse();
                if (rawResponse != null)
                {
                    rawResponse.LogRateLimitHeaders(textOperationRequest.Id, _logger, LogLevel.Warning);
                }
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
