using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Common HTTP headers used throughout the FoundationaLLM infrastructure.
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>
        /// API key header used by APIs to authenticate requests.
        /// </summary>
        public const string APIKey = "X-API-KEY";
        /// <summary>
        /// User identity header used by APIs to pass user identity information.
        /// </summary>
        public const string UserIdentity = "X-USER-IDENTITY";
        /// <summary>
        /// When the FoundationaLLM-AllowAgentHint feature flag is enabled, this header
        /// can be used by the client to pass an agent hint to the API. Used mainly for
        /// demo purposes.
        /// </summary>
        public const string AgentHint = "X-AGENT-HINT";
    }
}
