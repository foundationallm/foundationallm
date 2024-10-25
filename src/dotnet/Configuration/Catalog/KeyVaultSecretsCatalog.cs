using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.KeyVault;

namespace FoundationaLLM.Configuration.Catalog
{
    /// <summary>
    /// A catalog of Key Vault secrets used in this solution.
    /// </summary>
    public static class KeyVaultSecretsCatalog
    {
        /// <summary>
        /// Returns the list of all the Key Vault secrets for this solution that are required for the given version.
        /// </summary>
        /// <param name="version">The current version of the caller.</param>
        /// <param name="serviceName">Optional service name. When not specified, the generic key vault secrets list is returned.</param>
        /// <returns></returns>
        public static IEnumerable<KeyVaultSecretEntry> GetRequiredKeyVaultSecretsForVersion(
            string version,
            string serviceName = "")
        {
            // Extract the numeric part of the version, ignoring pre-release tags.
            var numericVersionPart = version.Split('-')[0];
            if (!Version.TryParse(numericVersionPart, out var currentVersion))
            {
                throw new ArgumentException($"Invalid version format for the provided version ({version}).", nameof(version));
            }

            var entriesList =  (serviceName == ServiceNames.AuthorizationAPI)
                ? new List<KeyVaultSecretEntry>()
                : new List<KeyVaultSecretEntry>();

            // Compare based on the Major, Minor, and Build numbers only.
            return entriesList.Where(entry =>
            {
                if (string.IsNullOrWhiteSpace(entry.MinimumVersion))
                {
                    return false;
                }

                var entryNumericVersionPart = entry.MinimumVersion.Split('-')[0];
                if (!Version.TryParse(entryNumericVersionPart, out var entryVersion))
                {
                    return false;
                }

                var entryVersionWithoutRevision = new Version(entryVersion.Major, entryVersion.Minor, entryVersion.Build);
                var currentVersionWithoutRevision = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                return entryVersionWithoutRevision <= currentVersionWithoutRevision;
            });
        }
    }

}
