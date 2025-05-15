using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TextExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            var workItems = contentItems
                .Select(ci => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRunId,
                    Stage = dataPipelineStageName,
                    InputArtifactId = ci.ContentIdentifier.CanonicalId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        /// <inheritdoc/>
        public override async Task<PluginResult<string>> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!string.IsNullOrWhiteSpace(dataPipelineRunWorkItem.PreviousStage))
                throw new PluginException(
                    $"The plugin {Name} can only be used for data pipeline starting stages.");

            var dataSourcePluginName = ResourcePath.GetResourcePath(
                dataPipelineDefinition.DataSource.PluginObjectId).ResourceId;

            var dataSourcePlugin =
                _packageManager.GetDataSourcePlugin(
                    dataSourcePluginName!,
                    dataPipelineDefinition.DataSource.DataSourceObjectId,
                    dataPipelineRun.TriggerParameterValues.FilterKeys(
                        "DataSource."),
                    _serviceProvider);

            //dataSourcePlugin.

            return new PluginResult<string>(
                string.Empty,
                true,
                false);
        }
    }
}
