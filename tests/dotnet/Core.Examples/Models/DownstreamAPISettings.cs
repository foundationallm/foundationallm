using FoundationaLLM.Core.Examples.Interfaces;

namespace FoundationaLLM.Core.Examples.Models
{
    public record DownstreamAPISettings : IDownstreamAPISettings
    {
        /// <inheritdoc/>
        public required Dictionary<string, DownstreamAPIKeySettings> DownstreamAPIs { get; init; }
    }

    /// <summary>
    /// Represents settings for downstream API key authentication.
    /// </summary>
    public record DownstreamAPIKeySettings
    {
        /// <summary>
        /// The URL of the downstream API.
        /// </summary>
        public required string APIUrl { get; init; }
        /// <summary>
        /// The value of the API key.
        /// </summary>
        public required string APIKey { get; init; }
    }
}
