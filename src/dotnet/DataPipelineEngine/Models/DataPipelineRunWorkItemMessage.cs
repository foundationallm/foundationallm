namespace FoundationaLLM.DataPipelineEngine.Models
{
    /// <summary>
    /// Represents a queue message for a data pipeline run work item.
    /// </summary>
    public class DataPipelineRunWorkItemMessage
    {
        /// <summary>
        /// Gets or set the identifier of the data pipeline run work item.
        /// </summary>
        public required string WorkItemId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run.
        /// </summary>
        public required string RunId { get; set; }
    }
}
