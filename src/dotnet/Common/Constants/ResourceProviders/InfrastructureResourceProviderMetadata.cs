using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Infrastructure;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Infrastructure resource provider.
    /// </summary>
    public static class InfrastructureResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                InfrastructureResourceTypeNames.AzureContainerAppsEnvironments,
                new ResourceTypeDescriptor(
                        InfrastructureResourceTypeNames.AzureContainerAppsEnvironments,
                        typeof(AzureContainerAppsEnvironment))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AzureContainerAppsEnvironment>)])
                    ],
                    Actions = [],
                    SubTypes = new()
                    {
                        {
                            InfrastructureResourceTypeNames.AzureContainerApps,
                            new ResourceTypeDescriptor(
                                InfrastructureResourceTypeNames.AzureContainerApps,
                                typeof(AzureContainerApp))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AzureContainerApp>)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AzureContainerApp)], [typeof(ResourceProviderUpsertResult)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                                ],
                                Actions = [
                                    new ResourceTypeAction(ResourceProviderActions.Restart, true, false, [
                                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [], [typeof(ResourceProviderActionResult)])
                                    ]),
                                    new ResourceTypeAction(ResourceProviderActions.Scale, true, false, [
                                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(InfrastructureScaleRequest)], [typeof(ResourceProviderActionResult)])
                                    ])
                                ]
                            }
                        }
                    }
                }
            },
            {
                InfrastructureResourceTypeNames.AzureKubernetesServices,
                new ResourceTypeDescriptor(
                        InfrastructureResourceTypeNames.AzureKubernetesServices,
                        typeof(AzureKubernetesService))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AzureKubernetesService>)])
                    ],
                    Actions = [],
                    SubTypes = new()
                    {
                        {
                            InfrastructureResourceTypeNames.AzureKubernetesServiceDeployments,
                            new ResourceTypeDescriptor(
                                InfrastructureResourceTypeNames.AzureKubernetesServiceDeployments,
                                typeof(AzureKubernetesServiceDeployment))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AzureKubernetesServiceDeployment>)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AzureKubernetesServiceDeployment)], [typeof(ResourceProviderUpsertResult)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                                ],
                                Actions = [
                                    new ResourceTypeAction(ResourceProviderActions.Restart, true, false, [
                                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [], [typeof(ResourceProviderActionResult)])
                                    ]),
                                    new ResourceTypeAction(ResourceProviderActions.Scale, true, false, [
                                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(InfrastructureScaleRequest)], [typeof(ResourceProviderActionResult)])
                                    ])
                                ]
                            }
                        }
                    }
                }
            }
        };
    }
}
