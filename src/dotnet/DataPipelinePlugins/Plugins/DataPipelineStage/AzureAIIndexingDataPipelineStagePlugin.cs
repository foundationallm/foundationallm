using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Azure AI Indexing Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class AzureAIIndexingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataPipelineStagePlugin
    {
        protected override string Name => PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            await Task.CompletedTask;
            throw new DataPipelineException(
                $"The {nameof(AzureAIIndexingDataPipelineStagePlugin)} data pipeline stage plugin cannot be used for a starting stage.");
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
