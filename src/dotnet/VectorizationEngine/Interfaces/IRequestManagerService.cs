namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Manages vectorization requests.
    /// </summary>
    public interface IRequestManagerService
    {
        /// <summary>
        /// Runs the vectorization requests processing cycle.
        /// </summary>
        /// <returns>A <see cref="Task"/> to await the completion of the run.</returns>
        Task Run();
    }
}
