using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Graph Publishing Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphPublishingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : KnowledgeGraphDataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_PUBLISHING_DATAPIPELINESTAGE;

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

            #endregion

            #region Consolidate all knowledge entities and relationships.

            var knowledgeBucketsRegistry = await LoadKnowledgeBucketsRegistry(
                dataPipelineDefinition,
                dataPipelineRun);

            var allKnowledgeEntities = await LoadKnowledgeEntities(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                [.. knowledgeBucketsRegistry.Entities.Keys]);

            _logger.LogInformation("Consolidated all knowledge entities. Total count: {KnowledgeEntitiesCount}.",
                allKnowledgeEntities.Count);

            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeEntity>(
                dataPipelineDefinition,
                dataPipelineRun,
                allKnowledgeEntities,
                string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                ]));

            _logger.LogInformation("Saved all knowledge entities. Total count: {KnowledgeEntitiesCount}.",
                allKnowledgeEntities.Count);

            var allKnowledgeRelationships = await LoadKnowledgeRelationships(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                [.. knowledgeBucketsRegistry.Relationships.Keys]);

            _logger.LogInformation("Consolidated all knowledge relationships. Total count: {KnowledgeRelationshipsCount}.",
                allKnowledgeRelationships.Count);

            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeRelationship>(
                dataPipelineDefinition,
                dataPipelineRun,
                allKnowledgeRelationships,
                string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                ]));

            _logger.LogInformation("Saved all knowledge relationships. Total count: {KnowledgeRelationshipsCount}.",
                allKnowledgeRelationships.Count);

            #endregion

            var canonicalRootPath = _dataPipelineStateService.GetDataPipelineCanonicalRootPath(
                dataPipelineDefinition,
                dataPipelineRun);

            var contextResult = await _contextResourceProvider.ExecuteResourceActionAsync<
                KnowledgeUnit,
                ContextKnowledgeUnitSetGraphRequest,
                ResourceProviderActionResult>(
                dataPipelineRun.InstanceId,
                knowledgeUnit.Name,
                ResourceProviderActions.SetGraph,
                new ContextKnowledgeUnitSetGraphRequest
                {
                    EntitiesSourceFilePath = string.Join('/', [
                        canonicalRootPath,
                        KNOWLEDGE_GRAPH_ROOT_PATH,
                        KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                    ]),
                    RelationshipsSourceFilePath = string.Join('/', [
                        canonicalRootPath,
                        KNOWLEDGE_GRAPH_ROOT_PATH,
                        KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                    ])
                },
                ServiceContext.ServiceIdentity!);

            if (!contextResult.IsSuccess)
                return new PluginResult(false, false,
                    $"The {Name} plugin failed to publish the knowledge graph for data pipeline run {dataPipelineRun.Id} due to a failure in the Context resource provider: {contextResult.ErrorMessage ?? "N/A"}");

            return
                new PluginResult(true, false);
        }

        private async Task<List<KnowledgeEntity>> LoadKnowledgeEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<string> knowledgeBucketIds)
        {
            using var semaphore = new SemaphoreSlim(10);

            var loadTasks = knowledgeBucketIds
                .Select(async knowledgeBucketId =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await LoadKnowledgeEntities(
                            dataPipelineDefinition,
                            dataPipelineRun,
                            dataPipelineRunWorkItem,
                            knowledgeBucketId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var knowledgeEntitiesLists = await Task.WhenAll(loadTasks);

            if (knowledgeEntitiesLists.Length < knowledgeBucketIds.Count)
            {
                _logger.LogWarning("Not all knowledge buckets have knowledge entities in data pipeline run work item {DataPipelineRunWorkItemId}. Expected {ExpectedCount}, but got {ActualCount}.",
                    dataPipelineRunWorkItem.Id, knowledgeBucketIds.Count, knowledgeEntitiesLists.Length);
                dataPipelineRunWorkItem.Warnings.Add(
                    $"Not all knowledge buckets have knowledge entities. Expected {knowledgeBucketIds.Count}, but got {knowledgeEntitiesLists.Length}.");
            }

            return [.. knowledgeEntitiesLists.SelectMany(x => x)];
        }

        private async Task<List<KnowledgeEntity>> LoadKnowledgeEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string knowledgeBucketId)
        {
            try
            {
                var knowledgeEntities = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<KnowledgeEntity>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    knowledgeBucketId,
                    KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME,
                    contentSection: KNOWLEDGE_GRAPH_ROOT_PATH);

                return knowledgeEntities?.ToList() ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load knowledge entities for bucket {KnowledgeBucketId} in data pipeline run work item {DataPipelineRunWorkItemId}.",
                    knowledgeBucketId, dataPipelineRunWorkItem.Id);
                return [];
            }
        }

        private async Task<List<KnowledgeRelationship>> LoadKnowledgeRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<string> knowledgeBucketIds)
        {
            using var semaphore = new SemaphoreSlim(10);

            var loadTasks = knowledgeBucketIds
                .Select(async knowledgeBucketId =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await LoadKnowledgeRelationships(
                            dataPipelineDefinition,
                            dataPipelineRun,
                            dataPipelineRunWorkItem,
                            knowledgeBucketId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var knowledgeRelationshipsLists = await Task.WhenAll(loadTasks);

            if (knowledgeRelationshipsLists.Length < knowledgeBucketIds.Count)
            {
                _logger.LogWarning("Not all knowledge buckets have knowledge relationships in data pipeline run work item {DataPipelineRunWorkItemId}. Expected {ExpectedCount}, but got {ActualCount}.",
                    dataPipelineRunWorkItem.Id, knowledgeBucketIds.Count, knowledgeRelationshipsLists.Length);
                dataPipelineRunWorkItem.Warnings.Add(
                    $"Not all knowledge buckets have knowledge relationships. Expected {knowledgeBucketIds.Count}, but got {knowledgeRelationshipsLists.Length}.");
            }

            return [.. knowledgeRelationshipsLists.SelectMany(x => x)];
        }

        private async Task<List<KnowledgeRelationship>> LoadKnowledgeRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string knowledgeBucketId)
        {
            try
            {
                var knowledgeRelationships = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<KnowledgeRelationship>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    knowledgeBucketId,
                    KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME,
                    contentSection: KNOWLEDGE_GRAPH_ROOT_PATH);

                return knowledgeRelationships?.ToList() ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load knowledge relationships for bucket {KnowledgeBucketId} in data pipeline run work item {DataPipelineRunWorkItemId}.",
                    knowledgeBucketId, dataPipelineRunWorkItem.Id);
                return [];
            }
        }
    }
}
