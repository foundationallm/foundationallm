using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Graph Consolidation Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphConsolidationDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : KnowledgeGraphDataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_CONSOLIDATION_DATAPIPELINESTAGE;

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<string> contentItemsCanonicalIds,
            string dataPipelineStageName,
            string previousDataPipelineStageName)
        {
            var workItem = new DataPipelineRunWorkItem
            {
                Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                RunId = dataPipelineRun.RunId,
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
            #region Deduplicate entities and relationships

            var contentArtifactsLoadResult = await _dataPipelineStateService.TryLoadDataPipelineRunArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                "content-items/content-items.json");

            if (!contentArtifactsLoadResult.Success
                || contentArtifactsLoadResult.Artifacts is null
                || contentArtifactsLoadResult.Artifacts.Count == 0)
                throw new PluginException("The content items artifact is missing.");

            var contentItemsRegistry = JsonSerializer.Deserialize<ContentItemsRegistry>(
                contentArtifactsLoadResult.Artifacts[0].Content)
                ?? throw new PluginException("The content items artifact is not valid.");

            var contentItemCanonicalIds = contentItemsRegistry.ToAddOrUpdate.Keys.ToList();
            var knowledgeParts = await LoadExtractedKnowledge(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemCanonicalIds);

            foreach (var knowledgePart in knowledgeParts)
                ProcessExtractedKnowledgePart(knowledgePart);

            var entitiesUpdateResults = await UpdateKnowledgeEntities(
                dataPipelineDefinition,
                dataPipelineRun);

            var relationshipsUpdateResults = await UpdateKnowledgeRelationships(
                dataPipelineDefinition,
                dataPipelineRun);

            var knowledgeBucketsResult = await UpdateKnowledgeBucketsRegistry(
                dataPipelineDefinition,
                dataPipelineRun,
                entitiesUpdateResults,
                relationshipsUpdateResults);

            if (!knowledgeBucketsResult.Success)
                return knowledgeBucketsResult;

            #endregion

            return
                new PluginResult(true, false);
        }

        private async Task<List<DataPipelineContentItemKnowledgePart>> LoadExtractedKnowledge(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<string> contentItemCanonicalIds)
        {
            using var semaphore = new SemaphoreSlim(10);

            var loadTasks = contentItemCanonicalIds
                .Select(async contentItemCanonicalId =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await LoadExtractedKnowledge(
                            dataPipelineDefinition,
                            dataPipelineRun,
                            dataPipelineRunWorkItem,
                            contentItemCanonicalId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var knowledgePartsLists = await Task.WhenAll(loadTasks);

            if (knowledgePartsLists.Length < contentItemCanonicalIds.Count)
            {
                _logger.LogWarning("Not all content items have extracted knowledge parts in data pipeline run work item {DataPipelineRunWorkItemId}. Expected {ExpectedCount}, but got {ActualCount}.",
                    dataPipelineRunWorkItem.Id, contentItemCanonicalIds.Count, knowledgePartsLists.Length);
                dataPipelineRunWorkItem.Warnings.Add(
                    $"Not all content items have extracted knowledge parts. Expected {contentItemCanonicalIds.Count}, but got {knowledgePartsLists.Length}.");
            }

            return [.. knowledgePartsLists.SelectMany(x => x)];
        }

        private async Task<List<DataPipelineContentItemKnowledgePart>> LoadExtractedKnowledge(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string contentItemCanonicalId)
        {
            try
            {
                var contentItemKnowledgeParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    contentItemCanonicalId,
                    KNOWLEDGE_PARTS_FILE_NAME);

                var knowledgeParts = contentItemKnowledgeParts?
                    .Where(p => p.EntitiesAndRelationships is not null)
                    .Select(p => p)
                    .ToList();

                return knowledgeParts ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load extracted knowledge for content item {ContentItemCanonicalId} in data pipeline run work item {DataPipelineRunWorkItemId}.",
                    contentItemCanonicalId, dataPipelineRunWorkItem.Id);
                return [];
            }
        }

        private void ProcessExtractedKnowledgePart(
            DataPipelineContentItemKnowledgePart knowledgePart)
        {
            if (string.IsNullOrWhiteSpace(knowledgePart.IndexEntryId))
            {
                throw new PluginException(
                    $"The knowledge part for content item '{knowledgePart.ContentItemCanonicalId}' does not have a valid IndexEntryId.");
            }

            foreach (var entity in knowledgePart.EntitiesAndRelationships!.Entities)
            {
                var existingEntity = _entityRelationships.Entities
                    .FirstOrDefault(
                        x => x.Type.Equals(entity.Type, StringComparison.OrdinalIgnoreCase)
                        && x.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase));
                if (existingEntity != null)
                {
                    existingEntity.Descriptions ??= [];
                    existingEntity.ChunkIds ??= [];

                    existingEntity.Descriptions.Add(entity.Description);
                    existingEntity.ChunkIds.Add(knowledgePart.IndexEntryId!);
                }
                else
                {
                    _entityRelationships.Entities.Add(new KnowledgeEntity
                    {
                        Type = entity.Type,
                        Name = entity.Name,
                        Descriptions = [entity.Description],
                        ChunkIds = [knowledgePart.IndexEntryId!],
                        IndexEntryId = Convert.ToBase64String(
                            MD5.HashData(Encoding.UTF8.GetBytes(
                                $"{entity.Type}-{entity.Name}-{Guid.NewGuid()}")))
                            .Replace("+", "--")
                            .Replace("/", "--")
                    });
                }
            }

            foreach (var relationship in knowledgePart.EntitiesAndRelationships.Relationships)
            {
                var existingRelationship = _entityRelationships.Relationships
                    .FirstOrDefault(
                        x => x.Source.Equals(relationship.Source, StringComparison.OrdinalIgnoreCase)
                        && x.Target.Equals(relationship.Target, StringComparison.OrdinalIgnoreCase));
                if (existingRelationship != null)
                {
                    existingRelationship.ShortDescriptions ??= [];
                    existingRelationship.Descriptions ??= [];
                    existingRelationship.ChunkIds ??= [];
                    existingRelationship.Strengths ??= [];

                    existingRelationship.ShortDescriptions.Add(relationship.ShortDescription);
                    existingRelationship.Descriptions.Add(relationship.Description);
                    existingRelationship.ChunkIds.Add(knowledgePart.IndexEntryId!);
                    existingRelationship.Strengths.Add(relationship.Strength);
                }
                else
                {
                    var sourceType = knowledgePart
                            .EntitiesAndRelationships
                            .Entities
                            .FirstOrDefault(e => e.Name == relationship.Source)?
                            .Type
                            ?? "N/A";
                    var targetType = knowledgePart
                            .EntitiesAndRelationships
                            .Entities
                            .FirstOrDefault(e => e.Name == relationship.Target)?
                            .Type
                            ?? "N/A";

                    _entityRelationships.Relationships.Add(new KnowledgeRelationship
                    {
                        Source = relationship.Source,
                        SourceType = sourceType,
                        Target = relationship.Target,
                        TargetType = targetType,
                        Strengths = [relationship.Strength],
                        ShortDescriptions = [relationship.ShortDescription],
                        Descriptions = [relationship.Description],
                        ChunkIds = [knowledgePart.IndexEntryId!],
                        IndexEntryId = Convert.ToBase64String(
                            MD5.HashData(Encoding.UTF8.GetBytes(
                                $"{relationship.Source}-{sourceType}-{relationship.Target}-{targetType}-{Guid.NewGuid()}")))
                            .Replace("+", "--")
                            .Replace("/", "--")
                    });
                }
            }

            foreach (var entity in _entityRelationships.Entities)
                entity.DescriptionsHash = ComputeHash(entity.ConsolidatedDescriptions);

            foreach (var relationship in _entityRelationships.Relationships)
                relationship.DescriptionsHash = ComputeHash(relationship.ConsolidatedDescriptions);
        }

        private async Task<Dictionary<string, bool>> UpdateKnowledgeEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            var entityBuckets = _entityRelationships.Entities
                .GroupBy(e => e.BucketId)
                .ToList();

            using var semaphore = new SemaphoreSlim(10);

            var updateTasks = entityBuckets
                .Select(async g =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await UpdateKnowledgeEntities(
                            dataPipelineDefinition,
                            dataPipelineRun,
                            g.Key,
                            g);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var updateTaskResults = await Task.WhenAll(updateTasks);

            var successResults = updateTaskResults.Count(r => r.Success);
            if (successResults < entityBuckets.Count)
            {
                _logger.LogWarning("Not all entity buckets were updated successfully in data pipeline run {DataPipelineRunId}. Expected {ExpectedCount}, but got {ActualCount}.",
                    dataPipelineRun.Id, entityBuckets.Count, successResults);
                // We do not fail the entire process if some buckets fail to update.
            }

            return updateTaskResults.ToDictionary(
                k => k.BucketId,
                v => v.Success);
        }

        private async Task<(string BucketId, bool Success)> UpdateKnowledgeEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string bucketId,
            IEnumerable<KnowledgeEntity> knowledgeEntities)
        {
            try
            {
                var knowledgeEntitiesFilePath = string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                bucketId,
                KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                ]);
                var existingKnowledgeEntites = (await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeEntity>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    knowledgeEntitiesFilePath)).ToList();

                var newKnowledgeEntities = knowledgeEntities
                    .ToDictionary(ke => ke.UniqueId);

                foreach (var existingKnowledgeEntity in existingKnowledgeEntites)
                {
                    if (newKnowledgeEntities.TryGetValue(existingKnowledgeEntity.UniqueId, out var newKnowledgeEntity))
                    {
                        if (existingKnowledgeEntity.DescriptionsHash == newKnowledgeEntity.DescriptionsHash)
                        {
                            // No changes, skip
                            newKnowledgeEntities.Remove(existingKnowledgeEntity.UniqueId);
                            continue;
                        }

                        existingKnowledgeEntity.Descriptions = newKnowledgeEntity.Descriptions;
                        existingKnowledgeEntity.ChunkIds = newKnowledgeEntity.ChunkIds;
                        existingKnowledgeEntity.LastChangedBy = dataPipelineRun.RunId;
                        newKnowledgeEntities.Remove(existingKnowledgeEntity.UniqueId);
                    }
                }

                if (newKnowledgeEntities.Count > 0)
                {
                    foreach (var newKnowledgeEntity in newKnowledgeEntities.Values)
                    {
                        newKnowledgeEntity.LastChangedBy = dataPipelineRun.RunId;
                        existingKnowledgeEntites.Add(newKnowledgeEntity);
                    }
                }

                await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeEntity>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    existingKnowledgeEntites,
                    knowledgeEntitiesFilePath);

                return (bucketId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update knowledge entities for bucket {BucketId} in data pipeline run {DataPipelineRunId}.",
                    bucketId, dataPipelineRun.Id);
                return (bucketId, false);
            }
        }

        private async Task<Dictionary<string, bool>> UpdateKnowledgeRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            var relationshipBuckets = _entityRelationships.Relationships
                .GroupBy(e => e.BucketId)
                .ToList();

            using var semaphore = new SemaphoreSlim(10);

            var updateTasks = relationshipBuckets
                .Select(async g =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await UpdateKnowledgeRelationships(
                            dataPipelineDefinition,
                            dataPipelineRun,
                            g.Key,
                            g);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var updateTaskResults = await Task.WhenAll(updateTasks);

            var successResults = updateTaskResults.Count(r => r.Success);
            if (successResults < relationshipBuckets.Count)
            {
                _logger.LogWarning("Not all relationship buckets were updated successfully in data pipeline run {DataPipelineRunId}. Expected {ExpectedCount}, but got {ActualCount}.",
                    dataPipelineRun.Id, relationshipBuckets.Count, successResults);
                // We do not fail the entire process if some buckets fail to update.
            }

            return updateTaskResults.ToDictionary(
                k => k.BucketId,
                v => v.Success);
        }

        private async Task<(string BucketId, bool Success)> UpdateKnowledgeRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string bucketId,
            IEnumerable<KnowledgeRelationship> knowledgeRelationships)
        {
            try
            {
                var knowledgeRelationshipsFilePath = string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                bucketId,
                KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                ]);
                var existingKnowledgeRelationships = (await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeRelationship>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    knowledgeRelationshipsFilePath)).ToList();

                var newKnowledgeRelationships = knowledgeRelationships
                    .ToDictionary(ke => ke.UniqueId);

                foreach (var existingKnowledgeRelationship in existingKnowledgeRelationships)
                {
                    if (newKnowledgeRelationships.TryGetValue(existingKnowledgeRelationship.UniqueId, out var newKnowledgeRelationship))
                    {
                        if (existingKnowledgeRelationship.DescriptionsHash == newKnowledgeRelationship.DescriptionsHash)
                        {
                            // No changes, skip
                            newKnowledgeRelationships.Remove(existingKnowledgeRelationship.UniqueId);
                            continue;
                        }

                        existingKnowledgeRelationship.Descriptions = newKnowledgeRelationship.Descriptions;
                        existingKnowledgeRelationship.ChunkIds = newKnowledgeRelationship.ChunkIds;
                        existingKnowledgeRelationship.LastChangedBy = dataPipelineRun.RunId;
                        newKnowledgeRelationships.Remove(existingKnowledgeRelationship.UniqueId);
                    }
                }

                if (newKnowledgeRelationships.Count > 0)
                {
                    foreach (var newKnowledgeEntity in newKnowledgeRelationships.Values)
                    {
                        newKnowledgeEntity.LastChangedBy = dataPipelineRun.RunId;
                        existingKnowledgeRelationships.Add(newKnowledgeEntity);
                    }
                }

                await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeRelationship>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    existingKnowledgeRelationships,
                    knowledgeRelationshipsFilePath);

                return (bucketId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update knowledge relationships for bucket {BucketId} in data pipeline run {DataPipelineRunId}.",
                    bucketId, dataPipelineRun.Id);
                return (bucketId, false);
            }
        }

        private async Task<PluginResult> UpdateKnowledgeBucketsRegistry(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            Dictionary<string, bool> entitiesUpdateResults,
            Dictionary<string, bool> relationshipsUpdateResults)
        {
            try
            {
                var knowledgeBucketsRegistry = await LoadKnowledgeBucketsRegistry(
                    dataPipelineDefinition,
                    dataPipelineRun);

                foreach (var entityBucketResult in entitiesUpdateResults.Where(r => r.Value))
                    if (knowledgeBucketsRegistry.Entities.TryGetValue(entityBucketResult.Key, out var entityBucket))
                        entityBucket.LastModifiedAt = DateTimeOffset.UtcNow;
                    else
                        knowledgeBucketsRegistry.Entities[entityBucketResult.Key] = new KnowledgeBucketsRegistryEntry
                        {
                            BucketId = entityBucketResult.Key,
                            LastModifiedAt = DateTimeOffset.UtcNow
                        };

                foreach (var relationshipBucketResult in relationshipsUpdateResults.Where(r => r.Value))
                    if (knowledgeBucketsRegistry.Relationships.TryGetValue(relationshipBucketResult.Key, out var relationshipBucket))
                        relationshipBucket.LastModifiedAt = DateTimeOffset.UtcNow;
                    else
                        knowledgeBucketsRegistry.Relationships[relationshipBucketResult.Key] = new KnowledgeBucketsRegistryEntry
                        {
                            BucketId = relationshipBucketResult.Key,
                            LastModifiedAt = DateTimeOffset.UtcNow
                        };

                await SaveKnowledgeBucketsRegistry(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    knowledgeBucketsRegistry);

                return new PluginResult(true, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update knowledge buckets registry in data pipeline run {DataPipelineRunId}.",
                    dataPipelineRun.Id);
                return new PluginResult(false, true,
                    $"The {Name} plugin failed to update the knowledge buckets registry for data pipeline run {dataPipelineRun.Id} due to an internal error.");
            }
        }
    }
}
