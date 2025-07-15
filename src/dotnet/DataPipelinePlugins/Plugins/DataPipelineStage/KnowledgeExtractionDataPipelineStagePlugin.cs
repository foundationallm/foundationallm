using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
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

        private const string CONTENT_PARTS_FILE_NAME = "content-parts.parquet";
        private const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";
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

            var contentItemContentParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                CONTENT_PARTS_FILE_NAME);

            var textCompletionRequest = new TextCompletionRequest
            {
                CompletionModelName = entityExtractionCompletionModel.ToString()!,
                CompletionModelParameters = new Dictionary<string, object>
                {
                    { TextOperationModelParameterNames.Temperature, (float)(double)entityExtractionModelTemperature },
                    { TextOperationModelParameterNames.MaxOutputTokenCount, (int)entityExtractionMaxOutputTokenCount }
                },
                TextChunks = [.. contentItemContentParts
                    .Select(cip => new TextChunk
                    {
                        Position = cip.Position,
                        Content = entityExtractionPromptText.Replace(
                            INPUT_TEXT_PLACEHOLDER, cip.Content),
                        TokensCount = cip.ContentSizeTokens + (int)entityExtractionMaxOutputTokenCount
                    })],
            };

            var completionsResult = await gatewayServiceClient.StartCompletionOperation(
                dataPipelineRun.InstanceId,
                textCompletionRequest);

            while (completionsResult.InProgress)
            {
                await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                completionsResult = await gatewayServiceClient.GetCompletionOperationResult(
                    dataPipelineRun.InstanceId,
                    completionsResult.OperationId!);

                _logger.LogInformation("Data pipeline run {DataPipelineRunId} entity extraction for {ContentItemCanonicalId}: {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                    dataPipelineRun.Id,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    completionsResult.ProcessedTextChunksCount,
                    textCompletionRequest.TextChunks.Count);
            }

            if (completionsResult.Failed)
                return new PluginResult(false, false,
                    $"The {Name} plugin failed to process the work item {dataPipelineRunWorkItem.Id} due to a failure in the Gateway API.");

            var contentItemKnowledgeParts = contentItemContentParts
                .Select(p =>
                {
                    var knowledgePart = DataPipelineContentItemKnowledgePart.FromContentItemPart(p);
                    var completionResult = completionsResult.TextChunks[knowledgePart.Position - 1];

                    var finalCompletionResult = completionResult.Completion?.Replace("```json", "").Replace("```", "").Trim(); //unwanted extra annotations
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

                    return knowledgePart;
                })
                .ToList();

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemKnowledgeParts,
                KNOWLEDGE_PARTS_FILE_NAME);

            return
                new PluginResult(true, false);
        }
    }
}
