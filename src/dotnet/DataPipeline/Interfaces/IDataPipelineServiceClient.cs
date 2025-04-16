using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipeline.Interfaces
{
    /// <summary>
    /// Defines the interface for Data Pipeline API clients.
    /// </summary>
    public interface IDataPipelineServiceClient
    {
        /// <summary>
        /// Gets a data pipeline run by its identifier.
        /// </summary>
        /// <param name="dataPipelineRunId">The data pipeline run identifier.</param>
        /// <returns>The requested data pipeline run object.</returns>
        Task<DataPipelineRun> GetDataPipelineRunAsync(
            string dataPipelineRunId);

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="dataPipelineRun">The data pipeline run to create.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun> CreateDataPipelineRunAsync(
            DataPipelineRun dataPipelineRun);
    }
}
