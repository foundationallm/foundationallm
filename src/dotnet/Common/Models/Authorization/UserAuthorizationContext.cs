using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents authorization context for a user.
    /// </summary>
    public class UserAuthorizationContext
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        [JsonPropertyName("security_principal_id")]
        public required string SecurityPrincipalId { get; set; }

        /// <summary>
        /// The user principal name (UPN).
        /// </summary>
        [JsonPropertyName("user_principal_name")]
        public required string UserPrincipalName { get; set; }

        /// <summary>
        /// The list of security group identifiers to which the user belongs.
        /// </summary>
        [JsonPropertyName("security_group_ids")]
        public required List<string> SecurityGroupIds { get; set; } = [];
    }
}
