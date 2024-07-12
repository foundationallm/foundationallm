using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Extends the <see cref="IAuthorizationService"/> interface with helper methods.
    /// </summary>
    public static class AuthorizationServiceExtensions
    {
        /// <summary>
        /// Filters the list of resources based on the authorizable action.
        /// </summary>
        /// <typeparam name="T">The object type of the resource being retrieved.</typeparam>
        /// <param name="authorizationService">The <see cref="IAuthorizationService"/> service.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <param name="resources">The list of all resources.</param>
        /// <param name="authorizableAction">The authorizable action to be checked.</param>
        /// <returns>A list of resources on which the user identity is allowed to perform the authorizable action.</returns>
        public static async Task<List<ResourceProviderGetResult<T>>> FilterResourcesByAuthorizableAction<T>(
             this IAuthorizationService authorizationService,
             string instanceId,
             UnifiedUserIdentity userIdentity,
             List<T> resources,
             string authorizableAction)
             where T : ResourceBase
        {
            var rolesWithActions = await authorizationService.ProcessRoleAssignmentsWithActionsRequest(
                instanceId,
                new RoleAssignmentsWithActionsRequest()
                {
                    Scopes = resources.Select(x => x.ObjectId!).ToList(),
                    PrincipalId = userIdentity.UserId!,
                    SecurityGroupIds = userIdentity.GroupIds
                },
                userIdentity);

            var results = new List<ResourceProviderGetResult<T>>();

            foreach (var resource in resources)
                if (rolesWithActions[resource.ObjectId!].Actions.Contains(authorizableAction))
                    results.Add(new ResourceProviderGetResult<T>()
                    {
                        Resource = resource,
                        Actions = rolesWithActions[resource.ObjectId!].Actions,
                        Roles = rolesWithActions[resource.ObjectId!].Roles
                    });

            return results;
        }
    }
}
