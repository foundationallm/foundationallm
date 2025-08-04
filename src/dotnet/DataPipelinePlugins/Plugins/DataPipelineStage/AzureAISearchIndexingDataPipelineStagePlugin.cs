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
    /// Implements the Azure AI Indexing Data Pipeline Stage Plugin.
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

        protected readonly IResourceProviderService? _vectorResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Vector);

        private const string CONTENT_PARTS_FILE_NAME = "content-parts.parquet";
        private const string KEY_FIELD_NAME = "Id";
        private const string FILE_NAME_METADATA_PROPERTY_NAME = "FileName";

        // <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (_vectorResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vector} was not loaded.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID,
                out var vectorDatabaseObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID,
                out var vectorStoreId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS,
                out var embeddingDimensions))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_METADATAPROPERTIES,
                out var metadataPropertiesObject))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_METADATAPROPERTIES} parameter.");

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                vectorDatabaseObjectId.ToString()!,
                ServiceContext.ServiceIdentity!);

            var endpointResourcePath =
                ResourcePath.GetResourcePath(vectorDatabase.APIEndpointConfigurationObjectId);

            var searchIndexClient = await clientFactoryService.CreateClient<SearchIndexClient>(
                endpointResourcePath.ResourceId!,
                ServiceContext.ServiceIdentity!,
                AzureAISearchService.CreateSearchIndexClient,
                null);

            var azureAISearchService = new AzureAISearchService(
                searchIndexClient,
                _serviceProvider.GetRequiredService<ILogger<AzureAISearchService>>()) as IAzureAISearchService;

            var metadataProperties = metadataPropertiesObject.ToString()!
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var metadataField = new ComplexField(vectorDatabase.MetadataPropertyName);
            foreach (var metadataProperty in metadataProperties)
                metadataField.Fields.Add(GetFieldTemplate(metadataProperty));

            IEnumerable<SearchField> indexFields =
                [
                    new SimpleField(KEY_FIELD_NAME, SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                    new SimpleField(vectorDatabase.VectorStoreIdPropertyName, SearchFieldDataType.String) { IsFilterable = true },
                    new SearchableField(vectorDatabase.ContentPropertyName),
                    new VectorSearchField(vectorDatabase.EmbeddingPropertyName, (int)embeddingDimensions, "vector-profile"),
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

            var contentItemParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                CONTENT_PARTS_FILE_NAME);

            if (!contentItemParts.Any())
                return new PluginResult(true, false, WarningMessage: "The content item has no content.");

            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                contentItemParts.First().Metadata!);
            var fileNameObject = metadata!.GetValueOrDefault(FILE_NAME_METADATA_PROPERTY_NAME, string.Empty);
            var fileName = fileNameObject!.ToString();
            if (string.IsNullOrWhiteSpace(fileName))
                throw new PluginException(
                    $"The {FILE_NAME_METADATA_PROPERTY_NAME} metadata property is not set in the content item parts.");

            var deletedKeysCount = await azureAISearchService.DeleteDocuments(
                vectorDatabase.DatabaseName,
                KEY_FIELD_NAME,
                $"{vectorDatabase.VectorStoreIdPropertyName} eq '{vectorStoreId}' and {vectorDatabase.MetadataPropertyName}/{FILE_NAME_METADATA_PROPERTY_NAME} eq '{fileName.Replace("'", "''")}'");

            _logger.LogInformation("Data pipeline run {DataPipelineRunId}, content item {ContentItemCanonicalId}: removed {DeletedKeysCount} existing entries from the index.",
                dataPipelineRun.Id,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                deletedKeysCount);

            await azureAISearchService.UploadDocuments(
                vectorDatabase.DatabaseName,
                [.. indexFields.Select(f => f.Name)],
                [.. contentItemParts.Select(cip => new object[]
                {
                    cip.IndexEntryId!,
                    vectorStoreId!,
                    cip.Content!,
                    cip.Embedding!,
                    JsonSerializer.Deserialize<Dictionary<string, object>>(cip.Metadata!)!
                })]);

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
