using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
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
    public class KnowledgeGraphEmbeddingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : KnowledgeGraphDataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_EMBEDDING_DATAPIPELINESTAGE;

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
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                out var knowledgeUnitObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID} parameter.");

            var knowledgeUnit = await _contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                knowledgeUnitObjectId.ToString()!,
                ServiceContext.ServiceIdentity!);

            if (!knowledgeUnit.HasKnowledgeGraph)
                throw new PluginException(
                    $"The knowledge unit {knowledgeUnit.Name} cannot be used by the {Name} plugin because it does not have a knowledge graph.");

            var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                knowledgeUnit.KnowledgeGraphVectorDatabaseObjectId!,
                ServiceContext.ServiceIdentity!);

            #endregion

            await CreateGatewayServiceClient(dataPipelineRun.InstanceId);

            if (dataPipelineRunWorkItem.ContentItemCanonicalId.StartsWith(
                KNOWLEDGE_GRAPH_ENTITY))
            {
                await LoadEntities(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                await LoadEntitiesEmbeddings(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                var entitiesEmbeddingResult = await EmbedEntities(
                    dataPipelineRun,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    vectorDatabase.EmbeddingModel,
                    vectorDatabase.EmbeddingDimensions);

                if (!entitiesEmbeddingResult.Success)
                    return entitiesEmbeddingResult;

                await SaveEntities(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                await SaveEntitiesEmbeddings(
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

                await LoadRelationshipsEmbeddings(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                var relationshipsEmbeddingResult = await EmbedRelationships(
                    dataPipelineRun,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    vectorDatabase.EmbeddingModel,
                    vectorDatabase.EmbeddingDimensions);

                if (!relationshipsEmbeddingResult.Success)
                    return relationshipsEmbeddingResult;

                await SaveRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem);

                await SaveRelationshipsEmbeddings(
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

        private async Task<PluginResult> EmbedEntities(
            DataPipelineRun dataPipelineRun,
            string knowledgeBucketId,
            string embeddingModel,
            int embeddingDimensions)
        {
            var entitiesToKeep = _entityRelationships.Entities
                .Where(e =>
                    e.LastChangedBy != dataPipelineRun.RunId
                    && !string.IsNullOrWhiteSpace(e.SummaryDescription)
                    && _entitiesEmbeddings.TryGetValue(e.UniqueId, out var existingEmbedding)
                    && existingEmbedding is not null
                    && existingEmbedding.SummaryDescriptionEmbedding is not null)
                .ToList();

            var entityIdsToKeep = entitiesToKeep
                .Select(e => e.UniqueId)
                .ToHashSet();

            _entitiesEmbeddings = _entitiesEmbeddings
                .Where(kv => entityIdsToKeep.Contains(kv.Key))
                .ToDictionary();

            var entitiesToEmbed = _entityRelationships.Entities
                .Where(e => !entityIdsToKeep.Contains(e.UniqueId))
                .ToList();

            var positionsMapping = new Dictionary<string, int>();
            var currentRequestPosition = 0;
            var startTime = DateTimeOffset.UtcNow;

            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = embeddingModel,
                EmbeddingModelDimensions = embeddingDimensions,
                Prioritized = false,
                TextChunks = [.. entitiesToEmbed
                    .Select(e =>
                    {
                        currentRequestPosition++;
                        positionsMapping[e.UniqueId] = currentRequestPosition;

                        var textChunk = new TextChunk
                        {
                            Position = currentRequestPosition,
                            Content = e.SummaryDescription,
                            TokensCount = (int)_tokenizer.CountTokens(e.SummaryDescription!)
                        };
                        return textChunk;
                    })]
            };

            Dictionary<int, Embedding?> embeddingsDictionary = [];

            if (textEmbeddingRequest.TextChunks.Count > 0)
            {
                var embeddingResult = await _gatewayServiceClient.StartEmbeddingOperation(
                    dataPipelineRun.InstanceId,
                    textEmbeddingRequest);

                while (embeddingResult.InProgress)
                {
                    await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                    embeddingResult = await _gatewayServiceClient.GetEmbeddingOperationResult(
                        dataPipelineRun.InstanceId,
                        embeddingResult.OperationId!);

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} bucket {BucketId} entity embedding: {ProcessedEntityCount} of {TotalEntityCount} entities, {TotalSeconds} seconds.",
                        dataPipelineRun.Id,
                        knowledgeBucketId,
                        embeddingResult.ProcessedTextChunksCount,
                        textEmbeddingRequest.TextChunks.Count,
                        (DateTimeOffset.UtcNow - startTime).TotalSeconds);
                }

                if (embeddingResult.Failed)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to embed knowledge graph entities for data pipeline run {dataPipelineRun.Id} due to a failure in the Gateway API.");

                embeddingsDictionary = embeddingResult.TextChunks.ToDictionary(
                    chunk => chunk.Position,
                    chunk => chunk.Embedding);
            }

            foreach (var entity in entitiesToEmbed)
            {
                if (embeddingsDictionary.TryGetValue(positionsMapping[entity.UniqueId], out var embedding))
                {
                    entity.LastChangedBy = dataPipelineRun.RunId;
                    _entitiesEmbeddings.Add(
                        entity.UniqueId,
                        new Common.Models.Knowledge.KnowledgeEntityEmbedding
                        {
                            Type = entity.Type,
                            Name = entity.Name,
                            SummaryDescriptionEmbedding = embedding!.Value.Vector.ToArray(),
                            IndexEntryId = entity.IndexEntryId,
                            LastChangedBy = dataPipelineRun.RunId
                        });
                }
                else
                    _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return an embedding for the entity {EntityId}.",
                        dataPipelineRun.Id,
                        entity.UniqueId);
            }

            return new PluginResult(true, false);
        }

        private async Task<PluginResult> EmbedRelationships(
            DataPipelineRun dataPipelineRun,
            string knowledgeBucketId,
            string embeddingModel,
            int embeddingDimensions)
        {
            var relationshipsToKeep = _entityRelationships.Relationships
                .Where(r =>
                    r.LastChangedBy != dataPipelineRun.RunId
                    && !string.IsNullOrWhiteSpace(r.SummaryDescription)
                    && _relationshipsEmbeddings.TryGetValue(r.UniqueId, out var existingEmbedding)
                    && existingEmbedding is not null
                    && existingEmbedding.SummaryDescriptionEmbedding is not null)
                .ToList();

            var relationshipIdsToKeep = relationshipsToKeep
                .Select(r => r.UniqueId)
                .ToHashSet();

            _relationshipsEmbeddings = _relationshipsEmbeddings
                .Where(kv => relationshipIdsToKeep.Contains(kv.Key))
                .ToDictionary();

            var relationshipsToEmbed = _entityRelationships.Relationships
                .Where(r => !relationshipIdsToKeep.Contains(r.UniqueId))
                .ToList();

            var positionsMapping = new Dictionary<string, int>();
            var currentRequestPosition = 0;
            var startTime = DateTimeOffset.UtcNow;

            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = embeddingModel,
                EmbeddingModelDimensions = embeddingDimensions,
                Prioritized = false,
                TextChunks = [.. relationshipsToEmbed
                    .Select(r =>
                    {
                        currentRequestPosition++;
                        positionsMapping[r.UniqueId] = currentRequestPosition;

                        var textChunk = new TextChunk
                        {
                            Position = currentRequestPosition,
                            Content = r.SummaryDescription,
                            TokensCount = (int)_tokenizer.CountTokens(r.SummaryDescription!)
                        };
                        return textChunk;
                    })]
            };

            Dictionary<int, Embedding?> embeddingsDictionary = [];

            if (textEmbeddingRequest.TextChunks.Count > 0)
            {
                var embeddingResult = await _gatewayServiceClient.StartEmbeddingOperation(
                    dataPipelineRun.InstanceId,
                    textEmbeddingRequest);

                while (embeddingResult.InProgress)
                {
                    await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                    embeddingResult = await _gatewayServiceClient.GetEmbeddingOperationResult(
                        dataPipelineRun.InstanceId,
                        embeddingResult.OperationId!);

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} bucket {BucketId} relationship embedding: {ProcessedEntityCount} of {TotalEntityCount} relationships, {TotalSeconds} seconds.",
                        dataPipelineRun.Id,
                        knowledgeBucketId,
                        embeddingResult.ProcessedTextChunksCount,
                        textEmbeddingRequest.TextChunks.Count,
                        (DateTimeOffset.UtcNow - startTime).TotalSeconds);
                }

                if (embeddingResult.Failed)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to embed knowledge graph relationships for data pipeline run {dataPipelineRun.Id} due to a failure in the Gateway API.");

                embeddingsDictionary = embeddingResult.TextChunks.ToDictionary(
                    chunk => chunk.Position,
                    chunk => chunk.Embedding);
            }

            foreach (var relationship in _entityRelationships.Relationships)
            {
                if (embeddingsDictionary.TryGetValue(positionsMapping[relationship.UniqueId], out var embedding))
                {
                    relationship.LastChangedBy = dataPipelineRun.RunId;
                    _relationshipsEmbeddings.Add(
                        relationship.UniqueId,
                        new KnowledgeRelationshipEmbedding
                        {
                            SourceType = relationship.SourceType,
                            Source = relationship.Source,
                            TargetType = relationship.TargetType,
                            Target = relationship.Target,
                            SummaryDescriptionEmbedding = embedding!.Value.Vector.ToArray(),
                            IndexEntryId = relationship.IndexEntryId,
                            LastChangedBy = dataPipelineRun.RunId
                        });
                }
                else
                    _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return an embedding for the relationship {RelationshipId}.",
                        dataPipelineRun.Id,
                        relationship.UniqueId);
            }

            return new PluginResult(true, false);
        }
    }
}
