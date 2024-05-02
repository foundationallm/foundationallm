using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings to initialize a request source service.
    /// </summary>
    public class RequestSourceServiceSettings
    {
        /// <summary>
        /// The name of the request source.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The name of the storage account.
        /// </summary>
        public required string AccountName { get; set; }

        /// <summary>
        /// The timeout in seconds available for the processing of each request provided by the request source.
        /// If a request is not processed within the timeout window it will become visible again to
        /// request managers and might be picked up for processing again.
        /// </summary>
        public required int VisibilityTimeoutSeconds { get; set; }
    }
}
