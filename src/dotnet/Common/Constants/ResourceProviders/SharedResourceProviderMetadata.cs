using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Global;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata shared by all resource providers.
    /// </summary>
    public static class SharedResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by all resource providers.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                SharedResourceTypeNames.Management,
                new ResourceTypeDescriptor(
                        SharedResourceTypeNames.Management,
                        typeof(ResourceBase))
                {
                    AllowedTypes = [],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.TriggerCommand, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Resource_Providers_Administrator}!", [], [typeof(ResourceProviderManagementAction)], [typeof(ResourceProviderActionResult)]),
                        ])
                    ]
                }
            }
        };
    }
}
