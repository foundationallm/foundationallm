using FoundationaLLM.Common.Models.Authentication;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens
{
    /// <summary>
    /// Agent access token validation result object.
    /// </summary>
    public class AgentAccessTokenValidationResult
    {
        /// <summary>
        /// Gets or sets the flag indicating whether the agent access token is valid.
        /// </summary>
        public required bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnifiedUserIdentity"/> virtual identity associated with the agent access token.
        /// </summary>
        public required UnifiedUserIdentity VirtualIdentity { get; set; }
    }
}
