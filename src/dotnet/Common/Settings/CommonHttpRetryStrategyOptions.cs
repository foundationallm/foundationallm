using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// Common Http retry resiliency strategy options used by the API classes and their libraries.
    /// </summary>
    public static class CommonHttpRetryStrategyOptions
    {
        /// <summary>
        /// Configures the commonly used Polly Http retry resiliency strategy options.
        /// </summary>
        /// <returns></returns>
        public static HttpRetryStrategyOptions GetCommonHttpRetryStrategyOptions() =>
            // See: https://www.pollydocs.org/strategies/retry.html
            new()
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(args is
                {
                    Outcome.Result.StatusCode:
                    //HttpStatusCode.RequestTimeout or
                    HttpStatusCode.TooManyRequests or
                    //HttpStatusCode.GatewayTimeout or
                    HttpStatusCode.BadGateway or
                    HttpStatusCode.ServiceUnavailable
                })
            };
    }
}
