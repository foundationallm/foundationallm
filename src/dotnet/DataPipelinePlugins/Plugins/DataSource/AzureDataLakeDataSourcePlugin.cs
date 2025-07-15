using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource
{
    /// <summary>
    /// Implements the Azure Data Lake data source plugin.
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    /// </summary>
    public class AzureDataLakeDataSourcePlugin(
        string dataSourceObjectId,
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataSourcePlugin
    {
        private readonly string _dataSourceObjectId = dataSourceObjectId;

        protected override string Name => PluginNames.AZUREDATALAKE_DATASOURCE;

        /// <inheritdoc/>
        public async Task<List<DataPipelineContentItem>> GetContentItems() => throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            ContentIdentifier contentItemIdentifier) => throw new NotImplementedException();
    }
}
