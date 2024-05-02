namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Provides the details of a long-running vectorization operation.
    /// </summary>
    public class VectorizationLongRunningOperation
    {
        /// <summary>
        /// The unique identifier of the long-running operation.
        /// </summary>
        public required string OperationId { get; set; }

        /// <summary>
        /// The first time we learned about the operation being run.
        /// </summary>
        public DateTime FirstResponseTime { get; set; }

        /// <summary>
        /// The last time we learned about the operation being run.
        /// </summary>
        public DateTime LastResponseTime { get; set; }

        /// <summary>
        /// Indicates whether the operation is complete or not.
        /// </summary>
        public bool Complete { get; set; }

        /// <summary>
        /// The number of times an attempt was made to retrieve the result.
        /// </summary>
        public int PollingCount { get; set; }
    }
}
