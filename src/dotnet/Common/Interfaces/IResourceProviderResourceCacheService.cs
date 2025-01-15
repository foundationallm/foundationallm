using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides the resource caching services used by FoundationaLLM resource providers. 
    /// </summary>
    public interface IResourceProviderResourceCacheService
    {
        /// <summary>
        /// Tries to get a resource value identified by a resource reference from the cache.
        /// </summary>
        /// <typeparam name="T">The type of resource value to be retrieved.</typeparam>
        /// <param name="resourceReference">The <see cref="ResourceReference"/> used as a key in the cache.</param>
        /// <param name="resourceValue">The resource value to be retrieved.</param>
        /// <returns><see langword="true"/> is the resource value was found in the cache, <see langword="false"/> otherwise.</returns>
        bool TryGetValue<T>(ResourceReference resourceReference, out T? resourceValue) where T: ResourceBase;

        /// <summary>
        /// Sets a resource value identified by a resource reference in the cache.
        /// </summary>
        /// <typeparam name="T">The type of resource value to be set.</typeparam>
        /// <param name="resourceReference">The <see cref="ResourceReference"/> used as a key in the cache.</param>
        /// <param name="resourceValue">The resource value to be set.</param>
        void SetValue<T>(ResourceReference resourceReference, T resourceValue) where T : ResourceBase;

        /// <summary>
        /// Resets the cache.
        /// </summary>
        void Reset();
    }
}
