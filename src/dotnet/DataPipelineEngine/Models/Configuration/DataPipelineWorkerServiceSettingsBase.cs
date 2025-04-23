namespace FoundationaLLM.DataPipelineEngine.Models.Configuration
{
    /// <summary>
    /// Provides the base settings for Data Pipeline worker services.
    /// </summary>
    public class DataPipelineWorkerServiceSettingsBase : DataPipelineServiceSettingsBase
    {
        /// <summary>
        /// Gets or sets the name of the queue used for processing data pipeline work items.
        /// </summary>
        public required string Queue { get; set; }
    }
}
