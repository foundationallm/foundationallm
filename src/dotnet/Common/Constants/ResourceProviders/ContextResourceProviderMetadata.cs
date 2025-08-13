using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Context resource provider.
    /// </summary>
    public static class ContextResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                ContextResourceTypeNames.KnowledgeUnits,
                new ResourceTypeDescriptor(
                    ContextResourceTypeNames.KnowledgeUnits,
                    typeof(KnowledgeUnit))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<KnowledgeUnit>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(KnowledgeUnit)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = []
                }
            },
            {
                ContextResourceTypeNames.KnowledgeSources,
                new ResourceTypeDescriptor(
                    ContextResourceTypeNames.KnowledgeSources,
                    typeof(KnowledgeSource))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<KnowledgeSource>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(KnowledgeSource)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Query, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(ContextKnowledgeSourceQueryRequest)], [typeof(ContextKnowledgeSourceQueryResponse)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.RenderGraph, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(ContextKnowledgeSourceQueryRequest)], [typeof(ContextKnowledgeSourceRenderGraphResponse)])
                        ])
                    ]
                }
            }
        };
    }
}
