﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Services
{
    /// <summary>
    /// Implements an authorization service that bypasses the Authorization API and allows all access by default.
    /// </summary>
    public class NullAuthorizationService : IAuthorizationService
    {
        /// <inheritdoc/>
        public async Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            ActionAuthorizationRequest authorizationRequest,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = authorizationRequest.ResourcePaths.Distinct().ToDictionary(rp => rp, auth => true);

            await Task.CompletedTask;
            return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentResult> ProcessRoleAssignmentRequest(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return new RoleAssignmentResult { Success = true };
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, RoleAssignmentsWithActionsResult>> ProcessRoleAssignmentsWithActionsRequest(
            string instanceId,
            RoleAssignmentsWithActionsRequest request,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = request.Scopes.Distinct().ToDictionary(scp => scp, res => new RoleAssignmentsWithActionsResult() { Actions = [], Roles = [] });

            await Task.CompletedTask;
            return defaultResults;
        }

        /// <inheritdoc/>
        public async Task<List<object>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return [];
        }

        public async Task<RoleAssignmentResult> RevokeRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return new RoleAssignmentResult { Success = true };
        }
    }
}
