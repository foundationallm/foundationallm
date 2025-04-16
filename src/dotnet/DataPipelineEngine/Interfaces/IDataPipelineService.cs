using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Interface for the Data Pipeline Service.
    /// </summary>
    public interface IDataPipelineService
    {
        /// <summary>
        /// Retrieves a data pipeline run by its name.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRunName">The name of the data pipeline run.</param>
        /// <returns>The data pipeline run identified by the provided name.</returns>
        Task<DataPipelineRun> GetDataPipelineRun(
            string instanceId,
            string dataPipelineRunName);

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The object with the properties of the new data pipeline run.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun> CreateDataPipelineRun(
            string instanceId,
            DataPipelineRun dataPipelineRun);
    }
}
