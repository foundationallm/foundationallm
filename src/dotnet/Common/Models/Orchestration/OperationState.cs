namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Represents the current state of a long running operation.
    /// </summary>
    public class OperationState
    {
        /// <summary>
        /// The identifier of the long running operation.
        /// </summary>
        public required string OperationId { get; set; }

        /// <summary>
        /// The status of the long running operation.
        /// </summary>
        public required OperationStatus Status { get; set; }

        /// <summary>
        /// The message describing the current state of the operation.
        /// </summary>
        public string? Message { get; set; }
    }
}
