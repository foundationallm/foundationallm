namespace FoundationaLLM.Core.Examples.Models
{
    /// <summary>
    /// Configuration options for the HTTP client.
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// The base URI for the HTTP client.
        /// </summary>
        public string? BaseUri { get; set; }

        /// <summary>
        /// The authentication scope for the HTTP client.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// The timeout for the HTTP client.
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
