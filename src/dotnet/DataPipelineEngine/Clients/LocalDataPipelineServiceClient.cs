using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace FoundationaLLM.DataPipelineEngine.Clients
{
    /// <summary>
    /// Local client for the Data Pipeline API.
    /// </summary>
    /// <param name="dataPipelineTriggerService">The data pipeline triggering service.</param>
    public class LocalDataPipelineServiceClient(
        IEnumerable<IHostedService> hostedServices,
        IDataPipelineStateService dataPipelineStateService) : IDataPipelineServiceClient
    {
        private readonly IDataPipelineTriggerService _dataPipelineTriggerService =
            (hostedServices.SingleOrDefault(hs => hs is IDataPipelineTriggerService) as IDataPipelineTriggerService)!;
        private readonly IDataPipelineRunnerService _dataPipelineRunnerService =
            (hostedServices.SingleOrDefault(hs => hs is IDataPipelineRunnerService) as IDataPipelineRunnerService)!;
        private readonly IDataPipelineStateService _dataPipelineStateService = dataPipelineStateService;

        private readonly JsonSerializerOptions _jsonSerializerOptions =
            new()
            {
                WriteIndented = true
            };

        /// <inheritdoc/>
        public IEnumerable<IResourceProviderService> ResourceProviders
        {
            set
            {
                if (null != _dataPipelineTriggerService)
                    (_dataPipelineTriggerService as IResourceProviderClient)!.ResourceProviders = value;
            }
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> CreateDataPipelineRunAsync(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            DataPipelineDefinitionSnapshot dataPipelineSnapshot,
            UnifiedUserIdentity userIdentity) =>
            await _dataPipelineTriggerService.TriggerDataPipeline(
                instanceId,
                dataPipelineRun,
                dataPipelineSnapshot,
                userIdentity);

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> GetDataPipelineRunAsync(
            string instanceId,
            string dataPipelineName,
            string runId,
            UnifiedUserIdentity userIdentity) =>
            await _dataPipelineStateService.GetDataPipelineRun(runId);

        /// <inheritdoc/>
        public async Task<DataPipelineRunFilterResponse> GetDataPipelineRunsAsync(
            string instanceId,
            DataPipelineRunFilter dataPipelineRunFilter,
            UnifiedUserIdentity userIdentity) =>
            await _dataPipelineStateService.GetDataPipelineRuns(dataPipelineRunFilter);

        /// <inheritdoc/>
        public async Task<BinaryData> GetServiceStateAsync()
        {
            var state = _dataPipelineRunnerService.CurrentRunners
                .Select(kvp => new
                {
                    RunId = kvp.Key,
                    RunnerState = kvp.Value.CurrentStageRunners
                        .Select(kvp2 => new
                        {
                            StageName = kvp2.Key,
                            StageRunnerState = new
                            {
                                kvp2.Value.Completed,
                                kvp2.Value.Successful,
                                kvp2.Value.WorkItemsCount,
                                kvp2.Value.CompletedWorkItemsCount,
                                kvp2.Value.SuccessfulWorkItemsCount,
                            }
                        })
                })
                .ToDictionary(kvp => kvp.RunId, kvp => kvp.RunnerState);
            var jsonState = JsonSerializer.Serialize(
                state,
                _jsonSerializerOptions);
            return await Task.FromResult<BinaryData>(BinaryData.FromString(jsonState));
        }
    }
}
