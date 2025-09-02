using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Gateway;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Graph Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphSummarizationDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : KnowledgeGraphDataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_SUMMARIZATION_DATAPIPELINESTAGE;

        private const string ENTITY_NAMES_PLACEHOLDER = "{entity_names}";
        private const string DESCRIPTIONS_LIST_PLACEHOLDER = "{descriptions_list}";

        private string _summarizationPrompt = null!;

        private readonly KnowledgeEntityRelationshipCollection<KnowledgeEntity, KnowledgeRelationship> _entityRelationships = new();

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<string> contentItemsCanonicalIds,
            string dataPipelineStageName,
            string previousDataPipelineStageName)
        {
            if (contentItemsCanonicalIds.Count != 1)
                throw new DataPipelineException(
                    $"The {Name} data pipeline stage plugin requires exactly one content item canonical identifier.");

            var knowledgeBucketsRegistry = await LoadKnowledgeBucketsRegistry(
                dataPipelineDefinition,
                dataPipelineRun);

            var workItems = new List<DataPipelineRunWorkItem>();

            workItems.AddRange(
                knowledgeBucketsRegistry.Entities.Values
                .Select(kbre => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRun.RunId,
                    Stage = dataPipelineStageName,
                    PreviousStage = previousDataPipelineStageName,
                    ContentItemCanonicalId = kbre.BucketId
                }));

            workItems.AddRange(
                knowledgeBucketsRegistry.Relationships.Values
                .Select(kbre => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRun.RunId,
                    Stage = dataPipelineStageName,
                    PreviousStage = previousDataPipelineStageName,
                    ContentItemCanonicalId = kbre.BucketId
                }));

            await Task.CompletedTask;
            return workItems;
        }

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            #region Load parameters

            if (_promptResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} was not loaded");

            if (_contextResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Context} was not loaded.");

            if (_vectorResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vector} was not loaded.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONPROMPTOBJECTID,
                out var entitySummarizationPromptId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONPROMPTOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMODEL,
                out var entitySummarizationCompletionModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMODEL} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMODELTEMPERATURE,
                out var entitySummarizationCompletionModelTemperature))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMODELTEMPERATURE} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT,
                out var entitySummarizationCompletionMaxOutputTokenCount))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT} parameter.");

            #endregion

            await CreateGatewayServiceClient(dataPipelineRun.InstanceId);
            await LoadSummarizationPrompt(
                entitySummarizationPromptId.ToString()!);

            if (dataPipelineRunWorkItem.ContentItemCanonicalId.StartsWith(
                KNOWLEDGE_GRAPH_ENTITY))
            {
                await LoadEntities(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                var entitiesSummarizationResult = await SummarizeEntities(
                    dataPipelineRun,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    entitySummarizationCompletionModel.ToString()!,
                    (float)(double)entitySummarizationCompletionModelTemperature,
                    (int)entitySummarizationCompletionMaxOutputTokenCount);

                if (!entitiesSummarizationResult.Success)
                    return entitiesSummarizationResult;

                await SaveEntities(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);
            }
            else if (dataPipelineRunWorkItem.ContentItemCanonicalId.StartsWith(
                KNOWLEDGE_GRAPH_RELATIONSHIP))
            {
                await LoadRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                var relationshipsSummarizationResult = await SummarizeRelationships(
                    dataPipelineRun,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    entitySummarizationCompletionModel.ToString()!,
                    (float)(double)entitySummarizationCompletionModelTemperature,
                    (int)entitySummarizationCompletionMaxOutputTokenCount);

                if (!relationshipsSummarizationResult.Success)
                    return relationshipsSummarizationResult;

                await SaveRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);
            }
            else
                return new PluginResult(false, true,
                    $"The {Name} plugin received an invalid content item canonical identifier {dataPipelineRunWorkItem.ContentItemCanonicalId}.");

            return
                new PluginResult(true, false);
        }

        private async Task LoadSummarizationPrompt(
            string summarizationPromptObjectId)
        {
            if (_summarizationPrompt is not null)
                return; // Already loaded

            var entitySummarizationPrompt = await _promptResourceProvider!.GetResourceAsync<PromptBase>(
                    summarizationPromptObjectId,
                    ServiceContext.ServiceIdentity!);

            _summarizationPrompt = (entitySummarizationPrompt as MultipartPrompt)!.Prefix!;
        }

        private async Task<PluginResult> SummarizeEntities(
            DataPipelineRun dataPipelineRun,
            string knowledgeBucketId,
            string summarizationModel,
            float summarizationModelTemperature,
            int summarizationMaxOutputTokenCount)
        {
            var positionsMapping = new Dictionary<string, int>();
            var currentRequestPosition = 0;
            var startTime = DateTimeOffset.UtcNow;

            var eligibleEntities = _entityRelationships.Entities
                .Where(e =>
                    (e.LastChangedBy == dataPipelineRun.Id // Only summarize entities that have changed in the current data pipeline run.
                    && e.Descriptions.Count > 1)
                    || string.IsNullOrWhiteSpace(e.SummaryDescription));

            var textCompletionRequest = new TextCompletionRequest
            {
                CompletionModelName = summarizationModel,
                CompletionModelParameters = new Dictionary<string, object>
                {
                    { TextOperationModelParameterNames.Temperature, summarizationModelTemperature },
                    { TextOperationModelParameterNames.MaxOutputTokenCount, summarizationMaxOutputTokenCount }
                },
                TextChunks = [.. eligibleEntities
                    .Select(e =>
                    {
                        var content = _summarizationPrompt
                            .Replace(ENTITY_NAMES_PLACEHOLDER, e.Name)
                            .Replace(DESCRIPTIONS_LIST_PLACEHOLDER, string.Join(Environment.NewLine, e.Descriptions));
                        currentRequestPosition++;
                        positionsMapping[e.UniqueId] = currentRequestPosition;

                        var textChunk = new TextChunk
                        {
                            Position = currentRequestPosition,
                            Content = content,
                            TokensCount =
                                (int)_tokenizer.CountTokens(content)
                                + summarizationMaxOutputTokenCount
                        };
                        return textChunk;
                    })],
            };

            Dictionary<int, string> completionsDictionary = [];

            if (textCompletionRequest.TextChunks.Count > 0)
            {
                var completionsResult = await _gatewayServiceClient.StartCompletionOperation(
                    dataPipelineRun.InstanceId,
                    textCompletionRequest);

                while (completionsResult.InProgress)
                {
                    await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                    completionsResult = await _gatewayServiceClient.GetCompletionOperationResult(
                        dataPipelineRun.InstanceId,
                        completionsResult.OperationId!);

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} bucket {BucketId} entity summarization: {ProcessedEntityCount} of {TotalEntityCount} entities, {TotalSeconds} seconds.",
                        dataPipelineRun.Id,
                        knowledgeBucketId,
                        completionsResult.ProcessedTextChunksCount,
                        textCompletionRequest.TextChunks.Count,
                        (DateTimeOffset.UtcNow - startTime).TotalSeconds);
                }

                if (completionsResult.Failed)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to summarize knowledge graph entities for data pipeline run {dataPipelineRun.Id} due to a failure in the Gateway API.");

                completionsDictionary = completionsResult.TextChunks.ToDictionary(
                    chunk => chunk.Position,
                    chunk => chunk.Completion!);
            }

            foreach (var entity in eligibleEntities)
            {
                if (entity.Descriptions.Count == 1)
                {
                    entity.SummaryDescription = entity.Descriptions[0];
                    entity.LastChangedBy = dataPipelineRun.RunId;
                }
                else
                {
                    if (completionsDictionary.TryGetValue(positionsMapping[entity.UniqueId], out var completion))
                    {
                        entity.SummaryDescription = completion;
                        entity.LastChangedBy = dataPipelineRun.RunId;
                    }
                    else
                        _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return a summary for the entity {EntityId}.",
                            dataPipelineRun.Id,
                            entity.UniqueId);
                }
            }

            return new PluginResult(true, false);
        }

        private async Task<PluginResult> SummarizeRelationships(
            DataPipelineRun dataPipelineRun,
            string knowledgeBucketId,
            string summarizationModel,
            float summarizationModelTemperature,
            int summarizationMaxOutputTokenCount)
        {
            var positionsMapping = new Dictionary<string, int>();
            var currentRequestPosition = 0;
            var startTime = DateTimeOffset.UtcNow;

            var eligibleRelationships = _entityRelationships.Relationships
                .Where(e =>
                    (e.LastChangedBy == dataPipelineRun.Id // Only summarize entities that have changed in the current data pipeline run.
                    && e.Descriptions.Count > 1)
                    || string.IsNullOrWhiteSpace(e.SummaryDescription));

            var textCompletionRequest = new TextCompletionRequest
            {
                CompletionModelName = summarizationModel,
                CompletionModelParameters = new Dictionary<string, object>
                {
                    { TextOperationModelParameterNames.Temperature, summarizationModelTemperature },
                    { TextOperationModelParameterNames.MaxOutputTokenCount, summarizationMaxOutputTokenCount }
                },
                TextChunks = [.. eligibleRelationships
                    .Select(r =>
                    {
                        var content = _summarizationPrompt
                            .Replace(ENTITY_NAMES_PLACEHOLDER, $"{r.Source},{r.Target}")
                            .Replace(DESCRIPTIONS_LIST_PLACEHOLDER, string.Join(Environment.NewLine, r.Descriptions));
                        currentRequestPosition++;
                        positionsMapping[r.UniqueId] = currentRequestPosition;

                        var textChunk = new TextChunk
                        {
                            Position = currentRequestPosition,
                            Content = content,
                            TokensCount =
                                (int)_tokenizer.CountTokens(content)
                                + summarizationMaxOutputTokenCount
                        };
                        return textChunk;
                    })],
            };

            Dictionary<int, string> completionsDictionary = [];

            if (textCompletionRequest.TextChunks.Count > 0)
            {
                var completionsResult = await _gatewayServiceClient.StartCompletionOperation(
                dataPipelineRun.InstanceId,
                textCompletionRequest);

                while (completionsResult.InProgress)
                {
                    await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                    completionsResult = await _gatewayServiceClient.GetCompletionOperationResult(
                        dataPipelineRun.InstanceId,
                        completionsResult.OperationId!);

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} bucket {BucketId} relationship summarization: {ProcessedEntityCount} of {TotalEntityCount} entities, {TotalSeconds} seconds.",
                        dataPipelineRun.Id,
                        knowledgeBucketId,
                        completionsResult.ProcessedTextChunksCount,
                        textCompletionRequest.TextChunks.Count,
                        (DateTimeOffset.UtcNow - startTime).TotalSeconds);
                }

                if (completionsResult.Failed)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to summarize knowledge graph relationships for data pipeline run {dataPipelineRun.Id} due to a failure in the Gateway API.");

                completionsDictionary = completionsResult.TextChunks.ToDictionary(
                    chunk => chunk.Position,
                    chunk => chunk.Completion!);
            }

            foreach (var relationship in eligibleRelationships)
            {
                if (relationship.Descriptions.Count == 1)
                {
                    relationship.SummaryDescription = relationship.Descriptions[0];
                    relationship.LastChangedBy = dataPipelineRun.RunId;
                }
                else
                {
                    if (completionsDictionary.TryGetValue(positionsMapping[relationship.UniqueId], out var completion))
                    {
                        relationship.SummaryDescription = completion;
                        relationship.LastChangedBy = dataPipelineRun.RunId;
                    }
                    else
                        _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return a summary for the relationship {RelationshipId}.",
                            dataPipelineRun.Id,
                            relationship.UniqueId);
                }
            }

            return new PluginResult(true, false);
        }
    }
}
