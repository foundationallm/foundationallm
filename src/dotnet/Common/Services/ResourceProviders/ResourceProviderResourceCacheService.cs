using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Provides the resource caching services used by FoundationaLLM resource providers.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used to log information.</param>
    public class ResourceProviderResourceCacheService(
        ILogger logger) : IResourceProviderResourceCacheService
    {
        private readonly ILogger _logger = logger;

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 10000, // Limit cache size to 5000 resources.
                ExpirationScanFrequency = TimeSpan.FromMinutes(5) // Scan for expired items every five minutes.
            });
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60)) // Cache entries are valid for 60 minutes.
            .SetSlidingExpiration(TimeSpan.FromMinutes(30)) // Reset expiration time if accessed within 5 minutes.
            .SetSize(1); // Each cache entry is a single resource.

        /// <inheritdoc/>
        public void SetValue<T>(ResourceReference resourceReference, T resourceValue) where T : ResourceBase
        {
            try
            {
                _cache.Set<T>(resourceReference, resourceValue, _cacheEntryOptions);
                _logger.LogInformation("The resource {ResourceName} of type {ResourceType} has been set in the cache.",
                    resourceReference.Name,
                    resourceReference.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error setting the resource {ResourceName} of type {ResourceType} in the cache.",
                    resourceReference.Name,
                    resourceReference.Type);
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue<T>(ResourceReference resourceReference, out T? resourceValue) where T : ResourceBase
        {
            resourceValue = default;

            try
            {
                if (_cache.TryGetValue<T>(resourceReference, out T? cachedValue)
                    && cachedValue != null)
                {
                    resourceValue = cachedValue;
                    _logger.LogInformation("The resource {ResourceName} of type {ResourceType} has been retrieved from the cache.",
                        resourceReference.Name,
                        resourceReference.Type);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error getting the resource {ResourceName} of type {ResourceType} from the cache.",
                    resourceReference.Name,
                    resourceReference.Type);
            }

            return false;
        }
    }
}
