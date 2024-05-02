using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a request to authorize an action.
    /// </summary>
    public class ActionAuthorizationRequest
    {
        /// <summary>
        /// The authorizable action for which authorization is requested.
        /// </summary>
        [JsonPropertyName("action")]
        public required string Action { get; set; }

        /// <summary>
        /// The list of resources for which authorization is requested.
        /// </summary>
        [JsonPropertyName("resources")]
        public required List<string> ResourcePaths { get; set; }

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
    }
}
