using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextPartitioning
{
    /// <summary>
    /// Implements the Semantic Content Text Partitioning plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class SemanticContentTextPartitioningPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IContentTextPartitioningPlugin
    {
        protected override string Name => PluginNames.SEMANTIC_CONTENTTEXTPARTITIONING;

        /// <inheritdoc/>
        public async Task<PluginResult<List<DataPipelineContentItemPart>>> PartitionText(
            string contentItemCanonicalId,
            string text) =>
            throw new NotImplementedException();
    }
}
