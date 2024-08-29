using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Attachment resource provider.
    /// </summary>
    public static class AttachmentResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                AttachmentResourceTypeNames.Attachments,
                new ResourceTypeDescriptor(
                        AttachmentResourceTypeNames.Attachments)
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<AttachmentFile>)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(AttachmentFile)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Filter, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceFilter)], [typeof(AttachmentDetail)])
                        ])
                    ]
                }
            }
        };
    }
}
