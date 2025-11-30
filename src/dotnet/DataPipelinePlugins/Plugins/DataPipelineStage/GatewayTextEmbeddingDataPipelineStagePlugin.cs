using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
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

        private const int GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS = 1;

        private readonly IResourceProviderService? _contextResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Context);
        private readonly IResourceProviderService? _vectorResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Vector);

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

            if (_contextResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Context} was not loaded.");

            if (_vectorResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vector} was not loaded.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                out var knowledgeUnitObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID} parameter.");

            var embeddingRequestSizeTokens = 100_000; // Default value.
            if (_pluginParameters.TryGetValue(
                PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGREQUESTSIZETOKENS,
                out var embeddingRequestSizeTokensObj))
                embeddingRequestSizeTokens = (int)embeddingRequestSizeTokensObj;

            var knowledgeUnit = await _contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                knowledgeUnitObjectId.ToString()!,
                ServiceContext.ServiceIdentity!);

            var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                knowledgeUnit.VectorDatabaseObjectId,
                ServiceContext.ServiceIdentity!);

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var embeddingServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    dataPipelineRun.InstanceId,
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var contentItemPartGroups = GetContentItemPartGroups(
                changedContentItemParts,
                embeddingRequestSizeTokens);
            _logger.LogInformation(
                "Data pipeline run {DataPipelineRunId} text parts embedding for {ContentItemCanonicalId}: {GroupsCount} operations with max request size of {EmbeddingMaxRequestSizeToken} tokens.",
                dataPipelineRun.Id,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                contentItemPartGroups.Count,
                embeddingRequestSizeTokens);

            var embeddingsDictionary = new Dictionary<int, Embedding?>();
            for (var i = 0; i < contentItemPartGroups.Count; i++)
            {
                var embeddingResult = await ExecuteEmbeddingOperation(
                    dataPipelineRun,
                    dataPipelineRunWorkItem,
                    vectorDatabase,
                    contentItemPartGroups[i],
                    embeddingServiceClient,
                    i + 1,
                    contentItemPartGroups.Count);

                if (embeddingResult.Failed)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to process the work item {dataPipelineRunWorkItem.Id} due to a failure in embedding chunk group {i + 1}.");

                foreach (var textChunk in embeddingResult.TextChunks)
                    embeddingsDictionary[textChunk.Position] = textChunk.Embedding;
            }

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

        private async Task<TextOperationResult>ExecuteEmbeddingOperation(
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            VectorDatabase vectorDatabase,
            List<DataPipelineContentItemContentPart> contentItemParts,
            GatewayServiceClient embeddingServiceClient,
            int embeddingOperationIndex,
            int embeddingOperationsCount)
        {
            var startTime = DateTimeOffset.UtcNow;

            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = vectorDatabase.EmbeddingModel,
                EmbeddingModelDimensions = vectorDatabase.EmbeddingDimensions,
                Prioritized = false,
                TextChunks = [.. contentItemParts
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

                _logger.LogInformation("Data pipeline run {DataPipelineRunId} text parts embedding for {ContentItemCanonicalId}: Operation {EmbeddingOperationIndex} of {EmbeddingOperationsCount} - {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                    dataPipelineRun.Id,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    embeddingOperationIndex,
                    embeddingOperationsCount,
                    embeddingResult.ProcessedTextChunksCount,
                    textEmbeddingRequest.TextChunks.Count);
            }

            _logger.LogInformation("Data pipeline run {DataPipelineRunId} text parts embedding for {ContentItemCanonicalId}: Operation {EmbeddingOperationIndex} of {EmbeddingOperationsCount} completed in {OperationDurationSeconds} seconds.",
                dataPipelineRun.Id,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                embeddingOperationIndex,
                embeddingOperationsCount,
                (DateTimeOffset.UtcNow - startTime).TotalSeconds);

            return embeddingResult;
        }

        private static List<List<DataPipelineContentItemContentPart>> GetContentItemPartGroups(
            List<DataPipelineContentItemContentPart> contentItemParts,
            int maxTokensPerGroup)
        {
            var groups = new List<List<DataPipelineContentItemContentPart>>();
            var currentGroup = new List<DataPipelineContentItemContentPart>();
            var currentTokens = 0;

            foreach (var contentItemPart in contentItemParts)
            {
                // If a single part exceeds the limit, send it alone.
                if (contentItemPart.ContentSizeTokens > maxTokensPerGroup)
                {
                    if (currentGroup.Count > 0)
                    {
                        groups.Add(currentGroup);
                        currentGroup = [];
                        currentTokens = 0;
                    }
                    groups.Add([contentItemPart]);
                    continue;
                }

                if (currentTokens + contentItemPart.ContentSizeTokens > maxTokensPerGroup)
                {
                    groups.Add(currentGroup);
                    currentGroup = [];
                    currentTokens = 0;
                }

                currentGroup.Add(contentItemPart);
                currentTokens += contentItemPart.ContentSizeTokens;
            }

            if (currentGroup.Count > 0)
                groups.Add(currentGroup);

            return groups;
        }
    }
}
