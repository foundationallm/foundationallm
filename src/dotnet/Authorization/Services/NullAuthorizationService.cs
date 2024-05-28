using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Services
{
    /// <summary>
    /// Implements an authorization service that bypasses the Authorization API and allows all access by default.
    /// </summary>
    public class NullAuthorizationService : IAuthorizationService
    {
        /// <inheritdoc/>
        public async Task<ActionAuthorizationResult> ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest)
        {
            var defaultResults = authorizationRequest.ResourcePaths.Distinct().ToDictionary(rp => rp, auth => true);

            await Task.CompletedTask;
            return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
        }

        public async Task<RoleAssignmentResult> ProcessRoleAssignmentRequest(string instanceId, RoleAssignmentRequest roleAssignmentRequest)
        {
            await Task.CompletedTask;
            return new RoleAssignmentResult { Success = true };
        }

        public async Task<Dictionary<string, RoleAssignmentsWithactionsResult>> ProcessRoleAssignmentsWithActionsRequest(string instanceId, RoleAssignmentsWithActionsRequest request)
        {
            var defaultResults = request.Scopes.Distinct().ToDictionary(scp => scp, res => new RoleAssignmentsWithactionsResult() { Actions = [], Roles = [] });

            await Task.CompletedTask;
            return defaultResults;
        }
    }
}
