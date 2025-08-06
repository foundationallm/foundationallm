using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Services.Runners;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the data pipeline runner service.
    /// </summary>
    public interface IDataPipelineRunnerService
    {
        /// <summary>
        /// Gets the current data pipeline runners that are processing runs.
        /// </summary>
        Dictionary<string, DataPipelineRunner> CurrentRunners { get; }

        /// <summary>
        /// Inidicates whether the specified data pipeline run can be started.
        /// </summary>
        /// <param name="dataPipelineRun">The data pipeline run to start.</param>
        /// <returns><see langword="true"/> if the specified data pipeline run can be started, <see langword="false"/> otherwise.</returns>
        Task<bool> CanStartRun(
            DataPipelineRun dataPipelineRun);

        /// <summary>
        /// Starts a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineRun">The data pipeline run to start.</param>
        /// <param name="contentItems">The list of content items to process.</param>
        /// <param name="dataPipelineDefinition">The snapshot of the definition of the data pipeline at the time the run was triggered.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The started data pipeline run.</returns>
        Task<DataPipelineRun> StartRun(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            DataPipelineDefinition dataPipelineDefinition,
            UnifiedUserIdentity userIdentity);
    }
}
