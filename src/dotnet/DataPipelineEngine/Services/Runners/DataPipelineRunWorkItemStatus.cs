namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Represents the status of a work item in a data pipeline run.
    /// </summary>
    public class DataPipelineRunWorkItemStatus
    {
        /// <summary>
        /// The canonical identifier of the content item referenced by the work item.
        /// </summary>
        public required string ContentItemCanonicalId { get; set; }

        /// <summary>
        /// Gets or sets the indicator of whether the work item is completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Gets or sets the indicator of whether the work item was completed successfully.
        /// </summary>
        public bool Successful { get; set; }
    }
}
