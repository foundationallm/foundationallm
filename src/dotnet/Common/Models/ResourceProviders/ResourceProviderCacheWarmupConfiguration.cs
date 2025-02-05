namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the configuration for resource provider cache warmup.
    /// </summary>
    public class ResourceProviderCacheWarmupConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the service to which the configuration applies.
        /// </summary>
        public required string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the description of the cache warmup configuration.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of resource names that should be pre-cached.
        /// </summary>
        public required List<string> ResourceObjectIds { get; set; }

        /// <summary>
        /// Gets or sets the list of security principal ids that should be pre-cached.
        /// </summary>
        public required List<string> SecurityPrincipalIds { get; set; }
    }
}
