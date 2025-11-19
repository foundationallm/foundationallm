using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.ContentSafety;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Azure AI Content Safety Shielding Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class AzureAIContentSafetyShieldingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.AZUREAICONTENTSAFETYSHIELDING_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            var contentItemParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.ContentParts);

            if (!contentItemParts.Any())
                return new PluginResult(true, false, WarningMessage: "The content item has no content.");

            var changedContentItemParts = contentItemParts
               .Where(part => part.LastChangedBy.Equals(dataPipelineRun.Id)) // Only shield parts that have changed in the current data pipeline run.
               .ToList();

            if (changedContentItemParts.Count == 0)
            {
                _logger.LogInformation(
                    "The {PluginName} plugin for the {Stage} stage determined there were no changes to process for the work item {WorkItemId}.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    dataPipelineRunWorkItem.Id);
                // Since none of the content has changed, we can skip the shielding step.
                return new PluginResult(true, false);
            }

            var contentSafetyService = serviceProvider.GetRequiredService<IContentSafetyService>();

            var shieldingResult = await contentSafetyService.DetectPromptInjection(
                dataPipelineRunWorkItem.Id,
                changedContentItemParts.Select(part =>
                    new ContentSafetyDocument
                    {
                        Id = part.Position,
                        Content = part.Content!
                    }),
                default);

            if (!shieldingResult.Success)
            {
                _logger.LogError(
                    "The {PluginName} plugin for the {Stage} stage failed to analyze content for the work item {WorkItemId}. Details: {Details}",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    dataPipelineRunWorkItem.Id,
                    shieldingResult.Details);
                return new PluginResult(false, false, ErrorMessage: "Failed to analyze content for prompt injection.");
            }

            var successfullyProcessedPartsCount = shieldingResult.DocumentResults.Values.Count(
                result => result.Success);
            var unsafeParts = shieldingResult.DocumentResults
                .Where(result => !result.Value.SafeContent)
                .ToList();

            _logger.LogInformation(
                "The {PluginName} plugin for the {Stage} stage processed {ProcessedPartsCount} of {TotalPartsCount} parts for the work item {WorkItemId}, with {UnsafePartsCount} parts detected as unsafe.",
                Name,
                dataPipelineRunWorkItem.Stage,
                successfullyProcessedPartsCount,
                changedContentItemParts.Count,
                dataPipelineRunWorkItem.Id,
                unsafeParts.Count);

            if (unsafeParts.Count > 0)
            {
                _logger.LogWarning(
                    "The {PluginName} plugin for the {Stage} stage detected {UnsafePartsCount} unsafe parts in work item {WorkItemId}. The unsafe part identifiers are {UnsafePartIds}. This is a critical situation that prevents further processing.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    unsafeParts.Count,
                    dataPipelineRunWorkItem.Id,
                    string.Join(',', unsafeParts.Select(p => p.Key)));

                // Notifying the data source that prompt injection was detected.
                // Each data source plugin can handle this notification as needed.
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

                _logger.LogWarning(
                    "The {PluginName} plugin is notifying the data source plugin {DataSourcePluginName} about prompt injection detected in work item {WorkItemId}.",
                    Name,
                    dataSourcePluginName,
                    dataPipelineRunWorkItem.Id);

                try
                {
                    await RetryHelper.ExecuteWithRetryAsync(async () =>
                        await dataSourcePlugin.HandleUnsafeContentItem(dataPipelineRunWorkItem.ContentItemCanonicalId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "The {PluginName} plugin encountered an error while notifying the data source plugin {DataSourcePluginName} about prompt injection detected in work item {WorkItemId}.",
                        Name,
                        dataSourcePluginName,
                        dataPipelineRunWorkItem.Id);

                    return new PluginResult(false, true, ErrorMessage: "Prompt injection detected in content. Data source plugin was not able to handle the unsafe content item successfully.");
                }

                return new PluginResult(false, true, ErrorMessage: "Prompt injection detected in content. Data source plugin handled the unsafe content item successfully.");
            }

            foreach (var contentItemPart in changedContentItemParts)
            {
                if (shieldingResult.DocumentResults.TryGetValue(contentItemPart.Position, out var analysisResult))
                {
                    contentItemPart.Shielded = true;
                    contentItemPart.LastChangedBy = dataPipelineRun.Id;
                }
                else
                    _logger.LogWarning(
                        "The {PluginName} plugin for the {Stage} stage could not find the analysis result for part {PartPosition} in work item {WorkItemId}.",
                        Name,
                        dataPipelineRunWorkItem.Stage,
                        contentItemPart.Position,
                        dataPipelineRunWorkItem.Id);
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemParts,
                DataPipelineStateFileNames.ContentParts);

            return successfullyProcessedPartsCount == changedContentItemParts.Count
                ? new PluginResult(true, false)
                : new PluginResult(false, false, WarningMessage: "Some content parts failed to process.");
        }
    }
}
