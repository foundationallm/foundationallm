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
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Knowledge_Units_Contributor}", [], [typeof(KnowledgeUnit)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Knowledge_Units_Contributor}", [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.CheckVectorStoreId, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Knowledge_Units_Contributor}", [], [typeof(CheckVectorStoreIdRequest)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.SetGraph, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(ContextKnowledgeUnitSetGraphRequest)], [typeof(ResourceProviderActionResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.LoadGraph, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderActionResult<KnowledgeGraph>)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.RenderGraph, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(ContextKnowledgeSourceQueryRequest)], [typeof(ContextKnowledgeUnitRenderGraphResponse)])
                        ])
                    ]
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
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Knowledge_Sources_Contributor}", [], [typeof(KnowledgeUnit)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Knowledge_Sources_Contributor}", [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Query, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(ContextKnowledgeSourceQueryRequest)], [typeof(ContextKnowledgeSourceQueryResponse)])
                        ])
                    ]
                }
            },
            {
                ContextResourceTypeNames.Files,
                new ResourceTypeDescriptor(
                    ContextResourceTypeNames.Files,
                    typeof(ResourceBase))
                {
                    AllowedTypes = [],
                    Actions = []
                }
            }
        };
    }
}
