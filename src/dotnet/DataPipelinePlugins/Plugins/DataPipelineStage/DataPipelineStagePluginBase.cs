using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements a basic Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class DataPipelineStagePluginBase: PluginBase, IDataPipelineStagePlugin
    {
        protected readonly IDataPipelineStateService _dataPipelineStateService;

        /// <summary>
        /// Initializes a new instance of the base data pipeline stage plugin.
        /// </summary>
        /// <param name="pluginParameters"></param>
        /// <param name="packageManager"></param>
        /// <param name="serviceProvider"></param>
        public DataPipelineStagePluginBase(
            Dictionary<string, object> pluginParameters,
            IPluginPackageManager packageManager,
            IServiceProvider serviceProvider)
            : base(pluginParameters, packageManager, serviceProvider) =>
            _dataPipelineStateService =
                _serviceProvider.GetRequiredService<IDataPipelineStateService>()
                ?? throw new PluginException("The data pipeline state service is not available in the dependency injection container.");

        /// <inheritdoc/>
        public virtual async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            await Task.CompletedTask;
            throw new DataPipelineException(
                $"The {Name} data pipeline stage plugin cannot be used for a starting stage.");
        }

        /// <inheritdoc/>
        public virtual async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            List<string> inboundArtifactIds,
            string dataPipelineRunId,
            string dataPipelineStageName,
            string previousDataPipelineStageName)
        {
            var workItems = inboundArtifactIds
                .Select(artifactId => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRunId,
                    Stage = dataPipelineStageName,
                    PreviousStage = previousDataPipelineStageName,
                    InputArtifactId = artifactId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        /// <inheritdoc/>
        public virtual async Task<PluginResult<string>> ProcessWorkItem(DataPipelineRunWorkItem workItem) =>
            await Task.FromResult(
                new PluginResult<string>(
                    Value: string.Empty,
                    Success: true,
                    StopProcessing: false));
    }
}
