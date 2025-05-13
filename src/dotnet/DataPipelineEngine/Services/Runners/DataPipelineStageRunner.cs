using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;

namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    /// <param name="stageName">The name of the data pipeline stage that is run.</param>
    /// <param name="stateService">The Data Pipeline State service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineStageRunner(
        string stageName,
        IDataPipelineStateService stateService,
        ILogger<DataPipelineStageRunner> logger)
    {
        private readonly string _stageName = stageName;
        private readonly IDataPipelineStateService _stateService = stateService;
        private readonly ILogger<DataPipelineStageRunner> _logger = logger;

        private readonly Dictionary<string, DataPipelineRunWorkItemStatus> _workItemsStatus = [];

        public string StageName => _stageName;

        public bool Completed => _workItemsStatus.Values.All(status => status.Completed);

        public bool Successful => _workItemsStatus.Values.All(status => status.Successful);

        public List<String> OutputArtifactIds =>
            [.. _workItemsStatus.Values
                .Where(status => status.OutputArtifactId != null)
                .Select(status => status.OutputArtifactId!)];

        public async Task InitializeNew(
            List<DataPipelineRunWorkItem> workItems)
        {
            if (workItems.Count == 0)
                return;

            var initializationSuccessful =
                await _stateService.PersistDataPipelineRunWorkItems(workItems);

            if (!initializationSuccessful)
                throw new DataPipelineServiceException(
                    $"Failed to initialize state for data pipeline run {workItems.First().RunId}.");

            _workItemsStatus.Clear();
            _workItemsStatus.AddRange(workItems.ToDictionary(
                workItem => workItem.Id,
                workItem => new DataPipelineRunWorkItemStatus()));
        }

        public void InitializeExisting(
            List<DataPipelineRunWorkItem> workItems)
        {
            if (workItems.Count == 0)
                return;

            _workItemsStatus.Clear();
            _workItemsStatus.AddRange(workItems.ToDictionary(
                workItem => workItem.Id,
                workItem => new DataPipelineRunWorkItemStatus
                {
                    Completed = workItem.Completed,
                    Successful = workItem.Successful
                }));
        }

        public async Task SetFailedWorkItems(
            List<DataPipelineRunWorkItem> workItems)
        {
            foreach (var workItem in workItems)
            {
                _workItemsStatus[workItem.Id].Completed = true;
                _workItemsStatus[workItem.Id].Successful = false;
            }

            var updateSuccessful =
                await _stateService.UpdateDataPipelineRunWorkItemsStatus(workItems);

            if (!updateSuccessful)
                throw new DataPipelineServiceException(
                    $"Failed to update state of failed work items for data pipeline run {workItems.First().RunId}.");
        }
    }
}
