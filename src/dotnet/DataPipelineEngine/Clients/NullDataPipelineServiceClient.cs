using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;

namespace FoundationaLLM.DataPipelineEngine.Clients
{
    /// <summary>
    /// Implements a no-operation client.
    /// </summary>
    public class NullDataPipelineServiceClient : IDataPipelineServiceClient
    {
        /// <inheritdoc/>
        public IEnumerable<IResourceProviderService> ResourceProviders { set {} }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> CreateDataPipelineRunAsync(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            DataPipelineDefinitionSnapshot dataPipelineSnapshot,
            UnifiedUserIdentity userIdentity) =>
            await Task.FromResult<DataPipelineRun?>(null);

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> GetDataPipelineRunAsync(
            string instanceId,
            string dataPipelineName,
            string runId,
            UnifiedUserIdentity userIdentity) =>
            await Task.FromResult<DataPipelineRun?>(null);

        /// <inheritdoc/>
        public async Task<DataPipelineRunFilterResponse> GetDataPipelineRunsAsync(
            string instanceId,
            DataPipelineRunFilter dataPipelineRunFilter,
            UnifiedUserIdentity userIdentity) =>
            await Task.FromResult(new DataPipelineRunFilterResponse { Name = string.Empty });

        /// <inheritdoc/>
        public async Task<BinaryData> GetServiceStateAsync() =>
            await Task.FromResult<BinaryData>(BinaryData.FromString(ResourceProviderNames.FoundationaLLM_DataPipeline));
    }
}
