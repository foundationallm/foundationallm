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
                AzureOpenAIResourceTypeNames.ConversationMappings,
                new ResourceTypeDescriptor(
                    AzureOpenAIResourceTypeNames.ConversationMappings,
                    typeof(AzureOpenAIConversationMapping))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AzureOpenAIConversationMapping>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AzureOpenAIConversationMapping)], [typeof(AzureOpenAIConversationMappingUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ]
                }
            },
            {
                AzureOpenAIResourceTypeNames.FileMappings,
                new ResourceTypeDescriptor(
                    AzureOpenAIResourceTypeNames.FileMappings,
                    typeof(AzureOpenAIFileMapping))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AzureOpenAIFileMapping>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AzureOpenAIFileMapping)], [typeof(ResourceProviderUpsertResult<AzureOpenAIFileMapping>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.LoadFileContent, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderActionResult<FileContent>)])
                        ])
                    ]
                }
            }
        };    

    }
}
