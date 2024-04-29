using FoundationaLLM.Authorization.Models;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Interfaces
{
    /// <summary>
    /// Provides methods for managing roles.
    /// </summary>
    public interface IRoleManagementService
    {
        /// <summary>
        /// Returns a list of all role definitions.
        /// </summary
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>A list of all role definitions.</returns>
        Task<List<RoleDefinition>> GetRoleDefinitions(string instanceId);

        /// <summary>
        /// Returns a list of all role definitions at the specified scope.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="scope">The role assignment scope.</param>
        /// <returns>A list of all role definitions.</returns>
        Task<List<RoleDefinition>> GetRoleDefinitionsForScope(string instanceId, string scope);

        /// <summary>
        /// Returns the role definition for the specified id.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleDefinitionId">The role definition unique identifier.</param>
        /// <returns>A role definition.</returns>
        Task<RoleDefinition> GetRoleDefinition(string instanceId, Guid roleDefinitionId);

        /// <summary>
        /// Returns a list of all role assignments for the specified role definition id.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleDefinitionId">The role definition unique identifier.</param>
        /// <returns>A list of role assignments.</returns>
        Task<List<RoleAssignment>> GetRoleAssignments(string instanceId, Guid roleDefinitionId);

        /// <summary>
        /// Returns a list of all role assignments for the specified scope.
        /// </summary
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="scope">The role assignment scope.</param>
        /// <returns>A list of role assignments.</returns>
        Task<List<RoleAssignment>> GetRoleAssignmentsForScope(string instanceId, string scope);

        /// <summary>
        /// Returns the role assignment for the specified role definition and assignment unique ids.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleDefinitionId">The role definition unique identifier.</param>
        /// <param name="roleAssignmentId">The role assignment unique identifier.</param>
        /// <returns>A role assignment.</returns>
        Task<RoleAssignment> GetRoleAssignment(string instanceId, Guid roleDefinitionId, Guid roleAssignmentId);
    }
}
