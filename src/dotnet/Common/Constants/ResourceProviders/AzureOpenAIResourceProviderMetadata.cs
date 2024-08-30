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
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<AssistantUserContext>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(AssistantUserContext)], [typeof(AssistantUserContextUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(ResourceProviderActionResult)])
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
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<FileUserContext>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(FileUserContext)], [typeof(FileUserContextUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], [])
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
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<FileContent>)])
                                ]
                            }
                        }
                    }
                }
            }
        };
    }
}
