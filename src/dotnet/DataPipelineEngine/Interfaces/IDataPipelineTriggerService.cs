using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the Data Pipeline Trigger service.
    /// </summary>
    public interface IDataPipelineTriggerService
    {
        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The data pipeline run to create.</param>
        /// <param name="dataPipelineSnapshot">The snapshot of the definition of the data pipeline at the time the run was triggered.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun?> TriggerDataPipeline(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            DataPipelineDefinitionSnapshot dataPipelineSnapshot,
            UnifiedUserIdentity userIdentity);
    }
}
