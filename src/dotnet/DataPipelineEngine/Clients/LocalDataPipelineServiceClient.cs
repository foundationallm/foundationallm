using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Hosting;

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
        private readonly IDataPipelineStateService _dataPipelineStateService = dataPipelineStateService;

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
            string runId,
            UnifiedUserIdentity userIdentity) =>
            await _dataPipelineStateService.GetDataPipelineRun(runId);
    }
}
