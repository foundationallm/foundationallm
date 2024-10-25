namespace FoundationaLLM.Common.Constants.Authentication
{
    /// <summary>
    /// Provides authetication key names for the AuthenticationParameters properties on ApiEndpointConfigurations
    /// </summary>
    public static class AuthenticationParametersKeys
    {
        /// <summary>
        /// The name of the App Config entry that contains the API key.
        /// </summary>
        public const string APIKeyConfigurationName = "api_key_configuration_name";

        /// <summary>
        /// The scope required for authentication.
        /// </summary>
        public const string Scope = "scope";

        /// <summary>
        /// The name of the header where the API key should provided.
        /// </summary>
        public const string APIKeyHeaderName = "api_key_header_name";

        /// <summary>
        /// An optional prefix that must be added before the API key (e.g., Bearer).
        /// </summary>
        public const string APIKeyPrefix = "api_key_prefix";

        /// <summary>
        /// All authentication parameter keys.
        /// </summary>
        public readonly static string[] All = [
            APIKeyConfigurationName,
            APIKeyHeaderName,
            Scope
        ];
    }
}
