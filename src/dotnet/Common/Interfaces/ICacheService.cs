using FoundationaLLM.Common.Models.Cache;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Caches objects.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <typeparam name="T">The type of object retrieved from the cache.</typeparam>
        /// <param name="key">The <see cref="CacheKey"/> key identifying the object in the cache.</param>
        /// <returns>The object identified by the key.</returns>
        T? Get<T>(CacheKey key);

        /// <summary>
        /// Gets an object from the cache. If the object is not found in the cache, will attempt to retrieve
        /// it using a specified retriever.
        /// </summary>
        /// <typeparam name="T">The type of object retrieved from the cache.</typeparam>
        /// <param name="key">The <see cref="CacheKey"/> key identifying the object in the cache.</param>
        /// <param name="valueRetriever">The retriever used to get the value to populate the cache.</param>
        /// <param name="allowNull">Indicates whether null values returned by the value retriever should be cached or not.</param>
        /// <param name="expirationTime">The <see cref="TimeSpan"/> time to live.</param>
        /// <returns>The object identified by the key.</returns>
        Task<T?> Get<T>(CacheKey key, Func<Task<T>> valueRetriever, bool allowNull, TimeSpan? expirationTime);

        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <typeparam name="T">The type of object retrieved from the cache.</typeparam>
        /// <param name="key">The <see cref="CacheKey"/> key identifying the object in the cache.</param>
        /// <param name="value">The object to be added to the cache.</param>
        /// <param name="expirationTime">The <see cref="TimeSpan"/> time to live.</param>
        /// <returns></returns>
        void Set<T>(CacheKey key, T? value, TimeSpan? expirationTime);

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">The <see cref="CacheKey"/> key identifying the object in the cache.</param>
        void Remove(CacheKey key);

        /// <summary>
        /// Removes all objects belonging to a category from the cache.
        /// </summary>
        /// <param name="key">The <see cref="CacheKey"/> key identifying the object in the cache.</param>
        void RemoveByCategory(CacheKey key);

        /// <summary>
        /// Gets the number of items in the cache that belong to a specified category.
        /// </summary>
        /// <param name="categoryName">The name of the category.</param>
        /// <returns></returns>
        int GetItemsCount(string categoryName);
    }
}
