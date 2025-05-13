using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;

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
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataPipelineStagePlugin
    {
        protected override string Name => PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
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
        public async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            List<string> inboundArtifactIds,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            var workItems = inboundArtifactIds
                .Select(artifactId => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRunId,
                    Stage = dataPipelineStageName,
                    InputArtifactId = artifactId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }
    }
}
