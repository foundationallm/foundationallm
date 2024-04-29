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
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> to process.</param>
        /// <returns>An <see cref="ActionAuthorizationResult"/> containing the result of the processing.</returns>
        Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            ActionAuthorizationRequest authorizationRequest);

        /// <summary>
        /// Processes a role assignment request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The role assignment request.</param>
        /// <returns></returns>
        Task<RoleAssignmentResult> ProcessRoleAssignmentRequest(string instanceId, RoleAssignmentRequest roleAssignmentRequest);
    }
}
