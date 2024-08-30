using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public static class VectorizationResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                VectorizationResourceTypeNames.VectorizationPipelines,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.VectorizationPipelines,
                    typeof(VectorizationPipeline))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<VectorizationPipeline>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(VectorizationPipeline)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(VectorizationResourceProviderActions.Activate, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(VectorizationResult)])
                        ]),
                        new ResourceTypeAction(VectorizationResourceProviderActions.Deactivate, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(VectorizationResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ]
                }
            },
            {
                VectorizationResourceTypeNames.VectorizationRequests,
                new ResourceTypeDescriptor(
                        VectorizationResourceTypeNames.VectorizationRequests,
                    typeof(VectorizationRequest))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(VectorizationRequest)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(VectorizationRequest)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(VectorizationResourceProviderActions.Process, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(VectorizationResult)])
                        ])
                    ]
                }
            },
            {
                VectorizationResourceTypeNames.TextPartitioningProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.TextPartitioningProfiles,
                    typeof(TextPartitioningProfile))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<TextPartitioningProfile>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(TextPartitioningProfile)], [typeof(ResourceProviderUpsertResult)]),
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
                VectorizationResourceTypeNames.TextEmbeddingProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.TextEmbeddingProfiles,
                    typeof(TextEmbeddingProfile))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<TextEmbeddingProfile>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(TextEmbeddingProfile)], [typeof(ResourceProviderUpsertResult)]),
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
                VectorizationResourceTypeNames.IndexingProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.IndexingProfiles,
                    typeof(IndexingProfile))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<IndexingProfile>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(IndexingProfile)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ]),
                            new ResourceTypeAction(ResourceProviderActions.Filter, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceFilter)], [typeof(IndexingProfile)])
                            ]),
                            new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(ResourceProviderActionResult)])
                            ])
                        ]
                }
            }
        };
    }
}
