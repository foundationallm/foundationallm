using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Configuration resource provider.
    /// </summary>
    public static class ConfigurationResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                ConfigurationResourceTypeNames.AppConfigurations,
                new ResourceTypeDescriptor(
                        ConfigurationResourceTypeNames.AppConfigurations,
                        typeof(AppConfigurationKeyBase))
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<AppConfigurationKeyBase>)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(AgentBase)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            }
            ,
            {
                ConfigurationResourceTypeNames.APIEndpointConfigurations,
                new ResourceTypeDescriptor(
                        ConfigurationResourceTypeNames.APIEndpointConfigurations,
                        typeof(APIEndpointConfiguration))
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<APIEndpointConfiguration>)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(APIEndpointConfiguration)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ]
                }
            }
        };
    }
}
