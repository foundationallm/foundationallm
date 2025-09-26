using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
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
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Azure AI Search Indexing Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class AzureAISearchIndexingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE;

        private readonly IResourceProviderService? _contextResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Context);
        private readonly IResourceProviderService? _vectorResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Vector);

        private const string KEY_FIELD_NAME = "Id";
        private const string FILE_ID_METADATA_PROPERTY_NAME = "FileId";

        // <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            // Before anything else, check if the content or metadata have been changed by the current data pipeline run.
            var contentChanged = await _dataPipelineStateService.DataPipelineRunWorkItemArtifactChanged(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.TextContent);
            var metadataChanged = await _dataPipelineStateService.DataPipelineRunWorkItemArtifactChanged(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.Metadata);
            var contentItemParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
               dataPipelineDefinition,
               dataPipelineRun,
               dataPipelineRunWorkItem,
               DataPipelineStateFileNames.ContentParts);

            if (!contentItemParts.Any())
                return new PluginResult(true, false, WarningMessage: "The content item has no content.");

            if (!contentChanged
                && !metadataChanged
                && !contentItemParts.Any(part =>
                    part.LastChangedBy.Equals(dataPipelineRun.Id)
                    || !part.Indexed))
            {
                // Conditions for skipping processing:
                // 1. The text content has not changed.
                // 2. The metadata has not changed.
                // 3. None of the content item parts have been updated by the current data pipeline run or are unindexed.
                // Note that a content item part can be unindexed even if it has not changed, for example if the content item
                // was removed from the index by another data pipeline run.

                _logger.LogInformation(
                    "The {PluginName} plugin for the {Stage} stage determined there were no changes to process the work item {WorkItemId}.",
                    Name,
                    dataPipelineRunWorkItem.Stage,
                    dataPipelineRunWorkItem.Id);

                return new PluginResult(true, false);
            }

            if (_contextResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Context} was not loaded.");

            if (_vectorResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vector} was not loaded.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                out var knowledgeUnitObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID,
                out var vectorStoreIdObj))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID} parameter.");

            var knowledgeUnit = await _contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                knowledgeUnitObjectId.ToString()!,
                ServiceContext.ServiceIdentity!);

            var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                knowledgeUnit.VectorDatabaseObjectId,
                ServiceContext.ServiceIdentity!);

            if (vectorDatabase.DatabaseType != VectorDatabaseType.AzureAISearch)
                throw new PluginException(
                    $"The vector database type {vectorDatabase.DatabaseType} is not supported by the {Name} plugin.");

            var vectorStoreId = string.IsNullOrWhiteSpace(knowledgeUnit.VectorStoreId)
                ? vectorStoreIdObj?.ToString()
                : knowledgeUnit.VectorStoreId;
            if (string.IsNullOrWhiteSpace(vectorStoreId))
                throw new PluginException(
                    $"The knowledge unit {knowledgeUnit.Name} does not specify a vector store id and the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID} is empty.");

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

            #region Create Azure AI Search Index if it does not exist

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

            #endregion

            var serializedMetadata = (await _dataPipelineStateService.LoadDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                DataPipelineStateFileNames.Metadata))
                .First().Content.ToString();
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                serializedMetadata);

            var fileIdObject = metadata!.GetValueOrDefault(FILE_ID_METADATA_PROPERTY_NAME, string.Empty);
            var fileId = fileIdObject!.ToString();
            if (string.IsNullOrWhiteSpace(fileId))
                throw new PluginException(
                    $"The {FILE_ID_METADATA_PROPERTY_NAME} metadata property is not set in the content item parts.");

            var deletedKeysCount = await azureAISearchService.DeleteDocuments(
                vectorDatabase.DatabaseName,
                KEY_FIELD_NAME,
                $"{vectorDatabase.VectorStoreIdPropertyName} eq '{vectorStoreId}' and {vectorDatabase.MetadataPropertyName}/{FILE_ID_METADATA_PROPERTY_NAME} eq '{fileId}'");

            _logger.LogInformation("Data pipeline run {DataPipelineRunId}, content item {ContentItemCanonicalId}: removed {DeletedKeysCount} existing entries from the index.",
                dataPipelineRun.Id,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                deletedKeysCount);

            var uploadResults = await azureAISearchService.UploadDocuments(
                vectorDatabase.DatabaseName,
                [.. indexFields.Select(f => f.Name)],
                [.. contentItemParts.Select(cip => new object[]
                {
                    cip.IndexEntryId!,
                    vectorStoreId!,
                    cip.Content!,
                    cip.Embedding!,
                    metadata!
                })]);

            #region Update content item parts flag that indicates whether they have been indexed or not

            // This is mostly preparing the ground for future enhancements, for example to only re-index certain content item parts.
            // For now, we are replacing the entire content item in the index every time.

            foreach (var contentItemPart in contentItemParts)
            {
                contentItemPart.Indexed = uploadResults.TryGetValue(contentItemPart.IndexEntryId!, out var indexed) && indexed;
                contentItemPart.LastChangedBy = dataPipelineRun.Id;
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemParts,
                DataPipelineStateFileNames.ContentParts);

            #endregion

            if (uploadResults.Values.Any(ur => !ur))
                return new PluginResult(
                    false,
                    false,
                    ErrorMessage: $"Failed to index {uploadResults.Values.Count(ur => !ur)} content item parts out of {uploadResults.Values.Count}.");

            return new PluginResult(true, false);
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
