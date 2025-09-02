using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Hashing;

namespace FoundationaLLM.Common.Services.Plugins
{
    /// <summary>
    /// Implements a basic Data Pipeline Stage Plugin.
    /// </summary>
    public class DataPipelineStagePluginBase: PluginBase, IDataPipelineStagePlugin
    {
        /// <summary>
        /// The data pipeline state service.
        /// </summary>
        protected readonly IDataPipelineStateService _dataPipelineStateService;

        /// <summary>
        /// The hasher used to identify identical text content.
        /// </summary>
        protected readonly XxHash128 _hasher = new XxHash128();

        /// <summary>
        /// Initializes a new instance of the base data pipeline stage plugin.
        /// </summary>
        /// <param name="pluginParameters"></param>
        /// <param name="packageManager"></param>
        /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
        /// <param name="serviceProvider"></param>
        public DataPipelineStagePluginBase(
            Dictionary<string, object> pluginParameters,
            IPluginPackageManager packageManager,
            IPluginPackageManagerResolver packageManagerResolver,
            IServiceProvider serviceProvider)
            : base(pluginParameters, packageManager, packageManagerResolver, serviceProvider) =>
            _dataPipelineStateService =
                _serviceProvider.GetRequiredService<IDataPipelineStateService>()
                ?? throw new PluginException("The data pipeline state service is not available in the dependency injection container.");

        /// <inheritdoc/>
        public virtual async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            string dataPipelineStageName)
        {
            await Task.CompletedTask;
            throw new DataPipelineException(
                $"The {Name} data pipeline stage plugin cannot be used for a starting stage.");
        }

        /// <inheritdoc/>
        public virtual async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<string> contentItemsCanonicalIds,
            string dataPipelineStageName,
            string previousDataPipelineStageName)
        {
            var workItems = contentItemsCanonicalIds
                .Select(contentItemCanonicalId => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRun.RunId,
                    Stage = dataPipelineStageName,
                    PreviousStage = previousDataPipelineStageName,
                    ContentItemCanonicalId = contentItemCanonicalId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        /// <inheritdoc/>
        public virtual async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem) =>
            await Task.FromResult(
                new PluginResult(
                    Success: true,
                    StopProcessing: false));

        /// <summary>
        /// Computes a hexadecimal hash string for the specified input using the current hashing algorithm.
        /// </summary>
        /// <param name="input">The input string to compute the hash for. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A lowercase hexadecimal string representing the computed hash of the input.</returns>
        protected string ComputeHash(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            _hasher.Append(inputBytes);
            var hashBytes = _hasher.GetHashAndReset();
            return Convert.ToBase64String(hashBytes);
        }
    }
}
