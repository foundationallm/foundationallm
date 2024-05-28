﻿using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Interfaces
{
    /// <summary>
    /// Defines the methods for authorization core.
    /// </summary>
    public interface IAuthorizationCore
    {
        /// <summary>
        /// Processes an authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> containing the details of the authorization request.</param>
        /// <returns>An <see cref="ActionAuthorizationResult"/> indicating whether the requested authorization was successfull or not for each resource path.</returns>
        ActionAuthorizationResult ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest);

        /// <summary>
        /// Checks if a specified security principal is allowed to process authorization requests. 
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="securityPrincipalId">The id of the security principal whose authorization is checked.</param>
        /// <returns>True if the security principal is allowed to process authorization requests.</returns>
        bool AllowAuthorizationRequestsProcessing(string instanceId, string securityPrincipalId);

        /// <summary>
        /// Assigns a role to an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The role assignment request.</param>
        /// <returns>The role assignment result.</returns>
        Task<RoleAssignmentResult> AssignRole(string instanceId, RoleAssignmentRequest roleAssignmentRequest);

        /// <summary>
        /// Revokes a role from an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment object identifier.</param>
        /// <returns>The role assignment result.</returns>
        Task<RoleAssignmentResult> RevokeRole(string instanceId, string roleAssignment);

        /// <summary>
        /// Returns a list of role names and a list of allowed actions for the specified scope.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="request">The get roles with actions request.</param>
        /// <returns>The get roles and actions result.</returns>
        Dictionary<string, RoleAssignmentsWithactionsResult> ProcessRoleAssignmentsWithActionsRequest(string instanceId, RoleAssignmentsWithActionsRequest request);
    }
}
