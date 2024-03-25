using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.Environment;

namespace FoundationaLLM.Configuration.Catalog
{
    /// <summary>
    /// Provides the catalog of environment variables required by the solution.
    /// </summary>
    public static class EnvironmentVariablesCatalog
    {
        private static readonly List<EnvironmentVariableEntry> GenericEntries =
            [
                new EnvironmentVariableEntry(EnvironmentVariables.FoundationaLLM_Version,
                    "The build version of the container. This is also used for the app version used to validate the minimum version of the app required to use certain configuration entries."),
            ];

        private static readonly List<EnvironmentVariableEntry> AppConfigEntries =
            [
                new EnvironmentVariableEntry(EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString,
                    "The connection string for the App Configuration service.")
            ];

        private static readonly List<EnvironmentVariableEntry> AuthorizationEntries =
            [
                new EnvironmentVariableEntry(EnvironmentVariables.FoundationaLLM_AuthorizationAPI_KeyVaultURI,
                    "The URI of the Azure Key Vault used by the Authorization API.")
            ];

        /// <summary>
        /// Returns the required environment variables.
        /// </summary>
        /// <param name="serviceName">Optional service name. When not specified, the default environment variables list is returned.</param>
        /// <returns></returns>
        public static IEnumerable<EnvironmentVariableEntry> GetRequiredEnvironmentVariables(
            string serviceName = "") =>
            serviceName switch
            {
                ServiceNames.AuthorizationAPI => GenericEntries.Concat(AuthorizationEntries),
                _ => GenericEntries.Concat(AppConfigEntries),
            };
    }

}
