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
        /// Absolute cache expiration in minutes.
        /// </summary>
        public int? AbsoluteCacheExpirationMinutes { get; set; } = 5;

        /// Sets how long the cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        public int? SlidingCacheExpirationMinutes { get; set; } = 2;

        /// <summary>
        /// The maximum number of items that can be stored in the cache.
        /// </summary>
        public long? CacheSizeLimit { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan? CacheExpirationScanFrequency { get; set; } = TimeSpan.FromSeconds(30);
    }
}
