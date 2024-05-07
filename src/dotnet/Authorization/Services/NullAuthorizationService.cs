using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;

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

        public async Task<Dictionary<string, ResourceProviderGetResult>> ProcessGetRolesWithActions(string instanceId, GetRolesWithActionsRequest request)
        {
            var defaultResults = request.Scopes.Distinct().ToDictionary(scp => scp, res => new ResourceProviderGetResult() { Actions = [], Roles = [] });

            await Task.CompletedTask;
            return defaultResults;
        }
    }
}
