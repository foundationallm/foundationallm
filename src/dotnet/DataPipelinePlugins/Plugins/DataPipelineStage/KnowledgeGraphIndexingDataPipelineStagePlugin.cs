using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Services.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Graph Indexing Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeGraphIndexingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : KnowledgeGraphDataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEGRAPH_INDEXING_DATAPIPELINESTAGE;

        private const string KEY_FIELD_NAME = "Id";
        private const string UNIQUE_ID_METADATA_PROPERTY_NAME = "UniqueId";
        private const string ITEM_TYPE_METADATA_PROPERTY_NAME = "ItemType";

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

            if (vectorDatabase.DatabaseType != VectorDatabaseType.AzureAISearch)
                throw new PluginException(
                    $"The vector database type {vectorDatabase.DatabaseType} is not supported by the {Name} plugin.");

            var vectorStoreId = knowledgeUnit.VectorStoreId
                ?? throw new PluginException(
                    $"The knowledge unit {knowledgeUnit.Name} does not have a valid vector store id.");

            #endregion

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var endpointResourcePath =
                ResourcePath.GetResourcePath(vectorDatabase.APIEndpointConfigurationObjectId);

            var searchIndexClient = await clientFactoryService.CreateClient<SearchIndexClient>(
                dataPipelineRun.InstanceId,
                endpointResourcePath.ResourceId!,
                ServiceContext.ServiceIdentity!,
                AzureAISearchService.CreateSearchIndexClient,
                null);

            var azureAISearchService = new AzureAISearchService(
                searchIndexClient,
                _serviceProvider.GetRequiredService<ILogger<AzureAISearchService>>()) as IAzureAISearchService;

            var metadataProperties = vectorDatabase.MetadataProperties
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var metadataField = new ComplexField(vectorDatabase.MetadataPropertyName);
            foreach (var metadataProperty in metadataProperties)
                metadataField.Fields.Add(GetFieldTemplate(metadataProperty));

            IEnumerable<SearchField> indexFields =
                [
                    new SimpleField(KEY_FIELD_NAME, SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                    new SimpleField(vectorDatabase.VectorStoreIdPropertyName, SearchFieldDataType.String) { IsFilterable = true },
                    new SearchableField(vectorDatabase.ContentPropertyName),
                    new VectorSearchField(vectorDatabase.EmbeddingPropertyName, vectorDatabase.EmbeddingDimensions, "vector-profile"),
                    metadataField
                ];

            await azureAISearchService.CreateIndexIfNotExists(
                vectorDatabase.DatabaseName,
                indexFields,
                new VectorSearch()
                {
                    Profiles =
                    {
                        new VectorSearchProfile("vector-profile", "algorithm-configuration")
                    },
                    Algorithms =
                    {
                        new HnswAlgorithmConfiguration("algorithm-configuration")
                    }
                });

            List<object[]> fieldValues;

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

                var entitiesToIndex = _entityRelationships.Entities
                    .Where(e =>
                        e.LastChangedBy.Equals(dataPipelineRun.RunId) // Only index entities that have changed in the current data pipeline run.
                        && _entitiesEmbeddings.ContainsKey(e.UniqueId))
                    .ToList();

                fieldValues = [.. entitiesToIndex.Select(
                    e => new object[]
                    {
                        e.IndexEntryId!, // Id
                        vectorStoreId, // VectorStoreId
                        e.SummaryDescription!, // Content
                        _entitiesEmbeddings[e.UniqueId], // Embedding
                        new Dictionary<string, object?> // Metadata
                        {
                            { UNIQUE_ID_METADATA_PROPERTY_NAME, e.UniqueId },
                            { ITEM_TYPE_METADATA_PROPERTY_NAME, KNOWLEDGE_GRAPH_ENTITY }
                        }
                    })];
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

                var relationshipsToIndex = _entityRelationships.Relationships
                    .Where(r =>
                        r.LastChangedBy.Equals(dataPipelineRun.RunId) // Only index relationships that have changed in the current data pipeline run.
                        && _relationshipsEmbeddings.ContainsKey(r.UniqueId))
                    .ToList();

                fieldValues = [.. relationshipsToIndex.Select(
                    r => new object[]
                    {
                        r.IndexEntryId!, // Id
                        vectorStoreId, // VectorStoreId
                        r.SummaryDescription!, // Content
                        _relationshipsEmbeddings[r.UniqueId], // Embedding
                        new Dictionary<string, object?> // Metadata
                        {
                            { UNIQUE_ID_METADATA_PROPERTY_NAME, r.UniqueId },
                            { ITEM_TYPE_METADATA_PROPERTY_NAME, KNOWLEDGE_GRAPH_RELATIONSHIP }
                        }
                    })];
            }
            else
                return new PluginResult(false, true,
                    $"The {Name} plugin received an invalid content item canonical identifier {dataPipelineRunWorkItem.ContentItemCanonicalId}.");


            await azureAISearchService.UploadDocuments(
                vectorDatabase.DatabaseName,
                [.. indexFields.Select(f => f.Name)],
                fieldValues);

            return
                new PluginResult(true, false);
        }

        private SearchFieldTemplate GetFieldTemplate(string fieldDefinition)
        {
            var tokens = fieldDefinition.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Length == 1)
                return new SearchableField(tokens[0]) { IsFilterable = true };

            return tokens[1] switch
            {
                "Edm.String" => new SearchableField(tokens[0]) { IsFilterable = true },
                "Edm.Int32"
                or "Edm.Int64"
                or "Edm.Single"
                or "Edm.Double"
                or "Edm.Boolean"
                or "Edm.DateTimeOffset" => new SimpleField(tokens[0], new SearchFieldDataType(tokens[1])) { IsFilterable = true },
                _ => throw new PluginException($"The {Name} plugin does not support field type {tokens[1]}.")
            };
        }
    }
}
