namespace FoundationaLLM.Common.Models.Configuration.Authorization
{
    /// <summary>
    /// Authorization service client settings
    /// </summary>
    public record AuthorizationServiceClientSettings
    {
        /// <summary>
        /// Provides the API URL of the Authorization service.
        /// </summary>
        public required string APIUrl { get; set; }

        /// <summary>
        /// Provides the API scope of the Authorization service.
        /// </summary>
        public required string APIScope { get; set; }

        /// <summary>
        /// Indicates whether to use caching in the Authorization service client.
        /// </summary>
        public bool EnableCache { get; set; } = false;

        /// <summary>
        /// Absolute cache expiration in seconds.
        /// </summary>
        public double? AbsoluteCacheExpirationSeconds { get; set; } = 300;

        /// Sets how many seconds the cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        public double? SlidingCacheExpirationSeconds { get; set; } = 120;

        /// <summary>
        /// The maximum number of items that can be stored in the cache.
        /// </summary>
        public long? CacheSizeLimit { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items in seconds.
        /// </summary>
        public double? CacheExpirationScanFrequencySeconds { get; set; } = 30;
    }
}
