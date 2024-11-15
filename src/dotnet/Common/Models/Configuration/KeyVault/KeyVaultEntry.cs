namespace FoundationaLLM.Common.Models.Configuration.KeyVault
{
    /// <summary>
    /// Represents a single entry in the Key Vault catalog.
    /// </summary>
    /// <param name="secretName"></param>
    /// <param name="minimumVersion"></param>
    /// <param name="description"></param>
    public class KeyVaultSecretEntry(string secretName, string? minimumVersion, string? description = null)
    {
        /// <summary>
        /// The name of the secret in the Key Vault.
        /// </summary>
        public string SecretName { get; set; } = secretName;
        /// <summary>
        /// The minimum version of the app that is required to use this Key Vault secret.
        /// </summary>
        public string? MinimumVersion { get; } = minimumVersion;
        /// <summary>
        /// A description of the Key Vault secret.
        /// </summary>
        public string? Description { get; set; } = description;
    }
}
