namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the configuration for resource provider cache warmup.
    /// </summary>
    public class ResourceProviderCacheWarmupConfiguration
    {
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
