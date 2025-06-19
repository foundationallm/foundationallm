namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Represents the status of a work item in a data pipeline run.
    /// </summary>
    public class DataPipelineRunWorkItemStatus
    {
        private bool _completed;
        private bool _successful;

        /// <summary>
        /// The canonical identifier of the content item referenced by the work item.
        /// </summary>
        public required string ContentItemCanonicalId { get; set; }

        /// <summary>
        /// Gets or sets the indicator of whether the work item is completed.
        /// </summary>
        public bool Completed
        {
            get => _completed;
            set
            {
                if (_completed != value)
                {
                    _completed = value;
                    Changed = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the indicator of whether the work item was completed successfully.
        /// </summary>
        public bool Successful
        {
            get => _successful;
            set
            {
                if (_successful != value)
                {
                    _successful = value;
                    Changed = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the work item status has been modified.
        /// </summary>
        public bool Changed { get; set; }
    }
}
