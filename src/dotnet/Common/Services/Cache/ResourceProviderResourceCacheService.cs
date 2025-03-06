using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Cache
{
    /// <summary>
    /// Provides the resource caching services used by FoundationaLLM resource providers.
    /// </summary>
    public class ResourceProviderResourceCacheService : IResourceProviderResourceCacheService
    {
        private readonly ResourceProviderCacheSettings _cacheSettings;
        private readonly ILogger _logger;
        private IMemoryCache _cache;
        private SemaphoreSlim _cacheLock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceProviderResourceCacheService"/> class.
        /// </summary>
        /// <param name="cacheSettings">The <see cref="ResourceProviderCacheSettings"/> providing settings for the cache.</param>
        /// <param name="logger">The <see cref="ILogger"/> used to log information.</param>
        public ResourceProviderResourceCacheService(
            ResourceProviderCacheSettings cacheSettings,
            ILogger logger)
        {
            _cacheSettings = cacheSettings;
            _logger = logger;
            _cache = CreateCache();
        }

        /// <inheritdoc/>
        public void SetValue<T>(ResourceReference resourceReference, T resourceValue) where T : ResourceBase
        {
            try
            {
                _cacheLock.Wait();
                _cache.Set(GetCacheKey(resourceReference), resourceValue, GetMemoryCacheEntryOptions());
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
            finally
            {
                _cacheLock.Release();
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue<T>(ResourceReference resourceReference, out T? resourceValue) where T : ResourceBase
        {
            resourceValue = default;

            try
            {
                _cacheLock.Wait();
                if (_cache.TryGetValue(GetCacheKey(resourceReference), out T? cachedValue)
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
            finally
            {
                _cacheLock.Release();
            }

            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            try
            {
                _cacheLock.Wait();
                _cache = CreateCache();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error resetting the cache.");
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private string GetCacheKey(ResourceReference resourceReference) =>
            $"{resourceReference.Type}|{resourceReference.Name}";

        private MemoryCache CreateCache() =>
            new(new MemoryCacheOptions
            {
                SizeLimit = _cacheSettings.CacheSizeLimit,
                ExpirationScanFrequency = TimeSpan.FromSeconds(
                    _cacheSettings.CacheExpirationScanFrequencySeconds ?? 30)
            });

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheSettings.AbsoluteCacheExpirationSeconds ?? 300))
            .SetSlidingExpiration(TimeSpan.FromSeconds(_cacheSettings.SlidingCacheExpirationSeconds ?? 120))
            .SetSize(1); // Each cache entry is a single resource.
    }
}
