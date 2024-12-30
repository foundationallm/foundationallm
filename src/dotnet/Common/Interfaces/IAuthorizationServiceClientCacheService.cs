using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides the caching services used by the FoundationaLLM authorization service client.
    /// </summary>
    public interface IAuthorizationServiceClientCacheService
    {
        /// <summary>
        /// Attempts to retrieve an <see cref="ActionAuthorizationResult"/> from the cache.
        /// </summary>
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> key used to identify the authorization result in the cache.</param>
        /// <param name="authorizationResult">The <see cref="ActionAuthorizationResult"/> to be retrieved.</param>
        /// <returns><see langword="true"/> if the value was found in the cache, <see langword="false"/> otherwise.</returns>
        bool TryGetValue(ActionAuthorizationRequest authorizationRequest, out ActionAuthorizationResult? authorizationResult);

        /// <summary>
        /// Sets an <see cref="ActionAuthorizationResult"/> in the cache.
        /// </summary>       
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> key used to set the authorization result in the cache.</param>
        /// <param name="authorizationResult">The <see cref="ActionAuthorizationResult"/> to be set.</param>
        void SetValue(ActionAuthorizationRequest authorizationRequest, ActionAuthorizationResult authorizationResult);
    }
}
