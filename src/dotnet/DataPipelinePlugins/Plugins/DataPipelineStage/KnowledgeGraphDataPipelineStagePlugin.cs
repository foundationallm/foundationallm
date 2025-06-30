using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Graph Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE;

        private const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";

        private readonly IResourceProviderService _promptResourceProvider =
            serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Prompt)
            ?? throw new PluginException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} is not available in the dependency injection container.");

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            List<string> contentItemsCanonicalIds,
            string dataPipelineRunId,
            string dataPipelineStageName,
            string previousDataPipelineStageName)
        {
            var workItem = new DataPipelineRunWorkItem
            {
                Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                RunId = dataPipelineRunId,
                Stage = dataPipelineStageName,
                PreviousStage = previousDataPipelineStageName,
                ContentItemCanonicalId = "__all__" // This indicates that the plugin will process all content items in the run.
            };

            await Task.CompletedTask;
            return [workItem];
        }

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            #region Load parameters

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONPROMPTOBJECTID,
                out var entitySummarizationPromptId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONPROMPTOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODEL,
                out var entitySummarizationCompletionModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODEL} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODELTEMPERATURE,
                out var entitySummarizationCompletionModelTemperature))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODELTEMPERATURE} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT,
                out var entitySummarizationCompletionMaxOutputTokenCount))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONEMBEDDINGMODEL,
                out var entitySummarizationEmbeddingModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONEMBEDDINGMODEL} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONEMBEDDINGDIMENSIONS,
                out var entitySummarizationEmbeddingDimensions))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONEMBEDDINGDIMENSIONS} parameter.");

            #endregion

            var entitySummarizationPrompt = await _promptResourceProvider.GetResourceAsync<PromptBase>(
                entitySummarizationPromptId.ToString()!,
                ServiceContext.ServiceIdentity!);

            var entitySummarizationPromptText = (entitySummarizationPrompt as MultipartPrompt)!.Prefix!;

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var gatewayServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            //var contentItemContentParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
            //    dataPipelineDefinition,
            //    dataPipelineRun,
            //    dataPipelineRunWorkItem,
            //    CONTENT_PARTS_FILE_NAME);

            //var textCompletionRequest = new TextCompletionRequest
            //{
            //    CompletionModelName = entityExtractionCompletionModel.ToString()!,
            //    CompletionModelParameters = new Dictionary<string, object>
            //    {
            //        { TextOperationModelParameterNames.Temperature, (float)(double)entityExtractionModelTemperature },
            //        { TextOperationModelParameterNames.MaxOutputTokenCount, (int)entityExtractionMaxOutputTokenCount }
            //    },
            //    TextChunks = [.. contentItemContentParts
            //        .Select(cip => new TextChunk
            //        {
            //            Position = cip.Position,
            //            Content = entityExtractionPromptText.Replace(
            //                INPUT_TEXT_PLACEHOLDER, cip.Content),
            //            TokensCount = cip.ContentSizeTokens + (int)entityExtractionMaxOutputTokenCount
            //        })],
            //};

            //var completionsResult = await gatewayServiceClient.StartCompletionOperation(
            //    dataPipelineRun.InstanceId,
            //    textCompletionRequest);

            //while (completionsResult.InProgress)
            //{
            //    await Task.Delay(TimeSpan.FromSeconds(1));
            //    completionsResult = await gatewayServiceClient.GetCompletionOperationResult(
            //        dataPipelineRun.InstanceId,
            //        completionsResult.OperationId!);
            //}

            //if (completionsResult.Failed)
            //    return new PluginResult(false, false,
            //        $"The {Name} plugin failed to process the work item {dataPipelineRunWorkItem.Id} due to a failure in the Gateway API.");

            //var contentItemKnowledgeParts = contentItemContentParts
            //    .Select(p =>
            //    {
            //        var knowledgePart = DataPipelineContentItemKnowledgePart.FromContentItemPart(p);
            //        var completionResult = completionsResult.TextChunks[knowledgePart.Position - 1];

            //        var finalCompletionResult = completionResult.Completion?.Replace("```json", "").Replace("```", "").Trim(); //unwanted extra annotations
            //        if (!string.IsNullOrWhiteSpace(finalCompletionResult))
            //        {
            //            try
            //            {
            //                knowledgePart.EntitiesAndRelationships =
            //                    JsonSerializer.Deserialize<KnowledgeEntityRelationship<ExtractedKnowledgeEntity, ExtractedKnowledgeRelationship>>(
            //                        finalCompletionResult);
            //            }
            //            catch (Exception ex)
            //            {
            //                _logger.LogWarning(ex, "Invalid entity extraction result for content item {ContentItemCanonicalId} content part {Position}: {EntityExtractionResult}",
            //                    dataPipelineRunWorkItem.ContentItemCanonicalId,
            //                    knowledgePart.Position,
            //                    finalCompletionResult);
            //            }
            //        }

            //        return knowledgePart;
            //    })
            //    .ToList();

            //await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
            //    dataPipelineDefinition,
            //    dataPipelineRun,
            //    dataPipelineRunWorkItem,
            //    contentItemKnowledgeParts,
            //    KNOWLEDGE_PARTS_FILE_NAME);

            return
                new PluginResult(true, false);
        }
    }
}
