using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Configuration.Constants;
using FoundationaLLM.Configuration.Models;

namespace FoundationaLLM.Configuration.Services
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
                        ConfigurationResourceTypeNames.AppConfigurations)
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(AppConfigurationKeyBase)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(AgentBase)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(ConfigurationResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            }
        };
    }
}
