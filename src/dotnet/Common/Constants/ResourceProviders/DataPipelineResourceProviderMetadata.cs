using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.DataPipeline resource provider.
    /// </summary>
    public static class DataPipelineResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                DataPipelineResourceTypeNames.DataPipelines,
                new ResourceTypeDescriptor(
                    DataPipelineResourceTypeNames.DataPipelines,
                    typeof(DataPipelineDefinition))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<DataPipelineDefinition>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Data_Pipelines_Contributor}!", [], [typeof(DataPipelineDefinition)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Data_Pipelines_Contributor}", [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Activate, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [], [typeof(ResourceProviderActionResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Deactivate, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [], [typeof(ResourceProviderActionResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Trigger, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(DataPipelineTriggerRequest)], [typeof(ResourceProviderActionResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ],
                    SubTypes = new()
                    {
                        {
                            DataPipelineResourceTypeNames.DataPipelineSnapshots,
                            new ResourceTypeDescriptor(
                                DataPipelineResourceTypeNames.DataPipelineSnapshots,
                                typeof(DataPipelineDefinitionSnapshot))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<DataPipelineDefinitionSnapshot>)])
                                ],
                                Actions = []
                            }
                        },
                        {
                            DataPipelineResourceTypeNames.DataPipelineRuns,
                            new ResourceTypeDescriptor (
                                DataPipelineResourceTypeNames.DataPipelineRuns,
                                typeof(DataPipelineRun))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Write, [], [], [typeof(ResourceProviderGetResult<DataPipelineRun>)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(DataPipelineRun)], [typeof(ResourceProviderUpsertResult<DataPipelineRun>)])
                                ],
                                Actions = [
                                    new ResourceTypeAction(ResourceProviderActions.Filter, false, true, [
                                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(DataPipelineRunFilter)], [typeof(DataPipelineRun)]),
                                    ])
                                ]
                            }
                        }
                    }
                }
            }
        };
    }
}
