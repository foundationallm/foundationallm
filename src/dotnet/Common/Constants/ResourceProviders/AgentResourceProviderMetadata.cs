using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Agent resource provider.
    /// </summary>
    public static class AgentResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                AgentResourceTypeNames.Agents,
                new ResourceTypeDescriptor(
                        AgentResourceTypeNames.Agents)
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<AgentBase>)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(AgentBase)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(AgentResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ]),
                            new ResourceTypeAction(AgentResourceProviderActions.Purge, true, false, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(ResourceProviderActionResult)])
                            ])
                        ]
                }
            }
        };
    }
}
