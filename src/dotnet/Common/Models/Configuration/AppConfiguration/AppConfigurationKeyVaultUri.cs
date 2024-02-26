using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
