using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.Cache
{
    /// <summary>
    /// Provides the caching services used by the FoundationaLLM authorization service client.
    /// </summary>
    /// <param name="settings">The <see cref="AuthorizationServiceClientSettings"/> used to configure the authorization service client.</param>
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

        private readonly SemaphoreSlim _cacheLock = new(1, 1);

        /// <inheritdoc/>
        public async void SetValue(ActionAuthorizationRequest authorizationRequest, ActionAuthorizationResult result)
        {
            await _cacheLock.WaitAsync();
            try
            {
                var key = GetCacheKey(authorizationRequest);
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("An invalid cache key was generated.");
                    return;
                }

                _cache.Set(key, result, GetMemoryCacheEntryOptions());
                _logger.LogDebug(
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
            var authorizationRequestJson = JsonSerializer.Serialize(authorizationRequest);

            try
            {
                var key = GetCacheKey(authorizationRequest);
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("An invalid cache key was generated.");
                    return false;
                }

                if (_cache.TryGetValue(key, out ActionAuthorizationResult? cachedValue)
                    && cachedValue != null)
                {
                    authorizationResult = cachedValue;
                    _logger.LogDebug("Cache hit for the following authorization request: {AuthorizationRequest}.",
                        authorizationRequestJson);
                    return true;
                }

                _logger.LogDebug("Cache miss for the following authorization request: {AuthorizationRequest}.",
                        authorizationRequestJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error getting the ActionAuthorizationResult from the cache.");
            }

            return false;
        }

        private static string GetCacheKey(
            ActionAuthorizationRequest authorizationRequest)
        {
            var resourcePaths = authorizationRequest.ResourcePaths is { Count: > 0 }
                ? string.Join(",", authorizationRequest.ResourcePaths)
                : string.Empty;

            var groupIds = authorizationRequest.UserContext?.SecurityGroupIds is { Count: > 0 }
                ? string.Join(",", authorizationRequest.UserContext.SecurityGroupIds)
                : string.Empty;

            var securityPrincipalId = authorizationRequest.UserContext?.SecurityPrincipalId ?? string.Empty;

            // Use a StringBuilder for efficient concatenation.
            var sb = new StringBuilder();
            sb.Append(authorizationRequest.Action);
            sb.Append(':');
            sb.Append(resourcePaths);
            sb.Append(':');
            sb.Append(authorizationRequest.ExpandResourceTypePaths);
            sb.Append(':');
            sb.Append(authorizationRequest.IncludeRoles);
            sb.Append(':');
            sb.Append(authorizationRequest.IncludeActions);
            sb.Append(':');
            sb.Append(securityPrincipalId);
            sb.Append(':');
            sb.Append(groupIds);

            // SHA256.HashData is thread-safe and doesn't require locking.
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));

            // Convert to hex string (shorter than Base64, URL-safe).
            return Convert.ToHexString(hashBytes);
        }

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.AbsoluteCacheExpirationSeconds ?? 300)) // Cache entries are valid for 5 minutes.
           .SetSlidingExpiration(TimeSpan.FromSeconds(settings.SlidingCacheExpirationSeconds ?? 120)) // Reset expiration time if accessed within 2 minutes.
           .SetSize(1); // Each cache entry is a single authorization result.
    }
}
