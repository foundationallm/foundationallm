using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// The configuration for the file store.
    /// </summary>
    public class CoreConfiguration
    {
        /// <summary>
        /// Indicates the maximum number of files that can be uploaded in a single message.
        /// </summary>
        public int MaxUploadsPerMessage { get; set; } = 10;

        /// <summary>
        /// A list of API endpoint configurations for file store connectors.
        /// </summary>
        public IEnumerable<APIEndpointConfiguration>? FileStoreConnectors { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking the completion of a response.
        /// </summary>
        public int CompletionResponsePollingIntervalSeconds { get; set; } = 5;
    }
}
