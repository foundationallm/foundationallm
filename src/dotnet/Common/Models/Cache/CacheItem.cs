namespace FoundationaLLM.Common.Models.Cache
{
    /// <summary>
    /// The cached object.
    /// </summary>
    public class CacheItem
    {
        /// <summary>
        /// The value of the object being cached.
        /// </summary>
        public required object? Value { get; set; }

        /// <summary>
        /// The UTC expiration time of the object being cached.
        /// </summary>
        public DateTime? ExpirationTimeUtc { get; set; }

        /// <summary>
        /// Indicates whether the cached object expired or not.
        /// </summary>
        public bool IsExpired => (ExpirationTimeUtc != null) && (DateTime.UtcNow > ExpirationTimeUtc);
    }
}
