using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines methods exposed by the Authorization service.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Processes an action authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="action">The action identifier.</param>
        /// <param name="resourcePaths">The resource paths.</param>
        /// <param name="expandResourceTypePaths">A value indicating whether to expand resource type paths that are not authorized.</param>
        /// <param name="includeRoles">A value indicating whether to include roles in the response.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <remarks>
        /// <para>
        /// If the action specified by <paramref name="action"/> is not authorized for a resource type path,
        /// and <paramref name="expandResourceTypePaths"/> is set to <c>true</c>, the response will include
        /// any authorized resource paths matching the resource type path.
        /// </para>
        /// <para>
        /// If <paramref name="includeRoles"/> is set to <c>true</c>, for each authrorized resource path, the response will include
        /// the roles from the role assignments that authorize the action for the resource path.
        /// </para>
        /// </remarks>
        /// <returns>An <see cref="ActionAuthorizationResult"/> containing the result of the processing.</returns>
        Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            string action,
            List<string> resourcePaths,
            bool expandResourceTypePaths,
            bool includeRoles,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a new role assignment.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The <see cref="RoleAssignmentRequest"/> containing the details of the role assignment to be created.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>A <see cref="RoleAssignmentOperationResult"/> containing information about the result of the operation.</returns>
        Task<RoleAssignmentOperationResult> CreateRoleAssignment(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Returns a list of role assignments.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="queryParameters">The <see cref="RoleAssignmentQueryParameters"/> providing the inputs for filtering the role assignments.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>The list of all role assignments for the specified instance.</returns>
        Task<List<RoleAssignment>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Deletes a role assignment.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment object identifier.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>A <see cref="RoleAssignmentOperationResult"/> containing information about the result of the operation.</returns>
        Task<RoleAssignmentOperationResult> DeleteRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity);
    }
}
