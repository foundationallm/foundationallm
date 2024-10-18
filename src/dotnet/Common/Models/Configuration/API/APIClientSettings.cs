namespace FoundationaLLM.Common.Models.Configuration.API
{
    /// <summary>
    /// Standard settings for an API client.
    /// </summary>
    public record APIClientSettings
    {
        /// <summary>
        /// Specifies the timeout for the downstream API HTTP client.
        /// If this value is null, the default timeout is used.
        /// For an infinite waiting period, use <see cref="Timeout.InfiniteTimeSpan"/>
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
