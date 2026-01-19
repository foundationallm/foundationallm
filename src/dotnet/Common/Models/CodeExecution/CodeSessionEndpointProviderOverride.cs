using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides overrides to the default code session endpoint provider configuration.
    /// </summary>
    public class CodeSessionEndpointProviderOverride
    {
        /// <summary>
        /// Gets or sets the user principal name (UPN) associated with the override.
        /// </summary>
        [JsonPropertyName("upn")]
        public required string UPN { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL of the code session service.
        /// </summary>
        [JsonPropertyName("endpoint")]
        public required string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the override is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public required bool Enabled { get; set; }
    }
}
