using Azure.Core;
using Azure.Identity;
using FoundationaLLM.Common.Constants.Configuration;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Provides the default credentials for authentication.
    /// </summary>
    public static class DefaultAuthentication
    {
        /// <summary>
        /// Indicates whether the environment we run in is production or not.
        /// </summary>
        public static bool Production {  get; set; }

        /// <summary>
        /// The default Azure credential to use for authentication.
        /// </summary>
        public static TokenCredential GetAzureCredential() =>
            Production
            ? new ManagedIdentityCredential(Environment.GetEnvironmentVariable(EnvironmentVariables.AzureClientId))
            : new AzureCliCredential();
    }
}
