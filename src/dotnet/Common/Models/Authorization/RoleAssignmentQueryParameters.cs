using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Parameters for querying role assignments.
    /// </summary>
    public class RoleAssignmentQueryParameters
    {
        /// <summary>
        /// The role assignment scope (resource object id).
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope {  get; set; }

        /// <summary>
        /// Gets or sets the list of security principal identifiers for which to filter role assignments.
        /// </summary>
        [JsonPropertyName("security_principal_ids")]
        public List<string>? SecurityPrincipalIds { get; set; }
    }
}
