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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Azure AI Indexing Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class AzureAIIndexingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE;

        protected readonly IResourceProviderService? _vectorResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Vector);

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
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREOBJECTID,
                out var vectorStoreObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS,
                out var embeddingDimensions))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS} parameter.");

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var vectorDatabase = await _vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                vectorDatabaseObjectId.ToString()!,
                ServiceContext.ServiceIdentity!);

            var vectorStoreResourcePath =
                ResourcePath.GetResourcePath(vectorStoreObjectId.ToString()!);
            var vectorStoreId = vectorStoreResourcePath.ResourceId;

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

            var metadataField = new ComplexField(vectorDatabase.MetadataPropertyName);
            metadataField.Fields.Add(
                new SearchableField("FileName") { IsFilterable = true });

            IEnumerable<SearchField> indexFields =
                [
                    new SimpleField("Id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
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

            var contentItemParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemContentParts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem);

            foreach (var contentItemPart in contentItemParts)
                contentItemPart.IndexEntryId = GetIndexEntryId(
                    vectorStoreId!,
                    contentItemPart.ContentItemCanonicalId!,
                    contentItemPart.Position);

            await azureAISearchService.UploadDocuments(
                vectorDatabase.DatabaseName,
                [.. indexFields.Select(f => f.Name)],
                [.. contentItemParts.Select(cip => new object[]
                {
                    cip.IndexEntryId!, vectorStoreId!, cip.Content!, cip.Embedding!, cip.Metadata!
                })]);

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemContentParts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemParts);

            return new PluginResult(true, false);
        }

        private string GetIndexEntryId(
            string vectorStoreId,
            string contentItemCanonicalId,
            int contentItemPartPosition)
        {
            var id = $"{vectorStoreId}-{contentItemCanonicalId}-{contentItemPartPosition:D6}";

            return
                Convert.ToBase64String(
                    MD5.HashData(Encoding.UTF8.GetBytes(id)))
                .Replace("+", "--")
                .Replace("/", "--");
        }
    }
}
