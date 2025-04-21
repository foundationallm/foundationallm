using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the data pipeline runner service.
    /// </summary>
    public interface IDataPipelineRunnerService : IDataPipelineResourceProviderClient
    {
        /// <summary>
        /// Starts a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineRun">The data pipeline run to start.</param>
        /// <param name="contentItems">The list of content items to process.</param>
        /// <param name="dataPipelineDefinition">The snapshot of the definition of the data pipeline at the time the run was triggered.</param>
        /// <returns>The started data pipeline run.</returns>
        Task<DataPipelineRun> StartRun(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            DataPipelineDefinition dataPipelineDefinition);
    }
}
