using FoundationaLLM.Common.Models.Configuration.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// One or more downstream APIs with a base URL and API key for authentication.
    /// </summary>
    public interface IDownstreamAPISettings
    {
        /// <summary>
        /// A dictionary of downstream APIs with a base URL and API key for authentication.
        /// </summary>
        Dictionary<string, DownstreamAPIClientConfiguration> DownstreamAPIs { get; }
    }
}
