using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.Gateway;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE;

        private const string ENTITY_TYPES_PLACEHOLDER = "{entity_types}";
        private const string INPUT_TEXT_PLACEHOLDER = "{input_text}";

        private const int GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS = 5;

        private readonly IResourceProviderService _promptResourceProvider =
            serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Prompt)
            ?? throw new PluginException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} is not available in the dependency injection container.");

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            #region Load parameters

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID,
                out var entityExtractionPromptId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONCOMPLETIONMODEL,
                out var entityExtractionCompletionModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONCOMPLETIONMODEL} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONENTITYTYPES,
                out var entityExtractionEntityTypes))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONENTITYTYPES} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMODELTEMPERATURE,
                out var entityExtractionModelTemperature))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMODELTEMPERATURE} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMAXOUTPUTTOKENCOUNT,
                out var entityExtractionMaxOutputTokenCount))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMAXOUTPUTTOKENCOUNT} parameter.");

            #endregion

            #region Early stop conditions

            var metadataChanged = await _dataPipelineStateService.DataPipelineRunWorkItemArtifactChanged(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.Metadata);
            var serializedMetadata = (await _dataPipelineStateService.LoadDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.Metadata))
                .First().Content.ToString();
            var contentItemContentParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.ContentParts);
            var changedContentItemContentParts = contentItemContentParts
                .Where(part => part.LastChangedBy.Equals(dataPipelineRun.Id))
                .ToList();
            var existingContentItemKnowledgeParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.KnowledgeParts);

            string? warningMessage = null;
            if (!contentItemContentParts.Any())
            {
                warningMessage = "The content item has no content.";

                await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem,
                    [],
                    DataPipelineStateFileNames.KnowledgeParts);

                return
                    new PluginResult(true, false, WarningMessage: warningMessage);
            }
            else if (changedContentItemContentParts.Count == 0)
            {
                if (!metadataChanged)
                {
                    _logger.LogInformation(
                        "The {PluginName} plugin for the {Stage} stage determined there were no changes to process for the work item {WorkItemId}.",
                        Name,
                        dataPipelineRunWorkItem.Stage,
                        dataPipelineRunWorkItem.Id);
                    return
                        new PluginResult(true, false, WarningMessage: warningMessage);
                }

                _logger.LogInformation(
                    "The {PluginName} plugin for the {Stage} stage determined that only metadata changed for the work item {WorkItemId}.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    dataPipelineRunWorkItem.Id);

                foreach (var existingKnowledgePart in existingContentItemKnowledgeParts)
                {
                    existingKnowledgePart.Metadata = serializedMetadata;
                    existingKnowledgePart.LastChangedBy = dataPipelineRun.Id;
                }

                await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                   dataPipelineDefinition,
                   dataPipelineRun,
                   dataPipelineRunWorkItem,
                   existingContentItemKnowledgeParts,
                   DataPipelineStateFileNames.KnowledgeParts);

                return
                    new PluginResult(true, false, WarningMessage: warningMessage);
            }

            #endregion

            var entityExtractionPrompt = await _promptResourceProvider.GetResourceAsync<PromptBase>(
                entityExtractionPromptId.ToString()!,
                ServiceContext.ServiceIdentity!);

            var entityExtractionPromptText = (entityExtractionPrompt as MultipartPrompt)!.Prefix!
                .Replace(ENTITY_TYPES_PLACEHOLDER, entityExtractionEntityTypes.ToString()!);

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var gatewayServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var completionsResult = await CreateAndExecuteTextCompletionRequestAsync(
                gatewayServiceClient,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                changedContentItemContentParts,
                entityExtractionPromptText,
                entityExtractionCompletionModel,
                entityExtractionModelTemperature,
                entityExtractionMaxOutputTokenCount,
                _logger);

            if (completionsResult.Failed)
                return new PluginResult(false, false,
                    $"The {Name} plugin failed to process the work item {dataPipelineRunWorkItem.Id} due to a failure in the Gateway API.");

            var completionsDictionary = completionsResult.TextChunks.ToDictionary(
                chunk => chunk.Position,
                chunk => chunk.Completion);

            var changedContentItemsContentPartsForRetry = new List<DataPipelineContentItemContentPart>();
            var changedContentItemKnowledgeParts = changedContentItemContentParts
                .Select(p =>
                {
                    var knowledgePart = DataPipelineContentItemKnowledgePart.FromContentItemPart(p);
                    knowledgePart.Metadata = serializedMetadata;

                    if (completionsDictionary.TryGetValue(knowledgePart.Position, out var completion))
                    {
                        #region Parse completion result

                        var finalCompletionResult = completion!.Replace("```json", "").Replace("```", "").Trim(); //unwanted extra annotations
                        if (!string.IsNullOrWhiteSpace(finalCompletionResult))
                        {
                            try
                            {
                                knowledgePart.EntitiesAndRelationships =
                                    JsonSerializer.Deserialize<KnowledgeEntityRelationshipCollection<ExtractedKnowledgeEntity, ExtractedKnowledgeRelationship>>(
                                        finalCompletionResult);
                            }
                            catch (JsonException jsonEx)
                            {
                                _logger.LogWarning(jsonEx, "Could not deserialize entity extraction result for content item {ContentItemCanonicalId} content part {Position}.",
                                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                                    knowledgePart.Position);
                                changedContentItemsContentPartsForRetry.Add(p);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Invalid entity extraction result for content item {ContentItemCanonicalId} content part {Position}: {EntityExtractionResult}",
                                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                                    knowledgePart.Position,
                                    finalCompletionResult);
                            }
                        }

                        #endregion
                    }
                    else
                        throw new PluginException($"The Gateway API did not return a completion for the content item part at position {p.Position}");

                    return knowledgePart;
                })
                .ToList();

            var changedContentItemKnowledgePartsDictionary = changedContentItemKnowledgeParts
                .ToDictionary(kp => kp.Position);

            #region Submit retry for content item parts that failed to extract entities

            if (changedContentItemsContentPartsForRetry.Count > 0)
            {

                // This is most likely due to the max output token count being too low.
                // This attempt will retry with a max output token count that is double the original one.

                _logger.LogInformation(
                    "The {PluginName} plugin for the {Stage} stage is retrying entity extraction for {RetryCount} content item parts for work item {WorkItemId}.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    changedContentItemsContentPartsForRetry.Count,
                    dataPipelineRunWorkItem.Id);

                var completionsRetryResult = await CreateAndExecuteTextCompletionRequestAsync(
                    gatewayServiceClient,
                    dataPipelineRun,
                    dataPipelineRunWorkItem,
                    changedContentItemsContentPartsForRetry,
                    entityExtractionPromptText,
                    entityExtractionCompletionModel,
                    entityExtractionModelTemperature,
                    (int)entityExtractionMaxOutputTokenCount * 2,
                    _logger);

                if (!completionsRetryResult.Failed)
                {
                    var completionsRetryDictionary = completionsRetryResult.TextChunks.ToDictionary(
                        chunk => chunk.Position,
                        chunk => chunk.Completion);

                    var contentItemKnowledgePartsFromRetry = changedContentItemsContentPartsForRetry
                        .Select(p =>
                        {
                            var knowledgePart = DataPipelineContentItemKnowledgePart.FromContentItemPart(p);
                            knowledgePart.Metadata = serializedMetadata;

                            if (completionsRetryDictionary.TryGetValue(knowledgePart.Position, out var completion))
                            {
                                #region Parse completion result

                                var finalCompletionResult = completion!.Replace("```json", "").Replace("```", "").Trim(); //unwanted extra annotations
                                if (!string.IsNullOrWhiteSpace(finalCompletionResult))
                                {
                                    try
                                    {
                                        knowledgePart.EntitiesAndRelationships =
                                            JsonSerializer.Deserialize<KnowledgeEntityRelationshipCollection<ExtractedKnowledgeEntity, ExtractedKnowledgeRelationship>>(
                                                finalCompletionResult);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Invalid entity extraction result for content item {ContentItemCanonicalId} content part {Position}: {EntityExtractionResult}",
                                            dataPipelineRunWorkItem.ContentItemCanonicalId,
                                            knowledgePart.Position,
                                            finalCompletionResult);
                                    }
                                }

                                #endregion
                            }

                            return knowledgePart;
                        })
                        .ToList();

                    // Overwrite the existing knowledge parts with the ones from the retry.
                    foreach (var contentItemKnowledgePartFromRetry in contentItemKnowledgePartsFromRetry)
                        changedContentItemKnowledgePartsDictionary[contentItemKnowledgePartFromRetry.Position] =
                            contentItemKnowledgePartFromRetry;
                }
                else
                    _logger.LogWarning(
                        "The {PluginName} plugin for the {Stage} stage failed retrying entity extraction for {RetryCount} content item parts for work item {WorkItemId} due to a failure in the Gateway API.",
                        Name,
                        dataPipelineRunWorkItem.Stage,
                        changedContentItemsContentPartsForRetry.Count,
                        dataPipelineRunWorkItem.Id);
            }

            #endregion

            var finalContentItemKnowledgeParts =
                changedContentItemKnowledgePartsDictionary.Values.ToList();

            // Determine if there are existing knowledge parts that were not changed in this run
            // and the are inside the range of content item parts (which means they need to be kept).
            var existingContentItemKnowledgePartsToKeep = existingContentItemKnowledgeParts
                .Where(ekp =>
                    !changedContentItemKnowledgePartsDictionary.ContainsKey(ekp.Position)
                    && ekp.Position <= contentItemContentParts.Count())
                .ToList();

            // If the metadata has changed, we need to update it on the existing knowledge parts to keep.
            // Note that for the changed ones, we already set the metadata above.
            if (metadataChanged)
                foreach (var existingKnowledgePartToKeep in existingContentItemKnowledgePartsToKeep)
                {
                    existingKnowledgePartToKeep.Metadata = serializedMetadata;
                    existingKnowledgePartToKeep.LastChangedBy = dataPipelineRun.Id;
                }

            finalContentItemKnowledgeParts.AddRange(
                existingContentItemKnowledgeParts);        

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                finalContentItemKnowledgeParts.OrderBy(x => x.Position),
                DataPipelineStateFileNames.KnowledgeParts);

            return
                new PluginResult(true, false, WarningMessage: warningMessage);
        }

        /// <summary>
        /// Creates and executes a text completion request using the GatewayServiceClient and polls until completion.
        /// </summary>
        /// <param name="gatewayServiceClient">The GatewayServiceClient instance.</param>
        /// <param name="dataPipelineRun">The current DataPipelineRun.</param>
        /// <param name="dataPipelineRunWorkItem">The current DataPipelineRunWorkItem.</param>
        /// <param name="changedContentItemContentParts">The content parts to process.</param>
        /// <param name="entityExtractionPromptText">The prompt text to use for each chunk.</param>
        /// <param name="entityExtractionCompletionModel">The completion model name.</param>
        /// <param name="entityExtractionModelTemperature">The model temperature.</param>
        /// <param name="entityExtractionMaxOutputTokenCount">The max output token count.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="pollingIntervalSeconds">Polling interval in seconds.</param>
        /// <returns>The completed TextOperationResult.</returns>
        private async Task<TextOperationResult> CreateAndExecuteTextCompletionRequestAsync(
            GatewayServiceClient gatewayServiceClient,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<DataPipelineContentItemContentPart> changedContentItemContentParts,
            string entityExtractionPromptText,
            object entityExtractionCompletionModel,
            object entityExtractionModelTemperature,
            object entityExtractionMaxOutputTokenCount,
            ILogger logger,
            int pollingIntervalSeconds = GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS)
        {
            var textCompletionRequest = new TextCompletionRequest
            {
                CompletionModelName = entityExtractionCompletionModel.ToString()!,
                CompletionModelParameters = new Dictionary<string, object>
                {
                    { TextOperationModelParameterNames.Temperature, (float)(double)entityExtractionModelTemperature },
                    { TextOperationModelParameterNames.MaxOutputTokenCount, (int)entityExtractionMaxOutputTokenCount }
                },
                TextChunks = [.. changedContentItemContentParts
                    .Select(cip => new TextChunk
                    {
                        Position = cip.Position,
                        Content = entityExtractionPromptText.Replace(INPUT_TEXT_PLACEHOLDER, cip.Content),
                        TokensCount = cip.ContentSizeTokens + (int)entityExtractionMaxOutputTokenCount
                    })],
            };

            var completionsResult = await gatewayServiceClient.StartCompletionOperation(
                dataPipelineRun.InstanceId,
                textCompletionRequest);

            while (completionsResult.InProgress)
            {
                await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds));
                completionsResult = await gatewayServiceClient.GetCompletionOperationResult(
                    dataPipelineRun.InstanceId,
                    completionsResult.OperationId!);

                logger.LogInformation("Data pipeline run {DataPipelineRunId} entity extraction for {ContentItemCanonicalId}: {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                    dataPipelineRun.Id,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    completionsResult.ProcessedTextChunksCount,
                    textCompletionRequest.TextChunks.Count);
            }

            return completionsResult;
        }
    }
}
