namespace FoundationaLLM.DataPipelineEngine.Models
{
    /// <summary>
    /// Represents a dequeued message from a data pipeline worker queue.
    /// </summary>
    public class DequeuedMessage
    {
        /// <summary>
        /// Gets or sets the data pipeline run work item queue message.
        /// </summary>
        public required DataPipelineRunWorkItemMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the queue message identifier.
        /// </summary>
        public required string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the queue pop receipt.
        /// </summary>
        public required string PopReceipt { get; set; }

        /// <summary>
        /// Gets or sets the number of times the message has been dequeued.
        /// </summary>
        public long DequeueCount { get; set; }
    }
}
