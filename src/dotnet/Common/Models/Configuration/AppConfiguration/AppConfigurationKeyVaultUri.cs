using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.AppConfiguration
{
    /// <summary>
    /// Provides details about an Azure Key Vault secret.
    /// </summary>
    public class AppConfigurationKeyVaultUri
    {
        /// <summary>
        /// The URI identifying the Azure Key Vault secret.
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; } 
    }
}
