using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Defines the interface for a data pipeline stage plugin.
    /// </summary>
    public interface IDataPipelineStagePlugin
    {
        /// <summary>
        /// Gets the list of data pipeline work items based on the provided content items.
        /// </summary>
        /// <param name="contentItems">The list of content items.</param>
        /// <param name="dataPipelineRunId">The unique identifier of the data pipeline run.</param>
        /// <param name="dataPipelineStageName">The name of the data pipeline stage.</param>
        /// <returns>A list of data pipeline work items.</returns>
        Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName);

        /// <summary>
        /// Gets the list of data pipeline work items based on the provided inbound artifact identifiers
        /// </summary>
        /// <param name="inboundArtifactIds">The list of inbound artifact identifiers.</param>
        /// <param name="dataPipelineRunId">The unique identifier of the data pipeline run.</param>
        /// <param name="dataPipelineStageName">The name of the data pipeline stage.</param>
        /// <param name="previousDataPipelineStageName">The name of the previous data pipeline stage.</param>
        /// <returns>A list of data pipeline work items.</returns>
        Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            List<string> inboundArtifactIds,
            string dataPipelineRunId,
            string dataPipelineStageName,
            string previousDataPipelineStageName);

        /// <summary>
        /// Processes a specified data pipeline run work item.
        /// </summary>
        /// <param name="dataPipelineDefinition">The definition of the data pipeline associated with the data pipeline run work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run associated with the data pipeline run work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item to process.</param>
        /// <returns>An object that contains the processing result and an indicator of success.</returns>
        Task<PluginResult<string>> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem);
    }
}
