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
        protected readonly int _actualTokenRateLimit =
            (int)(deployment.TokenRateLimit * tokenRateLimitMultiplier) / (60 / deployment.TokenRateRenewalPeriod);
        protected readonly int _actualRequestRateLimit =
            deployment.RequestRateLimit / (60 / deployment.RequestRateRenewalPeriod);

        protected readonly ITextOperationService _textOperationService = textOperationService;
        protected readonly ILoggerFactory _loggerFactory = loggerFactory;
        protected readonly ILogger<ModelDeploymentContext> _logger = loggerFactory.CreateLogger<ModelDeploymentContext>();
        protected readonly GatewayMetrics _metrics = metrics;

        protected readonly Dictionary<int, GatewayTextOperationRequest> _textOperationRequests = [];

        protected readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        /// <summary>
        /// The cummulated number of tokens for the current token rate window.
        /// </summary>
        protected int _tokenRateWindowTokenCount = 0;
        /// <summary>
        /// The cummulated number of requests for the current request rate window.
        /// </summary>
        protected int _requestRateWindowRequestCount = 0;
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
        /// <param name="textChunkContext">The context associated with the text chunk.</param>
        /// <remarks>
        /// <para>For embedding operations, the text chunk context must always be the number of dimensions required for embedding.</para>
        /// <para>For completion operations, the text chunk context must always be -1.</para>
        /// </remarks>
        /// <returns><see langword="true"/> if the text chunk can be added without breaching the token and reques rate limits.</returns>
        public bool TryAddInputTextChunk(TextChunk textChunk, int textChunkContext)
        {
            UpdateRateWindows();

            if (_tokenRateWindowTokenCount + textChunk.TokensCount > _actualTokenRateLimit)
                // Adding a new text chunk would push us over the token rate limit, so we need to refuse.
                return false;

            if (_requestRateWindowRequestCount == _actualRequestRateLimit)
                // We have already reached the allowed number of requests, so we need to refuse.
                return false;

            if (!_textOperationRequests.TryGetValue(textChunkContext, out GatewayTextOperationRequest? textOperationRequest))
            {
                if (_requestRateWindowRequestCount + 1 == _actualRequestRateLimit)
                    // Adding a new text chunk context would result in at least one additional request
                    // which would push us over the request rate limit, so we need to refuse.
                    return false;

                _textOperationRequests[textChunkContext] =
                    new GatewayTextOperationRequest
                    {
                        Id = Guid.NewGuid().ToString().ToLower(),
                        AccountName = _deployment.AccountEndpoint,
                        ModelName = _deployment.ModelName,
                        ModelVersion = _deployment.ModelVersion,
                        EmbeddingDimensions = textChunkContext,
                        TokenRateWindowStart = _tokenRateWindowStart,
                        RequestRateWindowStart = _requestRateWindowStart,
                        TextChunks = [textChunk]
                    };
            }
            else
            {
                textOperationRequest.TextChunks.Add(textChunk);
            }

            _tokenRateWindowTokenCount += textChunk.TokensCount;

            return true;
        }

        public async Task<List<OperationRequestResult>> ProcessTextOperationRequests()
        {
            try
            {
                var textOperationRequestsTasks = _textOperationRequests
                    .Select(async x => await ProcessTextOperationRequest(x.Value))
                    .ToArray();

                await Task.WhenAll(textOperationRequestsTasks);

                return [.. textOperationRequestsTasks.Select(t => t.Result)];
            }
            finally
            {
                _textOperationRequests.Clear();
            }
        }

        private async Task<OperationRequestResult> ProcessTextOperationRequest(
            GatewayTextOperationRequest textOperationRequest)
        {
            Interlocked.Increment(ref _requestRateWindowRequestCount);

            textOperationRequest.TokenRateWindowTokenCount = _tokenRateWindowTokenCount;
            textOperationRequest.RequestRateWindowRequestCount = _requestRateWindowRequestCount;

            _logger.LogInformation("Submitting text operation request with id {RequestId} and the following metrics: {RequestMetrics}",
                textOperationRequest.Id,
                JsonSerializer.Serialize(textOperationRequest, _jsonSerializerOptions));

            // Priority is false since the text operation context is already added to the queue.
            var textOperationResult =
                await _textOperationService.ExecuteTextOperation(
                    textOperationRequest.TextChunks,
                    _deployment.Name,
                    false,
                    _deployment.CanDoCompletions
                        ? -1 // For completion operations, we do not care about the embedding dimensions.
                        : _deployment.ModelName == "text-embedding-ada-002"
                            ? -1 // text-embedding-ada-002 does not support embedding dimensions.
                            : textOperationRequest.EmbeddingDimensions);

            var result = new OperationRequestResult
            {
                TextChunks = textOperationResult.TextChunks,
                Failed = textOperationResult.Failed,
                ErrorMessage = textOperationResult.ErrorMessage
            };

            if (textOperationResult.Failed)
            {
                _logger.LogWarning("The text operation request with id {RequestId} failed with the following error: {ErrorMessage}",
                    textOperationRequest.Id,
                    textOperationResult.ErrorMessage!);
                result.FailedOperationIds = [.. textOperationRequest.TextChunks
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

            return result;
        }

        private void UpdateRateWindows()
        {
            var refTime = DateTime.UtcNow;

            if ((refTime - _tokenRateWindowStart).TotalSeconds >= _deployment.TokenRateRenewalPeriod)
            {
                _tokenRateWindowStart = refTime;

                // Reset the rate window token count to the sum of token counts of all current embedding requests.
                _tokenRateWindowTokenCount = _textOperationRequests
                    .Sum(or => or.Value.TokensCount);
            }

            if ((refTime - _requestRateWindowStart).TotalSeconds >= _deployment.RequestRateRenewalPeriod)
            {
                _requestRateWindowStart = refTime;
                _requestRateWindowRequestCount = 0;
            }
        }
    }
}
