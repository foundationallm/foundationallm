using Polly;
using Polly.Retry;

namespace FoundationaLLM.Common.Utils
{
    /// <summary>
    /// Provides helper methods for implementing retry logic in applications.
    /// </summary>
    /// <remarks>This class typically contains static methods that assist with executing operations that may
    /// need to be retried due to transient failures, such as network errors or temporary unavailability of resources.
    /// It is intended to be used as a utility and cannot be instantiated.</remarks>
    public static class RetryHelper
    {
        /// <summary>
        /// Gets the default resilience pipeline configured with a retry strategy.
        /// </summary>
        /// <remarks>The default pipeline is configured to retry failed operations up to five times with a
        /// one-second delay between attempts. This pipeline can be used as a starting point for common resilience
        /// scenarios or as a baseline for further customization.</remarks>
        public static readonly ResiliencePipeline DefaultResiliencePipeline =
            new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromSeconds(1)
                })
                .Build();

        /// <summary>
        /// Executes the specified asynchronous action using a default resilience pipeline that automatically retries on
        /// transient failures.
        /// </summary>
        /// <param name="action">A delegate that represents the asynchronous operation to execute. The delegate should return a task that
        /// completes when the operation is finished.</param>
        /// <returns>A ValueTask that represents the asynchronous execution of the action, including any retries performed by the
        /// resilience pipeline.</returns>
        public static ValueTask ExecuteWithRetryAsync(Func<Task> action) =>
            DefaultResiliencePipeline.ExecuteAsync(
                async (cancellationToken) =>
                {
                    await action();
                    return;
                });
    }
}
