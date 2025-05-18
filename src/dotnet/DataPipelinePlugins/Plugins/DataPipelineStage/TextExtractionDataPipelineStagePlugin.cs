using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TextExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE;

        private readonly Dictionary<string, string> _contentTypeMappings = new()
        {
            { "application/pdf", "PDF" }
        };

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            var workItems = contentItems
                .Select(ci => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRunId,
                    Stage = dataPipelineStageName,
                    InputArtifactId = ci.ContentIdentifier.CanonicalId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        /// <inheritdoc/>
        public override async Task<PluginResult<string>> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!string.IsNullOrWhiteSpace(dataPipelineRunWorkItem.PreviousStage))
                throw new PluginException(
                    $"The plugin {Name} can only be used for data pipeline starting stages.");

            var dataSourcePluginName = ResourcePath.GetResourcePath(
                dataPipelineDefinition.DataSource.PluginObjectId).ResourceId;

            var dataSourcePlugin =
                _packageManager.GetDataSourcePlugin(
                    dataSourcePluginName!,
                    dataPipelineDefinition.DataSource.DataSourceObjectId,
                    dataPipelineRun.TriggerParameterValues.FilterKeys(
                        $"DataSource.{dataPipelineDefinition.DataSource.Name}."),
                    _serviceProvider);

            var rawContentResult = await dataSourcePlugin.GetContentItemRawContent(
                dataPipelineRunWorkItem.InputArtifactId);

            if (!rawContentResult.Success)
                return new PluginResult<string>(null, false, false, rawContentResult.ErrorMessage);

            if (!_contentTypeMappings.TryGetValue(
                rawContentResult.Value!.ContentType,
                out var contentType))
            {
                return new PluginResult<string>(
                    null,
                    false,
                    false,
                    $"The content type {rawContentResult.Value.ContentType} is not supported by the {Name} plugin.");
            }

            // Find all text extraction plugins that support the content type
            var textExtractionPluginsMetadata = _packageManager
                .GetMetadata(dataPipelineRun.InstanceId)
                .Plugins
                .Where(p =>
                    p.Category == PluginCategoryNames.ContentTextExtraction
                    && (p.Subcategory?.Split(',').Contains(contentType) ?? false))
                .ToList();

            if (textExtractionPluginsMetadata.Count == 0)
                throw new PluginException(
                    $"The {Name} plugin cannot map the content type {contentType} to a content text extraction plugin.");

            var dataPipelineStage = dataPipelineDefinition.GetStage(
                dataPipelineRunWorkItem.Stage);

            // Find the first plugin dependency that supports the content type
            var pluginDependency = dataPipelineStage.PluginDependencies
                .FirstOrDefault(pd => textExtractionPluginsMetadata.Select(p => p.ObjectId).Contains(pd.PluginObjectId))
                ?? throw new PluginException(
                    $"The {dataPipelineRunWorkItem.Stage} does not have a dependency context text extraction plugin to handle the {contentType} content type.");

            var textExtractionPluginMetadata = textExtractionPluginsMetadata
                .Single(p => p.ObjectId == pluginDependency.PluginObjectId);

            var textExtractionPlugin = _packageManager
                .GetContentTextExtractionPlugin(
                    textExtractionPluginMetadata.Name,
                    dataPipelineRun.TriggerParameterValues.FilterKeys(
                        $"Stage.{dataPipelineRunWorkItem.Stage}.Dependency.{textExtractionPluginMetadata.Name.Split('-').Last()}."),
                    _serviceProvider);

            var textContentResult = await textExtractionPlugin.ExtractText(
                rawContentResult.Value.RawContent);

            if (!textContentResult.Success)
                return new PluginResult<string>(null, false, false, textContentResult.ErrorMessage);

            // Save the text content to the data pipeline state
            var outputArtifactId = $"{dataPipelineRunWorkItem.InputArtifactId}.txt";

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                new Dictionary<string, BinaryData>
                {
                    { outputArtifactId, BinaryData.FromString(textContentResult.Value!) }
                });

            return
                new PluginResult<string>(dataPipelineRunWorkItem.InputArtifactId, true, false);
        }
    }
}
