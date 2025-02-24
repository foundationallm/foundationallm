﻿namespace FoundationaLLM.Common.Models.Cache
{
    /// <summary>
    /// Provides a standard set of settings for caching.
    /// </summary>
    public record CacheSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled or not.
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
