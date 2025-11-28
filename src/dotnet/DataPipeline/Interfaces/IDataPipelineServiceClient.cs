using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipeline.Interfaces
{
    /// <summary>
    /// Defines the interface for Data Pipeline API clients.
    /// </summary>
    public interface IDataPipelineServiceClient : IResourceProviderClient
    {
        /// <summary>
        /// Gets a data pipeline run by its identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineName"> The name of the data pipeline.</param>
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The requested data pipeline run object.</returns>
        Task<DataPipelineRun?> GetDataPipelineRunAsync(
            string instanceId,
            string dataPipelineName,
            string runId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves a list of data pipeline runs filtered by the provided filter criteria.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRunFilter">The definition of the filter criteria.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The requested list of data pipeline run objects.</returns>
        Task<DataPipelineRunFilterResponse> GetDataPipelineRunsAsync(
            string instanceId,
            DataPipelineRunFilter dataPipelineRunFilter,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The data pipeline run to create.</param>
        /// <param name="dataPipelineSnapshot">The snapshot of the definition of the data pipeline at the time the run was triggered.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun?> CreateDataPipelineRunAsync(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            DataPipelineDefinitionSnapshot dataPipelineSnapshot,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the current state of the Data Pipeline service.
        /// </summary>
        /// <returns>The state of the service in binary format.</returns>
        Task<BinaryData> GetServiceStateAsync();
    }
}
