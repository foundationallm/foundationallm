using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models;
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
            string action,
            List<string> resourcePaths,
            bool expandResourceTypePaths,
            bool includeRoleAssignments,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = resourcePaths.Distinct().ToDictionary(
                rp => rp,
                rp => new ResourcePathAuthorizationResult
                {
                    ResourcePath = rp,
                    Authorized = false
                });

            await Task.CompletedTask;
            return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> CreateRoleAssignment(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return new RoleAssignmentOperationResult { Success = true };
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
        public async Task<List<RoleAssignment>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return [];
        }

        public async Task<RoleAssignmentOperationResult> DeleteRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return new RoleAssignmentOperationResult { Success = true };
        }
    }
}
