using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Gateway Text Embedding Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class GatewayTextEmbeddingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE;

        private const int GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS = 5;

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
               .Where(part => part.LastChangedBy.Equals(dataPipelineRun.Id)) // Only embed parts that have changed in the current data pipeline run.
               .ToList();

            if (changedContentItemParts.Count == 0)
            {
                _logger.LogInformation(
                    "The {PluginName} plugin for the {Stage} stage determined there were no changes to process for the work item {WorkItemId}.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    dataPipelineRunWorkItem.Id);
                // Since none of the content has changed, we can skip the embedding step.
                return new PluginResult(true, false);
            }

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGMODEL,
                out var embeddingModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGMODEL} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS,
                out var embeddingDimensions))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS} parameter.");

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var embeddingServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = embeddingModel.ToString()!,
                EmbeddingModelDimensions = (int)embeddingDimensions,
                Prioritized = false,
                TextChunks = [.. changedContentItemParts
                    .Select(part => new TextChunk
                    {
                        Position = part.Position,
                        Content = part.Content,
                        TokensCount = part.ContentSizeTokens
                    })]
            };

            var embeddingResult = await embeddingServiceClient.StartEmbeddingOperation(
                dataPipelineRun.InstanceId,
                textEmbeddingRequest);

            while (embeddingResult.InProgress)
            {
                await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                embeddingResult = await embeddingServiceClient.GetEmbeddingOperationResult(
                    dataPipelineRun.InstanceId,
                    embeddingResult.OperationId!);

                _logger.LogInformation("Data pipeline run {DataPipelineRunId} text parts embedding for {ContentItemCanonicalId}: {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                    dataPipelineRun.Id,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    embeddingResult.ProcessedTextChunksCount,
                    textEmbeddingRequest.TextChunks.Count);
            }

            if (embeddingResult.Failed)
                return new PluginResult(false, false,
                    $"The {Name} plugin failed to process the work item {dataPipelineRunWorkItem.Id} due to a failure in the Gateway API.");

            var embeddingsDictionary = embeddingResult.TextChunks.ToDictionary(
                chunk => chunk.Position,
                chunk => chunk.Embedding);

            foreach (var contentItemPart in changedContentItemParts)
            {
                if (embeddingsDictionary.TryGetValue(contentItemPart.Position, out var embedding))
                {
                    contentItemPart.Embedding = embedding!.Value.Vector.ToArray();
                }
                else
                    throw new PluginException($"The Gateway API did not return an embedding for the content item part at position {contentItemPart.Position}");
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemParts,
                DataPipelineStateFileNames.ContentParts);

            return new PluginResult(true, false);
        }
    }
}
