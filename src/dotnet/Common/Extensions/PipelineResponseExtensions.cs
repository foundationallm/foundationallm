using Microsoft.Extensions.Logging;
using System.ClientModel.Primitives;

namespace FoundationaLLM.Common.Extensions
{
    /// This file contains extension methods for the PipelineResponse class.
    public static class PipelineResponseExtensions
    {
        /// <summary>
        /// Logs the rate limit headers from the response at the specified log level.
        /// </summary>
        /// <param name="response">The <see cref="PipelineResponse"/> whose headers are being logged.</param>
        /// <param name="operationId"> The operation identifier associated with the response, used for logging.</param>
        /// <param name="logger">The logger used for logging.</param>
        /// <param name="logLevel"> The log level at which to log the rate limit headers. Defaults to Debug.</param>
        public static void LogRateLimitHeaders(
            this PipelineResponse response,
            int operationId,
            ILogger logger,
            LogLevel logLevel = LogLevel.Debug)
        {
            var (LimitTokens, RemainingTokens, LimitRequests, RemainingRequests) = GetRateLimitHeaders(response);

            const string messageTemplate = "Rate limits for operation id {OperationId}: {RemainingTokens} of {LimitTokens} tokens, {RemainingRequests} of {LimitRequests} requests.";

            logger.Log(
                logLevel,
                messageTemplate,
                operationId,
                ToDisplayString(RemainingTokens),
                ToDisplayString(LimitTokens),
                ToDisplayString(RemainingRequests),
                ToDisplayString(LimitRequests));
        }

        private static (int LimitTokens, int RemainingTokens, int LimitRequests, int RemainingRequests) GetRateLimitHeaders(
            PipelineResponse response)
        {
            response.Headers.TryGetValue("x-ratelimit-limit-tokens", out var limitTokens);
            response.Headers.TryGetValue("x-ratelimit-remaining-tokens", out var remainingTokens);
            response.Headers.TryGetValue("x-ratelimit-limit-requests", out var limitRequests);
            response.Headers.TryGetValue("x-ratelimit-remaining-requests", out var remainingRequests);
            return (
                int.TryParse(limitTokens, out var limit) ? limit : -1,
                int.TryParse(remainingTokens, out var remaining) ? remaining : -1,
                int.TryParse(limitRequests, out var reqLimit) ? reqLimit : -1,
                int.TryParse(remainingRequests, out var reqRemaining) ? reqRemaining : -1
            );
        }

        private static string ToDisplayString(int value) => value == -1 ? "N/A" : value.ToString();
    }
}
