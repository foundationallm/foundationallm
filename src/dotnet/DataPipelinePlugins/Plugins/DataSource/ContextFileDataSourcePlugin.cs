using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource
{
    /// <summary>
    /// Implements the FoundationaLLM Context File data source plugin.
    /// </summary>
    /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class ContextFileDataSourcePlugin(
        string dataSourceObjectId,
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataSourcePlugin
    {
        private readonly string _dataSourceObjectId = dataSourceObjectId;

        protected override string Name => PluginNames.CONTEXTFILE_DATASOURCE;

        public List<DataPipelineContentItem> GetContentItems()
        {
            var contextFileObjectId = _pluginParameters[PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID]?.ToString()
                ?? throw new PluginException($"The {PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID} parameter is required by the {Name} plugin.");

            var canonicalId = contextFileObjectId[(contextFileObjectId.LastIndexOf('/') + 1)..];

            return [new DataPipelineContentItem {
                Id = $"content-item-{canonicalId}-{Guid.NewGuid().ToBase64String()}",
                DataSourceObjectId = _dataSourceObjectId,
                ContentIdentifier = new ContentIdentifier
                {
                    MultipartId = [contextFileObjectId],
                    CanonicalId = canonicalId
                }
            }];
        }
    }
}
