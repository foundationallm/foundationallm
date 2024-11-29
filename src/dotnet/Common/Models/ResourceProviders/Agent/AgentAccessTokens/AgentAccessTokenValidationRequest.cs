namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens
{
    /// <summary>
    /// Agent access token validation request object.
    /// </summary>
    public class AgentAccessTokenValidationRequest
    {
        /// <summary>
        /// The access token to validate.
        /// </summary>
        public required string AccessToken { get; set; }
    }
}
