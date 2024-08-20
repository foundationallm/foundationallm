namespace FoundationaLLM.Common.Models.Cache
{
    /// <summary>
    /// Composite caching key.
    /// </summary>
    /// <param name="name">The name of the object identified by the cache key.</param>
    /// <param name="category">The category of the object identified by the cache key.</param>
    public class CacheKey(
        string name,
        string category)
    {
        /// <summary>
        /// The name of the object from the cache.
        /// </summary>
        public string Name = name;

        /// <summary>
        /// The category of the object from the cache.
        /// </summary>
        public string Category = category;

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            (obj is CacheKey cacheKey)
            && (cacheKey.Name == Name)
            && (cacheKey.Category == Category);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(Name, Category);
    }
}
