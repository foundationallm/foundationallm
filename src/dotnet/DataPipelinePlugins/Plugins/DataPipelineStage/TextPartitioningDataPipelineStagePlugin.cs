using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Partitioning Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TextPartitioningDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE;

        private const string CONTENT_PARTS_FILE_NAME = "content-parts.parquet";

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.TEXTPARTITIONING_DATAPIPELINESTAGE_PARITTIONINGSTRATETGY,
                out var partitioningStrategy))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.TEXTPARTITIONING_DATAPIPELINESTAGE_PARITTIONINGSTRATETGY} parameter.");

            // Find all text partitioning plugins that support the partitioning strategy.
            var textPartitioningPluginsMetadata = _packageManager
                .GetMetadata(dataPipelineRun.InstanceId)
                .Plugins
                .Where(p =>
                    p.Category == PluginCategoryNames.ContentTextPartitioning
                    && (p.Subcategory?.Split(',').Contains(partitioningStrategy) ?? false))
                .ToList();

            if (textPartitioningPluginsMetadata.Count == 0)
                throw new PluginException(
                    $"The {Name} plugin cannot find a content text partitioning plugin for the {partitioningStrategy} partitioning strategy.");

            var dataPipelineStage = dataPipelineDefinition.GetStage(
                dataPipelineRunWorkItem.Stage);

            // Find the first plugin dependency that supports the partitioning strategy
            var pluginDependency = dataPipelineStage.PluginDependencies
                .FirstOrDefault(pd => textPartitioningPluginsMetadata.Select(p => p.ObjectId).Contains(pd.PluginObjectId))
                ?? throw new PluginException(
                    $"The {dataPipelineRunWorkItem.Stage} stage does not have a dependency content text partitioning plugin to handle the {partitioningStrategy} partitioning strategy.");

            var textPartitioningPluginMetadata = textPartitioningPluginsMetadata
                .Single(p => p.ObjectId == pluginDependency.PluginObjectId);

            // This plugin might originate from a different package manager instance,
            // so we need to resolve the package manager for plugin dependency plugin.
            var dependencyPackageManager = await _packageManagerResolver.GetPluginPackageManager(
                pluginDependency.PluginObjectId,
                ServiceContext.ServiceIdentity!);

            var textPartitioningPlugin = dependencyPackageManager
                .GetContentTextPartitioningPlugin(
                    textPartitioningPluginMetadata.Name,
                    dataPipelineRun.TriggerParameterValues.FilterKeys(
                        $"Stage.{dataPipelineRunWorkItem.Stage}.Dependency.{textPartitioningPluginMetadata.Name.Split('-').Last()}."),
                    _packageManagerResolver,
                    _serviceProvider);

            var inputContent = await _dataPipelineStateService.LoadDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                "content.txt");

            var originalFileName = (await _dataPipelineStateService.LoadDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                "original-file-name.txt"))
                .First().Content.ToString();

            if (inputContent == null
                || inputContent.Count == 0)
                throw new PluginException(
                    $"The {Name} plugin cannot find the input content for the {dataPipelineRunWorkItem.Stage} stage.");

            var textPartitioningResult = await textPartitioningPlugin.PartitionText(
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                inputContent.First().Content.ToString());

            if (textPartitioningResult.Value is not null)
                foreach (var itemPart in textPartitioningResult.Value)
                    itemPart.Metadata = new Dictionary<string, string> { { "FileName", originalFileName } };

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                textPartitioningResult.Value ?? [],
                CONTENT_PARTS_FILE_NAME);

            return
                new PluginResult(true, false);
        }
    }
}
