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
        /// The name of the configuration setting holding connection information
        /// </summary>
        public required string ConnectionConfigurationName { get; set; }

        /// <summary>
        /// The timeout in seconds available for the processing of each request provided by the request source.
        /// If a request is not processed within the timeout window it will become visible again to
        /// request managers and might be picked up for processing again.
        /// </summary>
        public required int VisibilityTimeoutSeconds { get; set; }

        /// <summary>
        /// The connection string to connect to the request source queue.
        /// </summary>
        public string? ConnectionString {  get; set; } 
    }
}
