using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Provides settings for the orchestration service.
    /// </summary>
    public class OrchestrationServiceSettings
    {
        /// <summary>
        /// Gets or sets the <see cref="BlobStorageServiceSettings"/> used to store exported completion requests.
        /// </summary>
        public required BlobStorageServiceSettings CompletionRequestsStorage { get; set; }
    }
}
