using FoundationaLLM.Core.Examples.Models;

namespace FoundationaLLM.Core.Examples.Interfaces
{
    /// <summary>
    /// One or more downstream APIs with a base URL and API key for authentication.
    /// </summary>
    public interface IDownstreamAPISettings
    {
        /// <summary>
        /// A dictionary of downstream APIs with a base URL and API key for authentication.
        /// </summary>
        Dictionary<string, DownstreamAPIKeySettings> DownstreamAPIs { get; }
    }
}
