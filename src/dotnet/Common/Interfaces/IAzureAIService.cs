using FoundationaLLM.Common.Models.AzureAIService;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Interface for the Azure AI service.
    /// </summary>
    public interface IAzureAIService
    {
        /// <summary>
        /// Creates a new data set.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<string> CreateDataSet(InputsMapping data, string blobName);
        /// <summary>
        /// Creates a data set version request.
        /// </summary>
        /// <param name="dataSetName"></param>
        /// <param name="dataSetPath"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<DataVersionResponse> CreateDataSetVersion(string dataSetName, string dataSetPath, int version = 1);
        /// <summary>
        /// Submits a job to the Azure AI service.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="dataSetName"></param>
        /// <param name="dataSetVersion"></param>
        /// <param name="metrics"></param>
        /// <returns></returns>
        Task<Guid> SubmitJob(string displayName, string dataSetName, int dataSetVersion, string metrics);
        /// <summary>
        /// Retrieves the status of a job.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<string> GetJobStatus(Guid jobId);
        /// <summary>
        /// Retrieves the results of a job by index.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        Task<string> GetResultsByIndex(Guid jobId, int startIndex = 0, int endIndex = 149);
        /// <summary>
        /// Downloads the results of a job.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<string> DownloadResults(Guid jobId);
    }
}
