namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Contains constants for environment variables used by the application.
    /// </summary>
    public static class EnvironmentVariables
    {
        /// <summary>
        /// The client id of the user assigned managed identity.
        /// </summary>
        public const string AzureClientId = "AZURE_CLIENT_ID";

        /// <summary>
        /// The build version of the container. This is also used for the app version used
        /// to validate the minimum version of the app required to use certain configuration entries.
        /// </summary>
        public const string FoundationaLLM_Version = "FOUNDATIONALLM_VERSION";

        /// <summary>
        /// The key for the FoundationaLLM:AppConfig:ConnectionString environment variable.
        /// This allows the caller to connect to the Azure App Configuration service.
        /// </summary>
        public const string FoundationaLLM_AppConfig_ConnectionString = "FoundationaLLM_AppConfig_ConnectionString";

        /// <summary>
        /// They key for the FoundationaLLM:Configuration:KeyVaultURI environment variable.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_KeyVaultURI = "FoundationaLLM_AuthorizationAPI_KeyVaultURI";
    }
}
