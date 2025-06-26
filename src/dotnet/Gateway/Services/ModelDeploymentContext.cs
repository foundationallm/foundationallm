using FoundationaLLM.Common.Constants.Gateway;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Provides context associated with model deployment used in text operations (embeddings or completions).
    /// </summary>
    /// <param name="deployment">The <see cref="AzureOpenAIAccountDeployment"/> object with the details of the model deployment.</param>
    /// <param name="tokenRateLimitMultiplier">The token rate limit multiplier used to account for the tokenization differences
    /// between the Gateway API and the deployed model.</param>
    /// <param name="textOperationService">The service providing the implementation of the text operation.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    /// <param name="metrics">The FoundationaLLM Gateway telemetry metrics.</param>
    public class ModelDeploymentContext(
        AzureOpenAIAccountDeployment deployment,
        double tokenRateLimitMultiplier,
        ITextOperationService textOperationService,
        ILoggerFactory loggerFactory,
        GatewayMetrics metrics)
    {
        protected readonly AzureOpenAIAccountDeployment _deployment = deployment;
        protected readonly double _tokenRateLimitMultiplier = tokenRateLimitMultiplier;
        protected readonly int _effectiveTokenRateLimit =
            (int)(deployment.TokenRateLimit * tokenRateLimitMultiplier) / (60 / deployment.TokenRateRenewalPeriod);
        protected readonly int _effectiveRequestRateLimit =
            deployment.RequestRateLimit / (60 / deployment.RequestRateRenewalPeriod);

        protected readonly ITextOperationService _textOperationService = textOperationService;
        protected readonly ILoggerFactory _loggerFactory = loggerFactory;
        protected readonly ILogger<ModelDeploymentContext> _logger = loggerFactory.CreateLogger<ModelDeploymentContext>();
        protected readonly GatewayMetrics _metrics = metrics;

        protected readonly List<InternalTextOperationRequest> _textOperationRequests = [];
        /// <summary>
        /// Embedding operations are grouped by the number of dimensions they require.
        /// For each embedding dimension, we send a single request to the model.
        /// This dictionary maps the number of dimensions to the index in the <see cref="_textOperationRequests"/> list.
        /// </summary>
        protected readonly Dictionary<int, int> _embeddingDimensionsIndexMapping = [];

        protected readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
        private readonly SemaphoreSlim _operationsSemaphore = new(1, 1);

        /// <summary>
        /// The actual cummulated number of tokens for the current token rate window.
        /// </summary>
        protected int _tokenRateWindowActualTokenCount = 0;
        /// <summary>
        /// The projects cummulated number of tokens for the current token rate window.
        /// </summary>
        protected int _tokenRateWindowProjectedTokenCount = 0;
        /// <summary>
        /// The actual cummulated number of requests for the current request rate window.
        /// </summary>
        protected int _requestRateWindowActualRequestCount = 0;
        /// <summary>
        /// The projected cummulated number of requests for the current request rate window.
        /// </summary>
        protected int _requestRateWindowProjectedRequestCount = 0;
        /// <summary>
        /// The start timestamp of the current token rate window.
        /// </summary>
        protected DateTime _tokenRateWindowStart = DateTime.MinValue;
        /// <summary>
        /// The start timestamp of the current request rate window.
        /// </summary>
        protected DateTime _requestRateWindowStart = DateTime.MinValue;

        /// <summary>
        /// Indicates whether the model in the deployment can perform embeddings.
        /// </summary>
        public bool ModelCanDoEmbeddings =>
            _deployment.CanDoEmbeddings;

        /// <summary>
        /// Indicates whether the model in the deployment can perform completions.
        /// </summary>
        public bool ModelCanDoCompletions =>
            _deployment.CanDoCompletions;

        public bool HasInput =>
            _textOperationRequests.Count > 0;

        /// <summary>
        /// Attempts to add a new text chunk to the input for the text operation request.
        /// </summary>
        /// <param name="textChunk">The text chunk to be added.</param>
        /// <param name="modelParameters">The model parameters for the text operation.</param>
        /// <remarks>
        /// <para>For embedding operations, <paramref name="modelParameters"/> must always contain a single property named
        /// <see cref="TextOperationContextPropertyNames.EmbeddingDimensions"/> which specifies the number of dimensions required for embedding.</para>
        /// <para>For completion operations, <paramref name="modelParameters"/> can contain the following parameters:
        /// <list type="number">
        /// <item><see cref="TextOperationContextPropertyNames.Temperature"/> - the completion model temperature.</item>
        /// <item><see cref="TextOperationContextPropertyNames.TopP"/> - the completion model top-p value.</item>
        /// <item><see cref="TextOperationContextPropertyNames.MaxOutputTokenCount"/> - the completion model max output token count.</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <returns><see langword="true"/> if the text chunk can be added without breaching the token and reques rate limits.</returns>
        public bool TryAddInputTextChunk(
            TextChunk textChunk,
            Dictionary<string, object> modelParameters)
        {
            UpdateRateWindows();

            if (_tokenRateWindowProjectedTokenCount + textChunk.TokensCount > _effectiveTokenRateLimit)
                // Adding a new text chunk would push us over the token rate limit, so we need to refuse.
                return false;

            if (_deployment.CanDoCompletions)
            {
                // For completion operations, every new text chunk is a new request,
                // so we need to automatically check the request rate limit.
                // Each entry in the list of text operation requests represents a single request to the model,
                // processing a single text chunk.

                if (_requestRateWindowProjectedRequestCount == _effectiveRequestRateLimit)
                    // We have already reached the allowed number of requests, so we need to refuse.
                    return false;

                _textOperationRequests.Add(new InternalTextOperationRequest
                    {
                        Id = _textOperationRequests.Count + 1,
                        AccountName = _deployment.AccountEndpoint,
                        DeploymentName = _deployment.Name,
                        ModelName = _deployment.ModelName,
                        ModelVersion = _deployment.ModelVersion,
                        ModelParameters = modelParameters,
                        TextChunks = [textChunk]
                    });

                _requestRateWindowProjectedRequestCount += 1;
            }
            else
            {
                if (!modelParameters.TryGetValue(
                        TextOperationModelParameterNames.EmbeddingDimensions,
                        out object? embeddingDimensionsObject)
                    || embeddingDimensionsObject is not int embeddingDimensions)
                    throw new GatewayException("The EmbeddingDimensions model parameter is missing.");

                // For embedding operations, we group text chunks by the number of embedding dimensions.
                // Each entry in the list of text operation requests represents a single request to the model,
                // processing a list of text chunks with the same number of embedding dimensions.
                // Each entry corresponds to a single embedding dimensions value and the _embeddingDimensionsIndexMapping dictionary
                // maps the embedding dimensions to the index in the _textOperationRequests list.

                    if (!_embeddingDimensionsIndexMapping.TryGetValue(embeddingDimensions, out int index))
                {
                    // We are about to add a new request, so we need to first check the request rate limit.
                    if (_requestRateWindowProjectedRequestCount == _effectiveRequestRateLimit)
                        // We have already reached the allowed number of requests, so we need to refuse.
                        return false;

                    // We are adding a new request for the given embedding dimensions,
                    // so we need to create a new entry in the _embeddingDimensionsIndexMapping dictionary.
                    index = _textOperationRequests.Count;
                    _embeddingDimensionsIndexMapping[embeddingDimensions] = index;

                    _textOperationRequests.Add(new InternalTextOperationRequest
                        {
                            Id = _textOperationRequests.Count + 1,
                            AccountName = _deployment.AccountEndpoint,
                            DeploymentName = _deployment.Name,
                            ModelName = _deployment.ModelName,
                            ModelVersion = _deployment.ModelVersion,
                            ModelParameters = modelParameters,
                        TextChunks = [textChunk]
                        });

                    // Update the request rate window projected request count.
                    _requestRateWindowProjectedRequestCount += 1;
                }
                else
                {
                    // We already have a request for the same embedding dimensions,
                    // so we can reuse the existing request.
                    // No need to check the request rate limit here, as we are not adding a new request.
                    _textOperationRequests[index].TextChunks.Add(textChunk);
                }
            }

            // Regardless of the cases above, we need to update the token rate window projected tokens count.
            _tokenRateWindowProjectedTokenCount += textChunk.TokensCount;

            return true;
        }

        public async Task<List<InternalTextOperationResult>> ProcessTextOperationRequests()
        {
            try
            {
                var textOperationRequestsTasks = _textOperationRequests
                    .Select(async x => await ProcessTextOperationRequest(x))
                    .ToArray();

                await Task.WhenAll(textOperationRequestsTasks);

                return [.. textOperationRequestsTasks.Select(t => t.Result)];
            }
            finally
            {
                // Clear the text operation requests and the embedding dimensions index mapping
                _textOperationRequests.Clear();
                _embeddingDimensionsIndexMapping.Clear();

                if (_tokenRateWindowActualTokenCount != _tokenRateWindowProjectedTokenCount)
                    _logger.LogWarning("The actual token rate window count ({ActualCount}) does not match the projected count ({ProjectedCount}). " +
                        "This may indicate that some text operation requests were not processed.",
                        _tokenRateWindowActualTokenCount, _tokenRateWindowProjectedTokenCount);

                if (_requestRateWindowActualRequestCount != _requestRateWindowProjectedRequestCount)
                    _logger.LogWarning("The actual request rate window count ({ActualCount}) does not match the projected count ({ProjectedCount}). " +
                        "This may indicate that some text operation requests were not processed.",
                        _requestRateWindowActualRequestCount, _requestRateWindowProjectedRequestCount);
            }
        }

        private async Task<InternalTextOperationResult> ProcessTextOperationRequest(
            InternalTextOperationRequest textOperationRequest)
        {
            var (RequestsCount, TokensCount) = await UpdateRateWindowsActuals(
                1,
                textOperationRequest.TokensCount);

            _logger.LogInformation("Text operation request id: {RequestId}, actual requests count: {RequestsCount}, actual tokens count: {TokensCount}.",
                textOperationRequest.Id, RequestsCount, TokensCount);
            _logger.LogDebug("Text operation details: {TextOperationRequest}",
                JsonSerializer.Serialize(textOperationRequest, _jsonSerializerOptions));

            var textOperationResult =
                await _textOperationService.ExecuteTextOperation(textOperationRequest);

            if (textOperationResult.Failed)
            {
                _logger.LogWarning("The text operation request with id {RequestId} failed with the following error: {ErrorMessage}",
                    textOperationRequest.Id,
                    textOperationResult.ErrorMessage!);
                textOperationResult.FailedOperationIds = [.. textOperationRequest.TextChunks
                    .Select(tc => tc.OperationId!)
                    .Distinct()];
            }
            else
            {
                if (_deployment.CanDoCompletions)
                {
                    _metrics.IncrementTextChunkMeters(0, textOperationRequest.TextChunksCount);
                    _metrics.IncrementTextChunksSizeTokens(0, textOperationRequest.TokensCount);
                }
                else
                {
                    _metrics.IncrementTextChunkMeters(textOperationRequest.TextChunksCount, 0);
                    _metrics.IncrementTextChunksSizeTokens(textOperationRequest.TokensCount, 0);
                }
            }

            return textOperationResult;
        }

        private async Task<(int RequestsCount, int TokensCount)> UpdateRateWindowsActuals(
            int requestsCount,
            int tokensCount)
        {
            await _operationsSemaphore.WaitAsync();
            try
            {
                _requestRateWindowActualRequestCount += requestsCount;
                _tokenRateWindowActualTokenCount += tokensCount;

                return (_requestRateWindowActualRequestCount, _tokenRateWindowActualTokenCount);
            }
            finally
            {
                _operationsSemaphore.Release();
            }
        }

        private void UpdateRateWindows()
        {
            var refTime = DateTime.UtcNow;

            if ((refTime - _tokenRateWindowStart).TotalSeconds >= _deployment.TokenRateRenewalPeriod)
            {
                _tokenRateWindowStart = refTime;

                _tokenRateWindowProjectedTokenCount =
                    _textOperationRequests.Sum(or => or.TokensCount);
                _tokenRateWindowActualTokenCount = 0;
            }

            if ((refTime - _requestRateWindowStart).TotalSeconds >= _deployment.RequestRateRenewalPeriod)
            {
                _requestRateWindowStart = refTime;

                _requestRateWindowProjectedRequestCount =
                    _textOperationRequests.Count;
                _requestRateWindowActualRequestCount = 0;
            }
        }
    }
}
