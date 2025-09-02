using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements basic capabilities for Knowledge Graph Data Pipeline Stage Plugins.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphDataPipelineStagePluginBase(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";
        protected const string KNOWLEDGE_GRAPH_ROOT_PATH = "knowledge-graph";
        protected const string KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME = "knowledge-graph-entities.parquet";
        protected const string KNOWLEDGE_GRAPH_ENTITIES_EMBEDDINGS_FILE_NAME = "knowledge-graph-entities-embeddings.parquet";
        protected const string KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME = "knowledge-graph-relationships.parquet";
        protected const string KNOWLEDGE_GRAPH_RELATIONSHIPS_EMBEDDINGS_FILE_NAME = "knowledge-graph-relationships-embeddings.parquet";
        protected const string KNOWLEDGE_GRAPH_BUCKETS_REGISTRY_FILE_NAME = "knowledge-graph-buckets.json";
        protected const string KNOWLEDGE_GRAPH_ENTITY = "Entity";
        protected const string KNOWLEDGE_GRAPH_RELATIONSHIP = "Relationship";

        protected const int GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS = 5;

        protected readonly IResourceProviderService? _promptResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Prompt);
        protected readonly IResourceProviderService? _contextResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Context);
        protected readonly IResourceProviderService? _vectorResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Vector);
        protected readonly ITokenizerService _tokenizer =
            serviceProvider.GetRequiredKeyedService<ITokenizerService>("MicrosoftML")
            ?? throw new PluginException("The MicrosoftML tokenizer service is not available in the dependency injection container.");

        protected GatewayServiceClient _gatewayServiceClient = null!;

        protected readonly KnowledgeEntityRelationshipCollection<KnowledgeEntity, KnowledgeRelationship> _entityRelationships = new();
        protected Dictionary<string, KnowledgeEntityEmbedding> _entitiesEmbeddings = [];
        protected Dictionary<string, KnowledgeRelationshipEmbedding> _relationshipsEmbeddings = [];

        protected async Task<KnowledgeBucketsRegistry> LoadKnowledgeBucketsRegistry(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun)
        {
            var knowledgeBucketsRegistryPath = string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    KNOWLEDGE_GRAPH_BUCKETS_REGISTRY_FILE_NAME
                ]);
            var knowledgeBucketsRegistryLoadResult = await _dataPipelineStateService.TryLoadDataPipelineRunArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                knowledgeBucketsRegistryPath);

            var knowledgeBucketsRegistry =
                knowledgeBucketsRegistryLoadResult.Success
                && knowledgeBucketsRegistryLoadResult.Artifacts.Count == 1
                ? JsonSerializer.Deserialize<KnowledgeBucketsRegistry>(
                    knowledgeBucketsRegistryLoadResult.Artifacts![0].Content)!
                : new KnowledgeBucketsRegistry();

            return knowledgeBucketsRegistry;
        }

        protected async Task SaveKnowledgeBucketsRegistry(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            KnowledgeBucketsRegistry knowledgeBucketsRegistry)
        {
            var knowledgeBucketsRegistryPath = string.Join('/', [
                KNOWLEDGE_GRAPH_ROOT_PATH,
                KNOWLEDGE_GRAPH_BUCKETS_REGISTRY_FILE_NAME
            ]);

            await _dataPipelineStateService.SaveDataPipelineRunArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                [
                    new DataPipelineStateArtifact
                    {
                        FileName = knowledgeBucketsRegistryPath,
                        Content = BinaryData.FromString(JsonSerializer.Serialize(knowledgeBucketsRegistry)),
                        ContentType = "application/json"
                    }
                ]);
        }

        protected async Task CreateGatewayServiceClient(
            string instanceId)
        {
            if (_gatewayServiceClient is not null)
                return; // Already created

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            _gatewayServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.GatewayAPI,
                    ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());
        }

        protected async Task LoadEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (_entityRelationships.Entities.Count > 0)
                return; // Already loaded

            var knowledgeEntitiesFilePath = string.Join('/', [
                KNOWLEDGE_GRAPH_ROOT_PATH,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
            ]);

            var result = await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeEntity>(
                dataPipelineDefinition,
                dataPipelineRun,
                knowledgeEntitiesFilePath);

            _entityRelationships.Entities.AddRange(result);
        }

        protected async Task LoadRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (_entityRelationships.Relationships.Count > 0)
                return; // Already loaded

            var knowledgeRelationshipsFilePath = string.Join('/', [
               KNOWLEDGE_GRAPH_ROOT_PATH,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
           ]);

            var result = await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeRelationship>(
                dataPipelineDefinition,
                dataPipelineRun,
                knowledgeRelationshipsFilePath);

            _entityRelationships.Relationships.AddRange(result);
        }

        protected async Task SaveEntities(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem) =>
            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeEntity>(
                dataPipelineDefinition,
                dataPipelineRun,
                _entityRelationships.Entities,
                string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                ]));

        protected async Task SaveRelationships(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem) =>
            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeRelationship>(
                dataPipelineDefinition,
                dataPipelineRun,
                _entityRelationships.Relationships,
                string.Join('/', [
                   KNOWLEDGE_GRAPH_ROOT_PATH,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                ]));

        protected async Task LoadEntitiesEmbeddings(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            var knowledgeEntitiesEmbeddingsFilePath = string.Join('/', [
                KNOWLEDGE_GRAPH_ROOT_PATH,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                KNOWLEDGE_GRAPH_ENTITIES_EMBEDDINGS_FILE_NAME
            ]);

            var result = await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeEntityEmbedding>(
                dataPipelineDefinition,
                dataPipelineRun,
                knowledgeEntitiesEmbeddingsFilePath);

            _entitiesEmbeddings.AddRange(
                result.ToDictionary(x => x.UniqueId));
        }

        protected async Task LoadRelationshipsEmbeddings(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            var knowledgeRelationshipsEmbeddingsFilePath = string.Join('/', [
                KNOWLEDGE_GRAPH_ROOT_PATH,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                KNOWLEDGE_GRAPH_RELATIONSHIPS_EMBEDDINGS_FILE_NAME
            ]);

            var result = await _dataPipelineStateService.LoadDataPipelineRunParts<KnowledgeRelationshipEmbedding>(
                dataPipelineDefinition,
                dataPipelineRun,
                knowledgeRelationshipsEmbeddingsFilePath);

            _relationshipsEmbeddings.AddRange(
                result.ToDictionary(x => x.UniqueId));
        }

        protected async Task SaveEntitiesEmbeddings(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem) =>
            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeEntityEmbedding>(
                dataPipelineDefinition,
                dataPipelineRun,
                _entitiesEmbeddings.Values,
                string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    KNOWLEDGE_GRAPH_ENTITIES_EMBEDDINGS_FILE_NAME
                ]));

        protected async Task SaveRelationshipsEmbeddings(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem) =>
            await _dataPipelineStateService.SaveDataPipelineRunParts<KnowledgeRelationshipEmbedding>(
                dataPipelineDefinition,
                dataPipelineRun,
                _relationshipsEmbeddings.Values,
                string.Join('/', [
                    KNOWLEDGE_GRAPH_ROOT_PATH,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    KNOWLEDGE_GRAPH_RELATIONSHIPS_EMBEDDINGS_FILE_NAME
                ]));
    }
}
