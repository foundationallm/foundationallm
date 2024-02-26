using Azure.Core;
using Azure.Identity;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Provides the default credentials for authentication.
    /// </summary>
    public static class DefaultAuthentication
    {
        /// <summary>
        /// The default Azure credential to use for authentication.
        /// </summary>
        public static TokenCredential GetAzureCredential(bool development = false) => new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeAzureDeveloperCliCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeEnvironmentCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeSharedTokenCacheCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true,
            ExcludeWorkloadIdentityCredential = true,

            ExcludeAzureCliCredential = !development,
            ExcludeManagedIdentityCredential = development
        });
    }
}
