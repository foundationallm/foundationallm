using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.DataSource resource provider.
    /// </summary>
    public static class DataSourceResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                DataSourceResourceTypeNames.DataSources,
                new ResourceTypeDescriptor(
                        DataSourceResourceTypeNames.DataSources)
                {
                    AllowedTypes = [
                            new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ResourceProviderGetResult<DataSourceBase>)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(DataSourceBase)], [typeof(ResourceProviderUpsertResult)]),
                            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(DataSourceResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ]),
                            new ResourceTypeAction(DataSourceResourceProviderActions.Filter, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceFilter)], [typeof(DataSourceBase)])
                            ]),
                            new ResourceTypeAction(DataSourceResourceProviderActions.Purge, true, false, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [], [typeof(ResourceProviderActionResult)])
                            ])
                        ]
                }
            }
        };
    }
}
