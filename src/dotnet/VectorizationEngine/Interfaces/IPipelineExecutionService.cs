namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Provides services to interact with the pipeline execution service.
    /// </summary>
    public interface IPipelineExecutionService
    {
        /// <summary>
        /// Executes the pipeline execution service until cancellation is signaled.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the pipeline execution service, allowing it to cleanup.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
