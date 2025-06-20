using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE;

        private const string CONTENT_PARTS_FILE_NAME = "content-parts.parquet";
        private const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            var contentItemContentParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                CONTENT_PARTS_FILE_NAME);

            var contentItemKnowledgeParts = contentItemContentParts
                .Select(p =>
                {
                    var knowledgePart = DataPipelineContentItemKnowledgePart.FromContentItemPart(p);
                    knowledgePart.Entities = "N/A";
                    return knowledgePart;
                })
                .ToList();

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemKnowledgeParts,
                KNOWLEDGE_PARTS_FILE_NAME);

            return
                new PluginResult(true, false);
        }
    }
}
