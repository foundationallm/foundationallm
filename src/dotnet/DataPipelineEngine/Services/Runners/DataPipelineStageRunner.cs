using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;

namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    /// <param name="stageName">The name of the data pipeline stage that is run.</param>
    /// <param name="stageRunStartTime">The start time of the stage run.</param>
    /// <param name="stateService">The Data Pipeline State service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineStageRunner(
        string stageName,
        DateTimeOffset? stageRunStartTime,
        IDataPipelineStateService stateService,
        ILogger<DataPipelineStageRunner> logger)
    {
        private readonly string _stageName = stageName;
        private readonly IDataPipelineStateService _stateService = stateService;
        private readonly ILogger<DataPipelineStageRunner> _logger = logger;
        private readonly DateTimeOffset _stageRunStartTime =
            stageRunStartTime ?? DateTimeOffset.UtcNow;

        private readonly Dictionary<string, DataPipelineRunWorkItemStatus> _workItemsStatus = [];
        private readonly object _syncRoot = new();

        public string StageName => _stageName;

        public bool Completed => _workItemsStatus.Values.All(status => status.Completed);

        public bool Successful => _workItemsStatus.Values.All(status => status.Successful);

        public int WorkItemsCount => _workItemsStatus.Count;

        public int CompletedWorkItemsCount =>
            _workItemsStatus.Values.Count(status => status.Completed);

        public int SuccessfulWorkItemsCount =>
            _workItemsStatus.Values.Count(status => status.Successful);

        public List<string> ContentItemsCanonicalIds =>
            [.. _workItemsStatus.Values.Select(status => status.ContentItemCanonicalId)];

        public bool Changed
        {
            get
            {
                lock (_syncRoot)
                {
                    return _workItemsStatus.Values.Any(status => status.Changed);
                }
            }
        }

        public DateTimeOffset StageRunStartTime => _stageRunStartTime;

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
                workItem => new DataPipelineRunWorkItemStatus
                {
                    ContentItemCanonicalId = workItem.ContentItemCanonicalId,
                    Changed = false
                }));
        }

        public void InitializeExisting(
            List<DataPipelineRunWorkItem> workItems)
        {
            if (workItems.Count == 0)
                return;

            lock (_syncRoot)
            {
                _workItemsStatus.Clear();
                _workItemsStatus.AddRange(workItems.ToDictionary(
                    workItem => workItem.Id,
                    workItem => new DataPipelineRunWorkItemStatus
                    {
                        ContentItemCanonicalId = workItem.ContentItemCanonicalId,
                        Completed = workItem.Completed,
                        Successful = workItem.Successful,
                        Changed = false
                    }));
            }
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

        public async Task ProcessDataPipelineRunWorkItem(
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            lock (_syncRoot)
            {
                if (!_workItemsStatus.TryGetValue(dataPipelineRunWorkItem.Id, out var status))
                {
                    _logger.LogWarning("Data pipeline stage runner does not contain status for work item {WorkItemId}.",
                        dataPipelineRunWorkItem.Id);
                    return;
                }

                status.Completed = dataPipelineRunWorkItem.Completed;
                status.Successful = dataPipelineRunWorkItem.Successful;
            }

            await Task.CompletedTask;
        }

        public void ResetChanged()
        {
            lock (_syncRoot)
            {
                foreach (var status in _workItemsStatus.Values)
                {
                    status.Changed = false;
                }
            }
        }
    }
}
