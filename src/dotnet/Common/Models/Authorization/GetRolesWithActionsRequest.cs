using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
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
        [JsonPropertyName("scope")]
        public required string Scope { get; set; }
    }
}
