using FoundationaLLM.AuthorizationEngine.Interfaces;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Authorization.API.Controllers
{
    /// <summary>
    /// Provides security endpoints.
    /// </summary>
    /// <param name="authorizationCore">The <see cref="IAuthorizationCore"/> service used to process authorization requests.</param>
    [Authorize(Policy = AuthorizationPolicyNames.MicrosoftEntraIDNoScopes)]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/roleassignments")]
    public class RoleAssignmentsController(
        IAuthorizationCore authorizationCore) : Controller
    {
        private readonly IAuthorizationCore _authorizationCore = authorizationCore;

        #region IAuthorizationCore

        /// <summary>
        /// Returns a list of role assignments for the specified instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>The list of all role assignments for the specified instance.</returns>
        [HttpPost("query")]
        public IActionResult GetRoleAssignments(string instanceId, [FromBody] RoleAssignmentQueryParameters queryParameters) =>
            new OkObjectResult(_authorizationCore.GetRoleAssignments(instanceId, queryParameters));

        /// <summary>
        /// Retrieves a role assignment.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentName">The role assignment object identifier.</param>
        /// <returns>The role assignment result.</returns>
        [HttpGet("{*roleAssignmentName}")]
        public IActionResult GetRoleAssignment(string instanceId, string roleAssignmentName)
        {
            var roleAssignment = _authorizationCore.GetRoleAssignment(instanceId, roleAssignmentName);
            return roleAssignment is not null
                ? new OkObjectResult(roleAssignment)
                : new NotFoundResult();
        }

        /// <summary>
        /// Assigns a role to an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentCreateRequest">The role assignment create request.</param>
        /// <returns>The role assignment result.</returns>
        [HttpPost]
        public async Task<IActionResult> AssignRole(string instanceId, RoleAssignmentCreateRequest roleAssignmentCreateRequest) =>
            new OkObjectResult(await _authorizationCore.CreateRoleAssignment(instanceId, roleAssignmentCreateRequest));

        /// <summary>
        /// Revokes a role from an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentName">The role assignment object identifier.</param>
        /// <returns>The role assignment result.</returns>
        [HttpDelete("{*roleAssignmentName}")]
        public async Task<IActionResult> RevokeRoleAssignment(string instanceId, string roleAssignmentName) =>
            new OkObjectResult(await _authorizationCore.DeleteRoleAssignment(instanceId, roleAssignmentName));

        #endregion
    }
}
