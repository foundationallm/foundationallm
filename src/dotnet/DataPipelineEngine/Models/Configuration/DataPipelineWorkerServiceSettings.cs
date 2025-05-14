namespace FoundationaLLM.DataPipelineEngine.Models.Configuration
{
    /// <summary>
    /// Provides the settings for the Data Pipeline Worker service.
    /// </summary>
    public class DataPipelineWorkerServiceSettings : DataPipelineServiceSettingsBase
    {
        /// <summary>
        /// Gets or sets the name of the queue used for processing data pipeline work items.
        /// </summary>
        public required string Queue { get; set; }

        /// <summary>
        /// Gets or sets the number of parallel processors to use for processing data pipeline run work items.
        /// </summary>
        public int ParallelProcessorsCount { get; set; } = 10;
    }
}
