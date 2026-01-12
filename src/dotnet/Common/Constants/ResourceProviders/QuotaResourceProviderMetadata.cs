using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.Quota;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Quota resource provider.
    /// </summary>
    public static class QuotaResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                QuotaResourceTypeNames.QuotaDefinitions,
                new ResourceTypeDescriptor(
                        QuotaResourceTypeNames.QuotaDefinitions,
                        typeof(QuotaDefinition))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [], [typeof(ResourceProviderGetResult<QuotaDefinition>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Contributor}", [], [typeof(QuotaDefinition)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, $"{AuthorizableOperations.Delete}|{RoleDefinitionNames.Contributor}", [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Delete}|{RoleDefinitionNames.Contributor}", [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ]
                }
            },
            {
                QuotaResourceTypeNames.QuotaMetrics,
                new ResourceTypeDescriptor(
                        QuotaResourceTypeNames.QuotaMetrics,
                        typeof(QuotaUsageMetrics))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [], [typeof(QuotaUsageMetrics)])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Filter, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [typeof(QuotaMetricsFilter)], [typeof(QuotaUsageMetrics)])
                        ]),
                        new ResourceTypeAction("history", false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [typeof(QuotaHistoryRequest)], [typeof(QuotaUsageHistory)])
                        ])
                    ]
                }
            },
            {
                QuotaResourceTypeNames.QuotaEvents,
                new ResourceTypeDescriptor(
                        QuotaResourceTypeNames.QuotaEvents,
                        typeof(QuotaEventDocument))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [], [typeof(QuotaEventDocument)])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Filter, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [typeof(QuotaEventFilter)], [typeof(QuotaEventDocument)])
                        ]),
                        new ResourceTypeAction("summary", false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Contributor}", [], [typeof(QuotaEventSummaryRequest)], [typeof(QuotaEventSummary)])
                        ])
                    ]
                }
            }
        };
    }
}
