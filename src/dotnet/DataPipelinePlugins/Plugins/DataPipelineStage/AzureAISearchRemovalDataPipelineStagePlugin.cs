using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
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
using Microsoft.Graph;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Azure AI Search Removal Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class AzureAISearchRemovalDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider)
    {
        protected override string Name => PluginNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE;

        private readonly IResourceProviderService? _contextResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Context);
        private readonly IResourceProviderService? _vectorResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Vector);

        private const string KEY_FIELD_NAME = "Id";
        private const string FILE_ID_METADATA_PROPERTY_NAME = "FileId";

        // <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            string dataPipelineStageName)
        {
            var workItems = contentItems
                .Where(ci => ci.ContentAction == ContentItemActions.Remove)
                .Select(ci => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRun.RunId,
                    Stage = dataPipelineStageName,
                    ContentItemCanonicalId = ci.ContentIdentifier.CanonicalId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        // <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (_contextResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Context} was not loaded.");

            if (_vectorResourceProvider is null)
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vector} was not loaded.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                out var knowledgeUnitObjectId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_VECTORSTOREID,
                out var vectorStoreIdObj))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_VECTORSTOREID} parameter.");

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
                    $"The knowledge unit {knowledgeUnit.Name} does not specify a vector store id and the {PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_VECTORSTOREID} is empty.");

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

            #region Reset the indexed flag of all content item parts

            var contentItemParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
               dataPipelineDefinition,
               dataPipelineRun,
               dataPipelineRunWorkItem,
               DataPipelineStateFileNames.ContentParts);

            foreach (var contentItemPart in contentItemParts)
            {
                contentItemPart.Indexed = false;
                contentItemPart.LastChangedBy = dataPipelineRun.Id;
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemParts,
                DataPipelineStateFileNames.ContentParts);

            #endregion

            return new PluginResult(true, false);
        }
    }
}
