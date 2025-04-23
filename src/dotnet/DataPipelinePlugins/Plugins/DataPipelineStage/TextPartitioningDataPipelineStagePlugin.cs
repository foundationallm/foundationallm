using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Partitioning Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    public class TextPartitioningDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataPipelineStagePlugin
    {
        protected override string Name => PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            await Task.CompletedTask;
            throw new DataPipelineException(
                $"The {nameof(TextExtractionDataPipelineStagePlugin)} data pipeline stage plugin cannot be used for a starting stage.");
        }
    }
}
