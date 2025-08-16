using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Gateway;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Plugins.DataPipeline.Constants;
using FoundationaLLM.Plugins.DataPipeline.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Graph Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE;

        private const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";
        private const string KNOWLEDGE_ENTITIES_FILE_PATH = "knowledge-graph/knowledge-graph-entities.parquet";
        private const string KNOWLEDGE_RELATIONSHIPS_FILE_PATH = "knowledge-graph/knowledge-graph-relationships.parquet";
        private const string ENTITY_NAMES_PLACEHOLDER = "{entity_names}";
        private const string DESCRIPTIONS_LIST_PLACEHOLDER = "{descriptions_list}";

        private const int GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS = 5;

        private readonly IResourceProviderService _promptResourceProvider =
            serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Prompt)
            ?? throw new PluginException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} is not available in the dependency injection container.");
        private readonly ITokenizerService _tokenizer =
            serviceProvider.GetRequiredKeyedService<ITokenizerService>("MicrosoftML")
            ?? throw new PluginException("The MicrosoftML tokenizer service is not available in the dependency injection container.");

        private KnowledgeGraphBuildingState _knowledgeGraphBuildingState = null!;
        private GatewayServiceClient _gatewayServiceClient = null!;
        private string _summarizationPrompt = null!;

        private readonly KnowledgeEntityRelationshipCollection<KnowledgeEntity, KnowledgeRelationship> _entityRelationships = new();

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
            #region Load state

            await LoadKnowledgeGraphState(
                dataPipelineDefinition,
                dataPipelineRun);

            var lastSuccessfullStep = _knowledgeGraphBuildingState.LastSuccessfullStep ?? string.Empty;
            var knowledgeGrapBuildingStep =
                lastSuccessfullStep switch
                {
                    "" => KnowledgeGraphBuildingSteps.CoreStructure,
                    KnowledgeGraphBuildingSteps.CoreStructure => KnowledgeGraphBuildingSteps.EntitiesSummarization,
                    KnowledgeGraphBuildingSteps.EntitiesSummarization => KnowledgeGraphBuildingSteps.RelationshipsSummarization,
                    KnowledgeGraphBuildingSteps.RelationshipsSummarization => KnowledgeGraphBuildingSteps.EntitiesEmbedding,
                    KnowledgeGraphBuildingSteps.EntitiesEmbedding => KnowledgeGraphBuildingSteps.RelationshipsEmbedding,
                    KnowledgeGraphBuildingSteps.RelationshipsEmbedding => KnowledgeGraphBuildingSteps.Publish,
                    KnowledgeGraphBuildingSteps.Publish => null, // No more steps to process
                    _ => throw new PluginException($"Unknown knowledge graph building step: {lastSuccessfullStep}.")
                };

            if (knowledgeGrapBuildingStep is null)
            {
                _logger.LogInformation("Knowledge graph building for data pipeline run {DataPipelineRunId} is already complete.",
                    dataPipelineRun.Id);
                return new PluginResult(true, false);
            }

            _logger.LogInformation("Starting knowledge graph building for data pipeline run {DataPipelineRunId} at step {KnowledgeGraphBuildingStep}.",
                dataPipelineRun.Id,
                knowledgeGrapBuildingStep);

            #endregion

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

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGESOURCEID,
                out var knowledgeSourceId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGESOURCEID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID,
                out var vectorDatabaseObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_VECTORSTOREID,
                out var vectorStoreId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_VECTORSTOREID} parameter.");

            #endregion

            if (knowledgeGrapBuildingStep == KnowledgeGraphBuildingSteps.CoreStructure)
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

                var contentItemCanonicalIds = JsonSerializer.Deserialize<List<string>>(
                    contentArtifactsLoadResult.Artifacts[0].Content)
                    ?? throw new PluginException("The content items artifact is not valid.");

                var knowledgeParts = await LoadExtractedKnowledge(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    dataPipelineRunWorkItem,
                    contentItemCanonicalIds);

                foreach (var knowledgePart in knowledgeParts)
                    AddExtractedKnowledgePart(knowledgePart);

                await SaveEntities(
                    dataPipelineDefinition,
                    dataPipelineRun);

                await SaveRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #endregion

                _knowledgeGraphBuildingState.LastSuccessfullStep =
                    KnowledgeGraphBuildingSteps.CoreStructure;
                await SaveKnowledgeGrapState(dataPipelineDefinition, dataPipelineRun);
                knowledgeGrapBuildingStep =
                    KnowledgeGraphBuildingSteps.EntitiesSummarization; // Move to the next step
            }

            if (knowledgeGrapBuildingStep ==
                KnowledgeGraphBuildingSteps.EntitiesSummarization)
            {
                await CreateGatewayServiceClient();
                await LoadSummarizationPrompt(
                    entitySummarizationPromptId.ToString()!);
                await LoadEntities(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #region Summarize entity descriptions

                var entitiesSummarizationResult = await SummarizeEntities(
                    dataPipelineRun,
                    entitySummarizationCompletionModel.ToString()!,
                    (float)(double)entitySummarizationCompletionModelTemperature,
                    (int)entitySummarizationCompletionMaxOutputTokenCount);

                if (!entitiesSummarizationResult.Success)
                    return entitiesSummarizationResult;

                await SaveEntities(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #endregion

                _knowledgeGraphBuildingState.LastSuccessfullStep =
                    KnowledgeGraphBuildingSteps.EntitiesSummarization;
                await SaveKnowledgeGrapState(dataPipelineDefinition, dataPipelineRun);
                knowledgeGrapBuildingStep =
                    KnowledgeGraphBuildingSteps.RelationshipsSummarization; // Move to the next step
            }

            if (knowledgeGrapBuildingStep ==
                KnowledgeGraphBuildingSteps.RelationshipsSummarization)
            {
                await CreateGatewayServiceClient();
                await LoadSummarizationPrompt(
                    entitySummarizationPromptId.ToString()!);
                await LoadRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #region Summarize relationship descriptions

                var relationshipsSummarizationResult = await SummarizeRelationships(
                    dataPipelineRun,
                    entitySummarizationCompletionModel.ToString()!,
                    (float)(double)entitySummarizationCompletionModelTemperature,
                    (int)entitySummarizationCompletionMaxOutputTokenCount);

                if (!relationshipsSummarizationResult.Success)
                    return relationshipsSummarizationResult;

                await SaveRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #endregion

                _knowledgeGraphBuildingState.LastSuccessfullStep =
                    KnowledgeGraphBuildingSteps.RelationshipsSummarization;
                await SaveKnowledgeGrapState(dataPipelineDefinition, dataPipelineRun);
                knowledgeGrapBuildingStep =
                    KnowledgeGraphBuildingSteps.EntitiesEmbedding; // Move to the next step
            }

            if (knowledgeGrapBuildingStep ==
                KnowledgeGraphBuildingSteps.EntitiesEmbedding)
            {
                await CreateGatewayServiceClient();
                await LoadSummarizationPrompt(
                    entitySummarizationPromptId.ToString()!);
                await LoadEntities(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #region Embed entity descriptions

                var entitiesEmbeddingResult = await EmbedEntities(
                    dataPipelineRun,
                    entitySummarizationEmbeddingModel.ToString()!,
                    (int)entitySummarizationEmbeddingDimensions);

                if (!entitiesEmbeddingResult.Success)
                    return entitiesEmbeddingResult;

                await SaveEntities(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #endregion

                _knowledgeGraphBuildingState.LastSuccessfullStep =
                    KnowledgeGraphBuildingSteps.EntitiesEmbedding;
                await SaveKnowledgeGrapState(dataPipelineDefinition, dataPipelineRun);
                knowledgeGrapBuildingStep =
                    KnowledgeGraphBuildingSteps.RelationshipsEmbedding; // Move to the next step
            }

            if (knowledgeGrapBuildingStep ==
                KnowledgeGraphBuildingSteps.RelationshipsEmbedding)
            {
                await CreateGatewayServiceClient();
                await LoadSummarizationPrompt(
                    entitySummarizationPromptId.ToString()!);
                await LoadRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #region Embed relationship descriptions

                var relationshipsEmbeddingResult = await EmbedRelationships(
                    dataPipelineRun,
                    entitySummarizationEmbeddingModel.ToString()!,
                    (int)entitySummarizationEmbeddingDimensions);

                if (!relationshipsEmbeddingResult.Success)
                    return relationshipsEmbeddingResult;

                await SaveRelationships(
                    dataPipelineDefinition,
                    dataPipelineRun);

                #endregion

                _knowledgeGraphBuildingState.LastSuccessfullStep =
                    KnowledgeGraphBuildingSteps.RelationshipsEmbedding;
                await SaveKnowledgeGrapState(dataPipelineDefinition, dataPipelineRun);
                knowledgeGrapBuildingStep =
                    KnowledgeGraphBuildingSteps.Publish; // Move to the next step.
            }

            if (knowledgeGrapBuildingStep ==
                    KnowledgeGraphBuildingSteps.Publish)
            {
                #region Publish knowledge graph to the Context API

                using var scope = _serviceProvider.CreateScope();

                var contextServiceClient = new ContextServiceClient(
                    new OrchestrationContext { CurrentUserIdentity = ServiceContext.ServiceIdentity },
                    scope.ServiceProvider.GetRequiredService<IHttpClientFactoryService>(),
                    scope.ServiceProvider.GetRequiredService<ILogger<ContextServiceClient>>());

                var canonicalRootPath = _dataPipelineStateService.GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun);
                var response = await contextServiceClient.SetKnowledgeUnitGraph(
                    dataPipelineRun.InstanceId,
                    knowledgeSourceId.ToString()!,
                    new ContextKnowledgeUnitSetGraphRequest
                    {
                        EntitiesSourceFilePath = $"{canonicalRootPath}/{KNOWLEDGE_ENTITIES_FILE_PATH}",
                        RelationshipsSourceFilePath = $"{canonicalRootPath}/{KNOWLEDGE_RELATIONSHIPS_FILE_PATH}"
                    });

                if (!response.Success)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to publish the knowledge graph for data pipeline run {dataPipelineRun.Id} due to a failure in the Context API: {response.ErrorMessage}");

                #endregion

                _knowledgeGraphBuildingState.LastSuccessfullStep =
                    KnowledgeGraphBuildingSteps.Publish;
                await SaveKnowledgeGrapState(dataPipelineDefinition, dataPipelineRun);
            }

            return
                new PluginResult(true, false);
        }

        private async Task LoadKnowledgeGraphState(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            var graphStateLoadResult = await _dataPipelineStateService.TryLoadDataPipelineRunArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                "knowledge-graph/knowledge-graph-state.json");

            if (!graphStateLoadResult.Success
                || graphStateLoadResult.Artifacts is null
                || graphStateLoadResult.Artifacts.Count == 0)
            {
                _knowledgeGraphBuildingState = new KnowledgeGraphBuildingState();
                await _dataPipelineStateService.SaveDataPipelineRunArtifacts(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    [
                        new DataPipelineStateArtifact
                        {
                            FileName = "knowledge-graph/knowledge-graph-state.json",
                            Content = BinaryData.FromString(JsonSerializer.Serialize(_knowledgeGraphBuildingState)),
                            ContentType = "application/json"
                        }
                    ]);
            }
            else
            {
                _knowledgeGraphBuildingState = JsonSerializer.Deserialize<KnowledgeGraphBuildingState>(
                    graphStateLoadResult.Artifacts[0].Content)!;
            }
        }

        private async Task SaveKnowledgeGrapState(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun) =>
            await _dataPipelineStateService.SaveDataPipelineRunArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                [
                    new DataPipelineStateArtifact
                    {
                        FileName = "knowledge-graph/knowledge-graph-state.json",
                        Content = BinaryData.FromString(JsonSerializer.Serialize(_knowledgeGraphBuildingState)),
                        ContentType = "application/json"
                    }
                ]);

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

        private void AddExtractedKnowledgePart(
            DataPipelineContentItemKnowledgePart knowledgePart)
        {
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
                        Position = _entityRelationships.Entities.Count + 1,
                        Type = entity.Type,
                        Name = entity.Name,
                        Descriptions = [entity.Description],
                        ChunkIds = [knowledgePart.IndexEntryId]
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
                    _entityRelationships.Relationships.Add(new KnowledgeRelationship
                    {
                        Position = _entityRelationships.Relationships.Count + 1,
                        Source = relationship.Source,
                        SourceType = knowledgePart
                            .EntitiesAndRelationships
                            .Entities
                            .FirstOrDefault(e => e.Name == relationship.Source)?
                            .Type
                            ?? "N/A",
                        Target = relationship.Target,
                        TargetType = knowledgePart
                            .EntitiesAndRelationships
                            .Entities
                            .FirstOrDefault(e => e.Name == relationship.Target)?
                            .Type
                            ?? "N/A",
                        Strengths = [relationship.Strength],
                        ShortDescriptions = [relationship.ShortDescription],
                        Descriptions = [relationship.Description],
                        ChunkIds = [knowledgePart.IndexEntryId!]
                    });
                }
            }
        }

        private async Task LoadEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            if (_entityRelationships.Entities.Count > 0)
                return; // Already loaded

            var result = await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeEntity>(
                dataPipelineDefinition,
                dataPipelineRun,
                KNOWLEDGE_ENTITIES_FILE_PATH);

            _entityRelationships.Entities.AddRange(result);
        }

        private async Task LoadRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            if (_entityRelationships.Relationships.Count > 0)
                return; // Already loaded

            var result = await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeRelationship>(
                dataPipelineDefinition,
                dataPipelineRun,
                KNOWLEDGE_RELATIONSHIPS_FILE_PATH);

            _entityRelationships.Relationships.AddRange(result);
        }

        private async Task SaveEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun) =>
            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeEntity>(
                dataPipelineDefinition,
                dataPipelineRun,
                _entityRelationships.Entities,
                KNOWLEDGE_ENTITIES_FILE_PATH);

        private async Task SaveRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun) =>
            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeRelationship>(
                dataPipelineDefinition,
                dataPipelineRun,
                _entityRelationships.Relationships,
                KNOWLEDGE_RELATIONSHIPS_FILE_PATH);

        private async Task CreateGatewayServiceClient()
        {
            if (_gatewayServiceClient is not null)
                return; // Already created

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            _gatewayServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());
        }

        private async Task LoadSummarizationPrompt(
            string summarizationPromptObjectId)
        {
            if (_summarizationPrompt is not null)
                return; // Already loaded

            var entitySummarizationPrompt = await _promptResourceProvider.GetResourceAsync<PromptBase>(
                    summarizationPromptObjectId,
                    ServiceContext.ServiceIdentity!);

            _summarizationPrompt = (entitySummarizationPrompt as MultipartPrompt)!.Prefix!;
        }

        private async Task<PluginResult> SummarizeEntities(
            DataPipelineRun dataPipelineRun,
            string summarizationModel,
            float summarizationModelTemperature,
            int summarizationMaxOutputTokenCount)
        {
            var positionsMapping = new Dictionary<int, int>();
            var currentRequestPosition = 0;
            var startTime = DateTimeOffset.UtcNow;

            var textCompletionRequest = new TextCompletionRequest
            {
                CompletionModelName = summarizationModel,
                CompletionModelParameters = new Dictionary<string, object>
                {
                    { TextOperationModelParameterNames.Temperature, summarizationModelTemperature },
                    { TextOperationModelParameterNames.MaxOutputTokenCount, summarizationMaxOutputTokenCount }
                },
                TextChunks = [.. _entityRelationships.Entities
                    .Where(e => e.Descriptions.Count > 1)
                    .Select(e =>
                    {
                        var content = _summarizationPrompt
                            .Replace(ENTITY_NAMES_PLACEHOLDER, e.Name)
                            .Replace(DESCRIPTIONS_LIST_PLACEHOLDER, string.Join(Environment.NewLine, e.Descriptions));
                        currentRequestPosition++;
                        positionsMapping[e.Position] = currentRequestPosition;

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

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} entity summarization: {ProcessedEntityCount} of {TotalEntityCount} entities, {TotalSeconds} seconds.",
                        dataPipelineRun.Id,
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

            foreach (var entity in _entityRelationships.Entities)
            {
                if (entity.Descriptions.Count == 1)
                    entity.SummaryDescription = entity.Descriptions[0];
                else
                {
                    if (completionsDictionary.TryGetValue(positionsMapping[entity.Position], out var completion))
                        entity.SummaryDescription = completion;
                    else
                        _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return a summary for the entity at position {EntityPosition}.",
                            dataPipelineRun.Id,
                            entity.Position);
                }
            }

            return new PluginResult(true, false);
        }

        private async Task<PluginResult> SummarizeRelationships(
            DataPipelineRun dataPipelineRun,
            string summarizationModel,
            float summarizationModelTemperature,
            int summarizationMaxOutputTokenCount)
        {
            var positionsMapping = new Dictionary<int, int>();
            var currentRequestPosition = 0;
            var startTime = DateTimeOffset.UtcNow;

            var textCompletionRequest = new TextCompletionRequest
            {
                CompletionModelName = summarizationModel,
                CompletionModelParameters = new Dictionary<string, object>
                {
                    { TextOperationModelParameterNames.Temperature, summarizationModelTemperature },
                    { TextOperationModelParameterNames.MaxOutputTokenCount, summarizationMaxOutputTokenCount }
                },
                TextChunks = [.. _entityRelationships.Relationships
                    .Where(r => r.Descriptions.Count > 1)
                    .Select(r =>
                    {
                        var content = _summarizationPrompt
                            .Replace(ENTITY_NAMES_PLACEHOLDER, $"{r.Source},{r.Target}")
                            .Replace(DESCRIPTIONS_LIST_PLACEHOLDER, string.Join(Environment.NewLine, r.Descriptions));
                        currentRequestPosition++;
                        positionsMapping[r.Position] = currentRequestPosition;

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

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} relationship summarization: {ProcessedEntityCount} of {TotalEntityCount} entities, {TotalSeconds} seconds.",
                        dataPipelineRun.Id,
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

            foreach (var relationship in _entityRelationships.Relationships)
            {
                if (relationship.Descriptions.Count == 1)
                    relationship.SummaryDescription = relationship.Descriptions[0];
                else
                {
                    if (completionsDictionary.TryGetValue(positionsMapping[relationship.Position], out var completion))
                        relationship.SummaryDescription = completion;
                    else
                        _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return a summary for the relationship at position {EntityPosition}.",
                            dataPipelineRun.Id,
                            relationship.Position);
                }
            }

            return new PluginResult(true, false);
        }

        private async Task<PluginResult> EmbedEntities(
            DataPipelineRun dataPipelineRun,
            string embeddingModel,
            int embeddingDimensions)
        {
            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = embeddingModel,
                EmbeddingModelDimensions = embeddingDimensions,
                Prioritized = false,
                TextChunks = [.. _entityRelationships.Entities
                    .Select(e => new TextChunk
                    {
                        Position = e.Position,
                        Content = e.SummaryDescription,
                        TokensCount = (int)_tokenizer.CountTokens(e.SummaryDescription!)
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

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} knowledge graph entities embedding: {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                        dataPipelineRun.Id,
                        embeddingResult.ProcessedTextChunksCount,
                        textEmbeddingRequest.TextChunks.Count);
                }

                if (embeddingResult.Failed)
                    return new PluginResult(false, false,
                        $"The {Name} plugin failed to embed knowledge graph entities for data pipeline run {dataPipelineRun.Id} due to a failure in the Gateway API.");

                embeddingsDictionary = embeddingResult.TextChunks.ToDictionary(
                    chunk => chunk.Position,
                    chunk => chunk.Embedding);
            }

            foreach (var entity in _entityRelationships.Entities)
            {
                if (embeddingsDictionary.TryGetValue(entity.Position, out var embedding))
                    entity.SummaryDescriptionEmbedding = embedding!.Value.Vector.ToArray();
                else
                    _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return an embedding for the entity at position {EntityPosition}.",
                        dataPipelineRun.Id,
                        entity.Position);
            }

            return new PluginResult(true, false);
        }

        private async Task<PluginResult> EmbedRelationships(
            DataPipelineRun dataPipelineRun,
            string embeddingModel,
            int embeddingDimensions)
        {
            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = embeddingModel,
                EmbeddingModelDimensions = embeddingDimensions,
                Prioritized = false,
                TextChunks = [.. _entityRelationships.Relationships
                    .Select(r => new TextChunk
                    {
                        Position = r.Position,
                        Content = r.SummaryDescription,
                        TokensCount = (int)_tokenizer.CountTokens(r.SummaryDescription!)
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

                    _logger.LogInformation("Data pipeline run {DataPipelineRunId} knowledge graph relationships embedding: {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                        dataPipelineRun.Id,
                        embeddingResult.ProcessedTextChunksCount,
                        textEmbeddingRequest.TextChunks.Count);
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
                if (embeddingsDictionary.TryGetValue(relationship.Position, out var embedding))
                    relationship.SummaryDescriptionEmbedding = embedding!.Value.Vector.ToArray();
                else
                    _logger.LogWarning("Data pipeline run {DataPipelineRunId}: The Gateway API did not return an embedding for the relationship at position {EntityPosition}.",
                        dataPipelineRun.Id,
                        relationship.Position);
            }

            return new PluginResult(true, false);
        }
    }
}
