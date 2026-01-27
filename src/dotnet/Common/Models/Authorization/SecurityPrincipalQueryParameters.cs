using FoundationaLLM.Common.Constants.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Parameters for querying role assignments.
    /// </summary>
    public class SecurityPrincipalQueryParameters
    {
        /// <summary>
        /// Gets or sets the name of the security principal to query.
        /// </summary>
        /// <remarks>
        /// The name can be a partial name used to indetify multiple security principals.
        /// When this property is specified, the SecurityPrincipalName property muse be set
        /// and the Ids property must be an empty array.
        /// </remarks>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the security principal to query.
        /// </summary>
        [JsonPropertyName("upn")]
        public string? UPN { get; set; }

        /// <summary>
        /// Gets or sets the type of security principal to query.
        /// Must be one of the <see cref="SecurityPrincipalTypes"/> values.
        /// </summary>
        /// <remarks>
        /// This property must be set when the Name property is specified.
        /// </remarks>
        [JsonPropertyName("security_principal_type")]
        public string? SecurityPrincipalType { get; set; }

        /// <summary>
        /// Gets of sets a list of security principal identifiers to query.
        /// </summary>
        /// <remarks>
        /// When this property is specified, the Name and SecurityPrincipalType properties must be null.
        /// </remarks>
        [JsonPropertyName("ids")]
        public string[]? Ids { get; set; }
    }
}
