namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Represents configuration settings for embedding operations.
    /// </summary>
    public class EmbeddingSettings
    {
        /// <summary>
        /// Gets or sets the API Endpoint Configuration object identifier for the endpoint used for embedding operations.
        /// </summary>
        public required string EmbeddingAPIEndpointConfigurationObjectId { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that maps model names to their corresponding model deployment names.
        /// </summary>
        /// <remarks>The keys are model names (e.g., text-embedding-3-large) and the values are model deployment names.</remarks>
        public Dictionary<string, string> ModelDeployments { get; set; } = [];
    }
}
