namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Represents the status of a long-running operation.
    /// </summary>
    public enum OperationStatus
    {
        /// <summary>
        /// Operation is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Operation has been completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Operation has completed in a failed state
        /// </summary>
        Failed
    }
}
