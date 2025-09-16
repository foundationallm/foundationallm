using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Authorization
{
    /// <summary>
    /// Represents a security principal, such as a user, group, or service, that can be assigned permissions or access
    /// rights.
    /// </summary>
    public class SecurityPrincipal : ResourceBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the security principal.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the security principal.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the on-premises account associated with the security principal.
        /// </summary>
        [JsonPropertyName("onpremises_account_name")]
        public string? OnPremisesAccountName { get; set; }
    }
}
