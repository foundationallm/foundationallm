namespace FoundationaLLM.DataPipelineEngine.Models.Configuration
{
    /// <summary>
    /// Provides the settings for the Data Pipeline service.
    /// </summary>
    public class DataPipelineServiceSettings : DataPipelineServiceSettingsBase
    {
        /// <summary>
        /// Gets or sets the queue used to submit data pipeline work items for the Data Pipeline Frontend Worker service.
        /// </summary>
        public required string FrontendWorkerQueue { get; set; }

        /// <summary>
        /// Gets or sets the queue used to submit data pipeline work items for the Data Pipeline Backend Worker service.
        /// </summary>
        public required string BackendWorkerQueue { get; set; }
    }
}
