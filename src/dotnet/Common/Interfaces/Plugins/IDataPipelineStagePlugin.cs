using FoundationaLLM.Common.Models.DataPipelines;

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
    }
}
