using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Common.Services.Cache
{
    /// <summary>
    /// Provides the caching services used by the FoundationaLLM authorization service client.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used to log information.</param>
    public class AuthorizationServiceClientCacheService(
        AuthorizationServiceClientSettings settings,
        ILogger logger) : IAuthorizationServiceClientCacheService
    {
        private readonly ILogger _logger = logger;
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = settings.CacheSizeLimit ?? 10000, // Limit cache size.
            ExpirationScanFrequency = TimeSpan.FromSeconds(settings.CacheExpirationScanFrequencySeconds ?? 30) // How often to scan for expired items.
        });

        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.AbsoluteCacheExpirationSeconds ?? 300)) // Cache entries are valid for 5 minutes.
           .SetSlidingExpiration(TimeSpan.FromSeconds(settings.SlidingCacheExpirationSeconds ?? 120)) // Reset expiration time if accessed within 2 minutes.
           .SetSize(1); // Each cache entry is a single authorization result.

        private readonly SemaphoreSlim _cacheLock = new(1, 1);
        private readonly MD5 _md5 = MD5.Create();

        /// <inheritdoc/>
        public async void SetValue(ActionAuthorizationRequest authorizationRequest, ActionAuthorizationResult result)
        {
            await _cacheLock.WaitAsync();
            try
            {
                _cache.Set(GetCacheKey(authorizationRequest), result, _cacheEntryOptions);
                _logger.LogInformation(
                    "An action authorization result for the authorizable action {AuthorizableAction} has been set in the cache.",
                    authorizationRequest.Action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error setting the authorization result in the cache.");
            }           
            finally
            {
                _cacheLock.Release();
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue(ActionAuthorizationRequest authorizationRequest, out ActionAuthorizationResult? authorizationResult)
        {
            authorizationResult = default;
            try
            {
                if (_cache.TryGetValue(GetCacheKey(authorizationRequest), out ActionAuthorizationResult? cachedValue)
                    && cachedValue != null)
                {
                    authorizationResult = cachedValue;
                    _logger.LogInformation("Cache hit for the authorization request");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error getting the ActionAuthorizationResult from the cache.");
            }

            return false;
        }

        private string GetCacheKey(
            ActionAuthorizationRequest authorizationRequest)
        {
            var resourcePaths = string.Join(",", authorizationRequest.ResourcePaths);
            var groupIds = string.Join(",", authorizationRequest.UserContext.SecurityGroupIds);
            var userIdentity = $"{authorizationRequest.UserContext.SecurityPrincipalId}:{authorizationRequest.UserContext.UserPrincipalName}:{groupIds}";

            var keyString = $"{authorizationRequest.Action}:{resourcePaths}:{authorizationRequest.ExpandResourceTypePaths}:{authorizationRequest.IncludeRoles}:{authorizationRequest.IncludeActions}:{userIdentity}";

            var hashBytes = _md5.ComputeHash(Encoding.UTF8.GetBytes(keyString));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
