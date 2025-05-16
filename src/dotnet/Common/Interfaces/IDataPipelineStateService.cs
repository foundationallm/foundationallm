using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using System.ClientModel;

namespace FoundationaLLM.Common.Interfaces
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
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <returns>The requested data pipeline run object.</returns>
        Task<DataPipelineRun?> GetDataPipelineRun(
            string runId);

        /// <summary>
        /// Gets a data pipeline run work item by its identifier.
        /// </summary>
        /// <param name="workItemId">The data pipeline run work item identifier.</param>
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <returns>The requests data pipeline run work item object.</returns>
        Task<DataPipelineRunWorkItem?> GetDataPipelineRunWorkItem(
            string workItemId,
            string runId);

        /// <summary>
        /// Updates the status of a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineRun">The data pipeline run whose status is to be updated.</param>
        /// <returns><see langword="true"/> if the status update is successful.</returns>
        Task<bool> UpdateDataPipelineRunStatus(
            DataPipelineRun dataPipelineRun);

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
        /// <returns><see langword="true"/> if the items statuses are successfully updated.</returns>
        Task<bool> UpdateDataPipelineRunWorkItemsStatus(
            List<DataPipelineRunWorkItem> workItems);

        /// <summary>
        /// Updates a data pipeline run work item.
        /// </summary>
        /// <param name="workItem">The data pipeline run work item to be updated.</param>
        /// <returns><see langword="true"/> if the data pipeline run work item is successfully updated.</returns>
        Task<bool> UpdateDataPipelineRunWorkItem(
            DataPipelineRunWorkItem workItem);

        /// <summary>
        /// Gets a list of active data pipeline runs.
        /// </summary>
        /// <returns>The list of active data pipeline runs.</returns>
        Task<List<DataPipelineRun>> GetActiveDataPipelineRuns();

        /// <summary>
        /// Gets the list of data pipeline run work items associated with a specified stage of a run.
        /// </summary>
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <param name="stage">The stage of the data pipeline run.</param>
        /// <returns>The list of data pipeline run work items associated with the specified stage of the run.</returns>
        Task<List<DataPipelineRunWorkItem>> GetDataPipelineRunStageWorkItems(
            string runId,
            string stage);

        /// <summary>
        /// Loads the artifacts associated with a data pipeline run work item.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <returns>A dictionary with the names and binary contents of the artifacts.</returns>
        Task<Dictionary<string, BinaryData>> LoadDataPipelineRunWorkItemArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem);

        /// <summary>
        /// Saves the artifacts associated with a data pipeline run work item.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <param name="artifacts">The dictionary with the names and binary contents of the artifacts.</param>
        /// <returns></returns>
        Task SaveDataPipelineRunWorkItemArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            Dictionary<string, BinaryData> artifacts);

        /// <summary>
        /// Starts processing data pipeline run work items.
        /// </summary>
        /// <param name="processWorkItem">The asynchronous delegate that is invoked for each data pipeline run work item.</param>
        /// <returns><see langword="true"/> if the processing is successfully started.</returns>
        Task<bool> StartDataPipelineRunWorkItemProcessing(
            Func<DataPipelineRunWorkItem, Task> processWorkItem);

        /// <summary>
        /// Stops processing data pipeline run work items.
        /// </summary>
        Task StopDataPipelineRunWorkItemProcessing();
    }
}
