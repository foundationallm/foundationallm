using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.DataPipelineEngine.Models.Configuration
{
    /// <summary>
    /// Provides the base settings for Data Pipeline services.
    /// </summary>
    public class DataPipelineServiceSettingsBase
    {
        /// <summary>
        /// Gets or sets the storage settings.
        /// </summary>
        public required BlobStorageServiceSettings Storage { get; set; }
    }
}
