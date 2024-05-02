using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Gateway.Interfaces
{
    /// <summary>
    /// Defines the interface of the FoundationaLLM Gateway core.
    /// </summary>
    public interface IGatewayCore : IGatewayServiceClient
    {
        /// <summary>
        /// Starts the Gateway service, allowing it to initialize.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the Gateway service, allowing it to cleanup.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the Gateway service until cancellation is signaled.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
