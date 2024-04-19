using FoundationaLLM.Authorization.Constants;
using FoundationaLLM.Authorization.Models;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Authorization.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Authorization resource provider.
    /// </summary>
    public class AuthorizationResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                AuthorizationResourceTypeNames.RoleAssignments,
                new ResourceTypeDescriptor(
                        AuthorizationResourceTypeNames.RoleAssignments)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(RoleAssignment)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(RoleAssignment)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], [])
                    ],
                    Actions = []
                }
            }
        };
    }
}
