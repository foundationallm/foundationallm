using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Conversation resource provider.
    /// </summary>
    public static class ConversationResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                ConversationResourceTypeNames.Conversations,
                new ResourceTypeDescriptor(
                    ConversationResourceTypeNames.Conversations,
                    typeof(Conversation))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<Conversation>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(Conversation)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], []),
                    ]
                }
            }
        };
    }
}
