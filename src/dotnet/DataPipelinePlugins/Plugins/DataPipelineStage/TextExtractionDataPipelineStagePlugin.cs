using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TextExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            string dataPipelineStageName)
        {
            var workItems = contentItems
                .Where(ci => ci.ContentAction == ContentItemActions.AddOrUpdate)
                .Select(ci => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRun.RunId,
                    Stage = dataPipelineStageName,
                    ContentItemCanonicalId = ci.ContentIdentifier.CanonicalId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!string.IsNullOrWhiteSpace(dataPipelineRunWorkItem.PreviousStage))
                throw new PluginException(
                    $"The plugin {Name} can only be used for data pipeline starting stages.");

            int maxContentSizeCharacters = 10000000; //10 million characters, ~2.5 million tokens

            if (pluginParameters.TryGetValue(
                PluginParameterNames.TEXTEXTRACTION_DATAPIPELINESTAGE_MAXCONTENTSIZECHARACTERS,
                out var maxContentSizeCharactersObject))
                maxContentSizeCharacters = (int)maxContentSizeCharactersObject;

            var dataSourcePluginName = ResourcePath.GetResourcePath(
                dataPipelineDefinition.DataSource.PluginObjectId).ResourceId;

            // This plugin might originate from a different package manager instance,
            // so we need to resolve the package manager for the data source plugin.
            var dataSourcePackageManager = await _packageManagerResolver.GetPluginPackageManager(
                dataPipelineDefinition.DataSource.PluginObjectId,
                ServiceContext.ServiceIdentity!);
                
            var dataSourcePlugin =
                dataSourcePackageManager.GetDataSourcePlugin(
                    dataSourcePluginName!,
                    dataPipelineDefinition.DataSource.DataSourceObjectId,
                    dataPipelineRun.TriggerParameterValues.FilterKeys(
                        $"DataSource.{dataPipelineDefinition.DataSource.Name}."),
                    _packageManagerResolver,
                    _serviceProvider);

            var contentItem = await _dataPipelineStateService.GetDataPipelineContentItem(
                dataPipelineRunWorkItem);
            var rawContentResult = await dataSourcePlugin.GetContentItemRawContent(
                contentItem.ContentIdentifier);

            if (!rawContentResult.Success)
                return new PluginResult(false, false, rawContentResult.ErrorMessage);

            // Merge metadata associated with raw content with metadata associated with the content item.
            // Metadata associated with raw content has priority (i.e., when keys are in conflict,
            // the metadata value associated with raw content wins.
            var metadata = rawContentResult.Value?.Metadata ?? [];
            if (contentItem.Metadata is not null)
                foreach (var contentItemMetadataItem in contentItem.Metadata)
                    if (!metadata.ContainsKey(contentItemMetadataItem.Key))
                        metadata.Add(
                            contentItemMetadataItem.Key,
                            contentItemMetadataItem.Value);

            if (!ContentTypeMappings.Map.TryGetValue(
                rawContentResult.Value!.ContentType,
                out var contentType))
            {
                return new PluginResult(false, true,
                    $"The content type {rawContentResult.Value.ContentType} is not supported by the {Name} plugin.");
            }

            var textContent = string.Empty;

            switch (contentType)
            {
                case "PDF":
                case "DOCX":
                case "PPTX":
                case "XLSX":

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
                            $"The {dataPipelineRunWorkItem.Stage} stage does not have a dependency content text extraction plugin to handle the {contentType} content type.");

                    var textExtractionPluginMetadata = textExtractionPluginsMetadata
                        .Single(p => p.ObjectId == pluginDependency.PluginObjectId);

                    // This plugin might originate from a different package manager instance,
                    // so we need to resolve the package manager for plugin dependency plugin.
                    var dependencyPackageManager = await _packageManagerResolver.GetPluginPackageManager(
                        pluginDependency.PluginObjectId,
                        ServiceContext.ServiceIdentity!);

                    var textExtractionPlugin = dependencyPackageManager
                        .GetContentTextExtractionPlugin(
                            textExtractionPluginMetadata.Name,
                            dataPipelineRun.TriggerParameterValues.FilterKeys(
                                $"Stage.{dataPipelineRunWorkItem.Stage}.Dependency.{textExtractionPluginMetadata.Name.Split('-').Last()}."),
                            _packageManagerResolver,
                            _serviceProvider);

                    var textContentResult = await textExtractionPlugin.ExtractText(
                        rawContentResult.Value.RawContent);

                    if (!textContentResult.Success)
                        return new PluginResult(false, false, textContentResult.ErrorMessage);

                    textContent = textContentResult.Value!;

                    break;

                case "TXT":
                case "MD":
                case "HTML":
                case "RTF":
                case "JS":
                case "YAML":
                case "TOML":
                case "JSON":
                case "JSONL":
                case "XML":
                case "TSV":
                case "CSV":
                case "CSS":
                case "PY":
                case "JAVA":
                case "SH":

                    textContent = rawContentResult.Value.RawContent.ToString();

                    break;

                default:
                    return new PluginResult(false, true,
                        $"The content type {contentType} is not supported by the {Name} plugin.");
            }

            // Enforce new line normalization
            textContent = textContent.Replace("\r\n", "\n").Replace("\r", "\n");

            string? warningMessage = null;
            if (textContent.Length > maxContentSizeCharacters)
            {
                _logger.LogWarning(
                    "The {PluginName} plugin for the {Stage} stage ignored the work item {WorkItemId} because its size exceeds {MaxContentSizeCharacters} characters.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    dataPipelineRunWorkItem.Id,
                    maxContentSizeCharacters);
                warningMessage = $"The extracted text content size exceeds the maximum allowed size of {maxContentSizeCharacters} characters and has been discarded.";
                textContent = string.Empty;
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                [ new DataPipelineStateArtifact
                    {
                        FileName = DataPipelineStateFileNames.TextContent,
                        ContentType = "text/plain",
                        Content = BinaryData.FromString(textContent)
                    },
                    new DataPipelineStateArtifact
                    {
                        FileName =  DataPipelineStateFileNames.Metadata,
                        ContentType = "application/json",
                        Content = BinaryData.FromString(JsonSerializer.Serialize(metadata))
                    }
                ]);

            return
                new PluginResult(true, false, WarningMessage: warningMessage);
        }
    }
}
