namespace FoundationaLLM.Common.Constants.Authorization
{
    /// <summary>
    /// Provides authorization policy names.
    /// </summary>
    public static class AuthorizationPolicyNames
    {
        /// <summary>
        /// Standard authorization policy for Microsoft Entra ID.
        /// </summary>
        public const string MicrosoftEntraIDStandard = "MicrosoftEntraIDStandard";

        /// <summary>
        /// Authorization policy for Microsoft Entra ID with no scopes.
        /// </summary>
        public const string MicrosoftEntraIDNoScopes = "MicrosoftEntraIDNoScopes";

        /// <summary>
        /// Authorization policy for FoundationaLLM Agent Access Tokens.
        /// </summary>
        public const string FoundationaLLMAgentAccessToken = "FoundationaLLMAgentAccessToken";
    }
}
