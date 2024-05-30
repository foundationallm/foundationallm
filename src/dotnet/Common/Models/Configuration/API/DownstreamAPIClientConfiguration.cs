using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Models.Configuration.API
{
    /// <inheritdoc/>
    public record DownstreamAPISettings : IDownstreamAPISettings
    {
        /// <inheritdoc/>
        public required Dictionary<string, DownstreamAPIClientConfiguration> DownstreamAPIs { get; init; }
    }

    /// <summary>
    /// Represents settings for downstream API clients.
    /// </summary>
    public record DownstreamAPIClientConfiguration
    {
        /// <summary>
        /// The URL of the downstream API.
        /// </summary>
        public required string APIUrl { get; init; }
        /// <summary>
        /// The value of the API key.
        /// </summary>
        public required string APIKey { get; init; }
        /// <summary>
        /// Specifies the timeout for the downstream API HTTP client.
        /// If this value is null, the default timeout is used.
        /// For an infinite waiting period, use <see cref="Timeout.InfiniteTimeSpan"/>
        /// </summary>
        public TimeSpan? Timeout { get; init; }
    }
}
