using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines the interface for Data Pipeline API clients.
    /// </summary>
    public interface IDataPipelineServiceClient
    {
        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="dataPipelineRun">The </param>
        /// <returns></returns>
        Task<DataPipelineRun> CreateDataPipelineRunAsync(
            DataPipelineRun dataPipelineRun);
    }
}
