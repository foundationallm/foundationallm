using FoundationaLLM.Authorization.Utils;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Authorization.Models
{
    /// <summary>
    /// Represents a security role assignment.
    /// </summary>
    public class RoleAssignment : ResourceBase
    {
        /// <summary>
        /// The unique identifier of the role definition.
        /// </summary>
        [JsonPropertyName("role_definition_id")]
        public required string RoleDefinitionId { get; set; }

        /// <summary>
        /// The unique identifier of the security principal to which the role is assigned.
        /// </summary>
        [JsonPropertyName("principal_id")]
        public required string PrincipalId { get; set; }

        /// <summary>
        /// The type of the security principal to which the role is assigned. Can be User, Group, or ServicePrincipal.
        /// </summary>
        [JsonPropertyName("principal_type")]
        public required string PrincipalType { get; set; }

        /// <summary>
        /// The scope at which the role is assigned.
        /// </summary>
        [JsonPropertyName("scope")]
        public required string Scope { get; set; }

        /// <summary>
        /// The <see cref="ResourcePath"/> resulting from parsing the scope path.
        /// </summary>
        [JsonIgnore]
        public ResourcePath? ScopeResourcePath { get; set; }

        /// <summary>
        /// The <see cref="RoleDefinition"/> referenced by the <see cref="RoleDefinitionId"/> property.
        /// </summary>
        [JsonIgnore]
        public RoleDefinition? RoleDefinition { get; set; }

        /// <summary>
        /// The explicit list of all allowed actions resulting from expanding all wildcards.
        /// </summary>
        [JsonIgnore]
        public HashSet<string> AllowedActions { get; set; } = [];

        public void Enrich(List<string> allowedInstanceIds)
        {
            ScopeResourcePath = ResourcePathUtils.ParseForRoleAssignmentScope(
                Scope,
                allowedInstanceIds);

            RoleDefinition = RoleDefinitions.All[RoleDefinitionId];
            AllowedActions = new HashSet<string>(
                RoleDefinition.GetAllowedActions());
        }
    }
}
