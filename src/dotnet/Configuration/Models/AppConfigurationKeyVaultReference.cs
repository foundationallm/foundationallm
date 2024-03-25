using System.Text.Json.Serialization;

namespace FoundationaLLM.Configuration.Models
{
    /// <summary>
    /// Azure App Configuration key vault reference.
    /// </summary>
    public class AppConfigurationKeyVaultReference : AppConfigurationKeyBase
    {
        /// <summary>
        /// The URI of the Azure Key Vault service being referenced.
        /// </summary>
        [JsonPropertyName("key_vault_uri")]
        public string? KeyVaultUri { get; set; }

        /// <summary>
        /// The name of the Azure Key Vault secret being referenced.
        /// </summary>
        [JsonPropertyName("key_vault_secret_name")]
        public string? KeyVaultSecretName { get; set; }
    }
}
