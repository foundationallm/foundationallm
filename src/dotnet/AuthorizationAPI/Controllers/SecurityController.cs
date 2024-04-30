using FoundationaLLM.Authorization.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Authorization.API.Controllers
{
    /// <summary>
    /// Provides security endpoints.
    /// </summary>
    /// <param name="authorizationCore">The <see cref="IAuthorizationCore"/> service used to process authorization requests.</param>
    /// <param name="roleManagementService">The <see cref="IRoleManagementService"/> service.</param>
    [Authorize(Policy = "RequiredClaims")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/security")]
    public class SecurityController(
        IAuthorizationCore authorizationCore,
        IRoleManagementService roleManagementService) : Controller
    {
        private readonly IAuthorizationCore _authorizationCore = authorizationCore;
        private readonly IRoleManagementService _roleManagementService = roleManagementService;

        #region IAuthorizationCore

        /// <summary>
        /// Returns a list of role names and a list of allowed actions for the specified scope.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="request">The get roles with actions request.</param>
        /// <returns>The get roles and actions result.</returns>
        [HttpPost("roles/actions")]
        public IActionResult ProcessGetRolesWithActions(string instanceId, [FromBody] GetRolesWithActionsRequest request) =>
            new OkObjectResult(_authorizationCore.ProcessGetRolesWithActions(instanceId, request));

        /// <summary>
        /// Assigns a role to an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment.</param>
        /// <returns></returns>
        [HttpPost("roles/assign")]
        public async Task<IActionResult> AssignRole(string instanceId, RoleAssignmentRequest roleAssignmentRequest) =>
            new OkObjectResult(await _authorizationCore.AssignRole(instanceId, roleAssignmentRequest));

        /// <summary>
        /// Revokes a role from an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment.</param>
        /// <returns></returns>
        [HttpPost("roles/revoke")]
        public async Task<IActionResult> RevokeRole(string instanceId, RoleAssignmentRequest roleAssignmentRequest) =>
            new OkObjectResult(await _authorizationCore.RevokeRole(instanceId, roleAssignmentRequest));

        #endregion

        #region IRoleManagementService

        /// <summary>
        /// Returns the role assignment for the specified role definition and assignment unique ids.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleDefinitionId">The role definition unique identifier.</param>
        /// <param name="roleAssignmentId">The role assignment unique identifier.</param>
        /// <returns>A role assignment.</returns>
        [HttpGet("roles/{roleDefinitionId}/assignments/{roleAssignmentId}")]
        public async Task<IActionResult> GetRoleAssignment(string instanceId, Guid roleDefinitionId, Guid roleAssignmentId) =>
            new OkObjectResult(await _roleManagementService.GetRoleAssignment(instanceId, roleDefinitionId, roleAssignmentId));


        /// <summary>
        /// Returns a list of all role definitions.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>A list of all role definitions.</returns>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoleDefinitions(string instanceId) =>
            new OkObjectResult(await _roleManagementService.GetRoleDefinitions(instanceId));

        /// <summary>
        /// Returns a list of all role definitions at the specified scope.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="scope">The role assignment scope.</param>
        /// <returns>A list of all role definitions.</returns>
        [HttpGet("roles/scope/{*scope}")]
        public async Task<IActionResult> GetRoleDefinitionsForScope(string instanceId, string scope) =>
            new OkObjectResult(await _roleManagementService.GetRoleDefinitionsForScope(instanceId, scope));

        /// <summary>
        /// Returns the role definition for the specified id.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleDefinitionId">The role definition unique identifier.</param>
        /// <returns>A role definition.</returns>
        [HttpGet("roles/{roleDefinitionId}")]
        public async Task<IActionResult> GetRoleDefinition(string instanceId, Guid roleDefinitionId) =>
            new OkObjectResult(await _roleManagementService.GetRoleDefinition(instanceId, roleDefinitionId));

        /// <summary>
        /// Returns a list of all role assignments for the specified role definition id.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleDefinitionId">The role definition unique identifier.</param>
        /// <returns>A list of role assignments.</returns>
        [HttpGet("roles/{roleDefinitionId}/assignments")]
        public async Task<IActionResult> GetRoleAssignments(string instanceId, Guid roleDefinitionId) =>
            new OkObjectResult(await _roleManagementService.GetRoleAssignments(instanceId, roleDefinitionId));

        /// <summary>
        /// Returns a list of all role assignments for the specified scope.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="scope">The role assignment scope.</param>
        /// <returns>A list of role assignments.</returns>
        [HttpGet("roles/assignments/scope/{*scope}")]
        public async Task<IActionResult> GetRoleAssignmentsForScope(string instanceId, string scope) =>
            new OkObjectResult(await _roleManagementService.GetRoleAssignmentsForScope(instanceId, scope));


        #endregion
    }
}
