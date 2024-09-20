using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.AzureOpenAI resource provider.
    /// </summary>
    public static class AzureOpenAIResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts,
                new ResourceTypeDescriptor(
                    AzureOpenAIResourceTypeNames.AssistantUserContexts,
                    typeof(AssistantUserContext))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AssistantUserContext>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AssistantUserContext)], [typeof(AssistantUserContextUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ]
                }
            },
            {
                AzureOpenAIResourceTypeNames.FileUserContexts,
                new ResourceTypeDescriptor(
                    AzureOpenAIResourceTypeNames.FileUserContexts,
                    typeof(FileUserContext))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<FileUserContext>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(FileUserContext)], [typeof(FileUserContextUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    SubTypes = new()
                    {
                        {
                            AzureOpenAIResourceTypeNames.FilesContent,
                            new ResourceTypeDescriptor (
                                AzureOpenAIResourceTypeNames.FilesContent,
                                typeof(FileContent))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<FileContent>)])
                                ]
                            }
                        }
                    }
                }
            }
        };
    }
}
