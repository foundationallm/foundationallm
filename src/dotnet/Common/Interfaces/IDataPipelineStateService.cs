using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

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
        /// <param name="dataPipelineDefinition"> The data pipeline definition used to initialize the run state.</param>
        /// <param name="dataPipelineRun">The details of the data pipeline run.</param>
        /// <param name="contentItems">The list of content items to be processed by the data pipeline run.</param>
        /// <returns><see langword="true"/> if the initialization is successful.</returns>
        Task<bool> InitializeDataPipelineRunState(
            DataPipelineDefinition dataPipelineDefinition,
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
        /// Gets a list of data pipeline runs filtered by the provided filter criteria.
        /// </summary>
        /// <param name="dataPipelineRunFilter">The filter criteria used to filter data pipeline runs.</param>
        /// <returns>The list of requests data pipeline runs.</returns>
        Task<DataPipelineRunFilterResponse> GetDataPipelineRuns(
            DataPipelineRunFilter dataPipelineRunFilter);

        /// <summary>
        /// Gets a data pipeline content item associated with a data pipeline run work item.
        /// </summary>
        /// <param name="dataPipelineRunWorkItem">The data pipeline work item that references the content item.</param>
        /// <returns>The content item referenced by the work item.</returns>
        Task<DataPipelineContentItem> GetDataPipelineContentItem(
            DataPipelineRunWorkItem dataPipelineRunWorkItem);

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
        /// Tries to load the artifacts associated with a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="artifactsNameFilter">The name pattern used to identify the artifacts to load.</param>
        /// <returns>A boolean indicating whether the operation was successful or not and
        /// the list with the details of the artifacts if the operation is successful.</returns>
        Task<(bool Success, List<DataPipelineStateArtifact> Artifacts)> TryLoadDataPipelineRunArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string artifactsNameFilter);

        /// <summary>
        /// Indicates whether an artifact associated with a data pipeline run work item has changed.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <param name="artifactName">The name of the artifact associated with the data pipeline run work item.</param>
        /// <returns><see langword="true"/> if the artifact was changed by the data pipeline run indicated by
        /// <paramref name="dataPipelineRun"/>, <see langword="false"/> otherwise.</returns>
        Task<bool> DataPipelineRunWorkItemArtifactChanged(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string artifactName);

        /// <summary>
        /// Loads the artifacts associated with a data pipeline run work item.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <param name="artifactsNameFilter">The name pattern used to identify a subset of the artifacts.</param>
        /// <returns>A list with the binary contents of the artifacts.</returns>
        Task<List<DataPipelineStateArtifact>> LoadDataPipelineRunWorkItemArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string artifactsNameFilter);

        /// <summary>
        /// Loads the content item parts associated with a data pipeline run work item.
        /// </summary>
        /// <typeparam name="T">The type of the content item parts to be loaded.</typeparam>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <param name="fileName"> The name of the file that contains the content item parts.</param>
        /// <param name="contentSection">The section in the storage that contains the work item parts. Default is "content-items".</param>
        /// <returns>A list with the content item parts associated with the data pipeline run work item.</returns>
        Task<IEnumerable<T>> LoadDataPipelineRunWorkItemParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string fileName,
            string contentSection = "content-items")
            where T : class, new();

        /// <summary>
        /// Loads the content item parts associated with a data pipeline run work item.
        /// </summary>
        /// <typeparam name="T">The type of the content item parts to be loaded.</typeparam>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="contentItemCanonicalId">The content item canonical identifier.</param>
        /// <param name="fileName"> The name of the file that contains the content item parts.</param>
        /// <param name="contentSection">The section in the storage that contains the work item parts. Default is "content-items".</param>
        /// <returns>A list with the content item parts associated with the data pipeline run work item.</returns>
        Task<IEnumerable<T>> LoadDataPipelineRunWorkItemParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string contentItemCanonicalId,
            string fileName,
            string contentSection = "content-items")
            where T : class, new();

        /// <summary>
        /// Loads the parts associated with a data pipeline run.
        /// </summary>
        /// <typeparam name="T">The type of the data pipeline run part to be loaded.</typeparam>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the run.</param>
        /// <param name="dataPipelineRun">The data pipeline run .</param>
        /// <param name="filePath"> The name of the file that contains the data pipeline run parts.</param>
        /// <returns>A list with the parts associated with the data pipeline run.</returns>
        /// <remarks>
        /// The <paramref name="filePath"/> parameter must contain a path that is relative to the path
        /// of the data pipeline run artifacts directory.
        /// </remarks>
        Task<IEnumerable<T>> LoadDataPipelineRunParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string filePath)
            where T : class, new();

        /// <summary>
        /// Saves the artifacts associated with a data pipeline run work item.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <param name="artifacts">The list with the binary contents of the artifacts.</param>
        /// <returns></returns>
        Task SaveDataPipelineRunWorkItemArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<DataPipelineStateArtifact> artifacts);

        /// <summary>
        /// Saves the artifacts associated with a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the run.</param>
        /// <param name="dataPipelineRun">The data pipeline run.</param>
        /// <param name="artifacts">The list of artifacts to be saved.</param>
        /// <returns></returns>
        Task SaveDataPipelineRunArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<DataPipelineStateArtifact> artifacts);

        /// <summary>
        /// Saves the content item parts associated with a data pipeline run work item.
        /// </summary>
        /// <typeparam name="T">The type of the content item parts to be saved.</typeparam>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the work item.</param>
        /// <param name="dataPipelineRun">The data pipeline run item associated with the work item.</param>
        /// <param name="dataPipelineRunWorkItem">The data pipeline run work item.</param>
        /// <param name="contentItemParts">The list with the content item parts.</param>
        /// <param name="fileName"> The name of the file that contains the content item parts.</param>
        /// <param name="contentSection">The section in the storage that contains the work item parts. Default is "content-items".</param>
        Task SaveDataPipelineRunWorkItemParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            IEnumerable<T> contentItemParts,
            string fileName,
            string contentSection = "content-items")
            where T : class, new();

        /// <summary>
        /// Saves the parts associated with a data pipeline run.
        /// </summary>
        /// <typeparam name="T">The type of the data pipeline part to be saved.</typeparam>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the run.</param>
        /// <param name="dataPipelineRun">The data pipeline run.</param>
        /// <param name="dataPipelineRunParts">The list of data pipeline run parts.</param>
        /// <param name="filePath"> The path of the file that contains the data pipeline run parts.</param>
        /// /// <remarks>
        /// The <paramref name="filePath"/> parameter must contain a path that is relative to the path
        /// of the data pipeline run artifacts directory.
        /// </remarks>
        Task SaveDataPipelineRunParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            IEnumerable<T> dataPipelineRunParts,
            string filePath)
            where T : class, new();

        /// <summary>
        /// Gets the canonical root path that corresponds to a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the run.</param>
        /// <param name="dataPipelineRun">The data pipeline run.</param>
        /// <returns>The path of the canonical root directory.</returns>
        /// <remarks>
        /// The canonical root path is where we store artifacts shared by all individual runs
        /// that are trigerred for the same data pipeline definition and set of trigger parameters.
        /// This includes the content items that are processed by the data pipeline runs.
        /// </remarks>
        string GetDataPipelineCanonicalRootPath(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun);

        /// <summary>
        /// Gets the root path for a data pipeline run.
        /// </summary>
        /// <param name="dataPipelineDefinition">The data pipeline definition associated with the run.</param>
        /// <param name="dataPipelineRun">The data pipeline run.</param>
        /// <returns>The path of the run root directory.</returns>
        /// <remarks>
        /// The run root path is where the run-specific artifacts are stored.
        /// </remarks>
        string GetDataPipelineRunRootPath(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun);

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
