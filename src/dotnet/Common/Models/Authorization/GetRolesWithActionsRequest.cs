using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a request to fetch the roles and allowed actions for a resource scope.
    /// </summary>
    public class GetRolesWithActionsRequest
    {
        /// <summary>
        /// The id of the security principal requesting authorization.
        /// </summary>
        [JsonPropertyName("principal_id")]
        public required string PrincipalId { get; set; }

        /// <summary>
        /// The list of security group ids to which the principal belongs.
        /// </summary>
        [JsonPropertyName("security_group_ids")]
        public List<string> SecurityGroupIds { get; set; } = [];

        /// <summary>
        /// The scope of the request.
        /// </summary>
        [JsonPropertyName("scopes")]
        public required List<string> Scopes { get; set; }
    }
}
