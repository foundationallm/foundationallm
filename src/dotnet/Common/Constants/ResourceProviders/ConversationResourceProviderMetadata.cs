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
                    ConversationResourceTypeNames.Conversations)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<Conversation>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(Conversation)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ]
                }
            }
        };
    }
}
