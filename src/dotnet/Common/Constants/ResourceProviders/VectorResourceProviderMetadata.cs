using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public static class VectorResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                VectorResourceTypeNames.VectorDatabases,
                new ResourceTypeDescriptor(
                    VectorResourceTypeNames.VectorDatabases,
                    typeof(VectorizationPipeline))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<VectorDatabase>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Vector_Databases_Contributor}", [], [typeof(VectorDatabase)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ],
                    SubTypes = new()
                    {
                        {
                            VectorResourceTypeNames.VectorStores,
                            new ResourceTypeDescriptor (
                                VectorResourceTypeNames.VectorStores,
                                typeof(VectorizationPipelineExecution))
                            {
                                AllowedTypes = [],
                                Actions = []
                            }
                        }
                    }
                }
            }
        };
    }
}
