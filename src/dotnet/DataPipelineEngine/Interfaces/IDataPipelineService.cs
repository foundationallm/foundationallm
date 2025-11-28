using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Interface for the Data Pipeline Service.
    /// </summary>
    public interface IDataPipelineService
    {
        /// <summary>
        /// Retrieves a list of data pipeline runs filtered by the provided filter criteria.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRunFilter">The definition of the filter criteria.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The requested list of data pipeline run objects.</returns>
        Task<DataPipelineRunFilterResponse> GetDataPipelineRuns(
            string instanceId,
            DataPipelineRunFilter dataPipelineRunFilter,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves a data pipeline run by its name.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineName"> The name of the data pipeline.</param>
        /// <param name="runId">The identifier of the data pipeline run.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The data pipeline run identified by the provided identifier.</returns>
        Task<DataPipelineRun> GetDataPipelineRun(
            string instanceId,
            string dataPipelineName,
            string runId,
            UnifiedUserIdentity userIdentiy);

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The object with the properties of the new data pipeline run.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun> CreateDataPipelineRun(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            UnifiedUserIdentity userIdentity);
    }
}
