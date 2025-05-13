using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the Data Pipeline State Service.
    /// </summary>
    public interface IDataPipelineStateService
    {
        /// <summary>
        /// Initializes the state of a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineRun">The details of the data pipeline run.</param>
        /// <param name="contentItems">The list of content items to be processed by the data pipeline run.</param>
        /// <returns><see langword="true"/> if the initialization is successful.</returns>
        Task<bool> InitializeDataPipelineRunState(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems);

        /// <summary>
        /// Gets a data pipeline run by its identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The requested data pipeline run object.</returns>
        Task<DataPipelineRun?> GetDataPipelineRun(
            string instanceId,
            string runId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Persists a list of data pipeline run work items.
        /// </summary>
        /// <param name="workItems">The list of data pipeline work items to be persisted.</param>
        /// <returns><see langword="true"/> if the items are successfully persisted.</returns>
        Task<bool> PersistDataPipelineRunWorkItems(
            List<DataPipelineRunWorkItem> workItems);

        /// <summary>
        /// Updates the status of data pipeline run work items.
        /// </summary>
        /// <param name="workItems">The list of data pipeline work items whose status must be updated.</param>
        Task UpdateDataPipelineRunWorkItemsStatus(
            List<DataPipelineRunWorkItem> workItems);
    }
}
