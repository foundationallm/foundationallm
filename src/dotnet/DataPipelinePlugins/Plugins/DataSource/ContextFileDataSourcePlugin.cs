using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource
{
    /// <summary>
    /// Implements the FoundationaLLM Context File data source plugin.
    /// </summary>
    /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class ContextFileDataSourcePlugin(
        string dataSourceObjectId,
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IDataSourcePlugin
    {
        private readonly string _dataSourceObjectId = dataSourceObjectId;

        protected override string Name => PluginNames.CONTEXTFILE_DATASOURCE;

        public async Task<List<DataPipelineContentItem>> GetContentItems()
        {
            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID,
                out var contextFileObjectIdObject))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID} parameter.");
            var contextFileObjectId = contextFileObjectIdObject.ToString()!;

            var contentAction = ContentItemActions.AddOrUpdate;
            if (_pluginParameters.TryGetValue(
                PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTENTACTION,
                out var contentActionObject))
                contentAction = contentActionObject.ToString()!;

            var resourcePath = ResourcePath.GetResourcePath( contextFileObjectId );
            var canonicalId = resourcePath.MainResourceId!;

            return await Task.FromResult<List<DataPipelineContentItem>>(
                [
                    new DataPipelineContentItem
                    {
                        Id = $"content-item-{canonicalId}-{Guid.NewGuid().ToBase64String()}",
                        DataSourceObjectId = _dataSourceObjectId,
                        ContentIdentifier = new ContentIdentifier
                        {
                            MultipartId = [contextFileObjectId],
                            CanonicalId = canonicalId
                        },
                        ContentAction = contentAction
                    }
                ]);
        }

        /// <inheritdoc/>
        public async Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            ContentIdentifier contentItemIdentifier)
        {
            var contentItemCanonicalId = contentItemIdentifier.CanonicalId;
            var contextFileObjectId = _pluginParameters[PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID]?.ToString()
                ?? throw new PluginException($"The {PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID} parameter is required by the {Name} plugin.");
            var resourcePath = ResourcePath.GetResourcePath(contextFileObjectId);

            var contextServiceClient = GetContextServiceClient();

            var response = await contextServiceClient.GetFileContent(
                resourcePath.InstanceId!,
                contentItemCanonicalId);

            return response.TryGetValue(out var fileContent)
                ? new PluginResult<ContentItemRawContent>(
                    new ContentItemRawContent
                    {
                        Name = fileContent.FileName,
                        ContentType = fileContent.ContentType,
                        RawContent = BinaryData.FromStream(fileContent.FileContent!),
                        Metadata = new Dictionary<string, object>
                        {
                            { "FileId", contentItemCanonicalId },
                            { "FileName", fileContent.FileName }
                        }
                    },
                    true,
                    false)
                : new PluginResult<ContentItemRawContent>(
                    null,
                    false,
                    false);
        }

        /// <inheritdoc/>
        public async Task HandleUnsafeContentItem(string canonicalContentItemIdentifier)
        {
            var contextFileObjectId = _pluginParameters[PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID]?.ToString()
                ?? throw new PluginException($"The {PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID} parameter is required by the {Name} plugin.");
            var resourcePath = ResourcePath.GetResourcePath(contextFileObjectId);

            // We need to delete the offending file from the context file store to avoid future usage.

            var contextServiceClient = GetContextServiceClient();

            var response = await contextServiceClient.DeleteFileRecord(
                resourcePath.InstanceId!,
                canonicalContentItemIdentifier);
        }

        private ContextServiceClient GetContextServiceClient() =>
            new(
                new OrchestrationContext { CurrentUserIdentity = ServiceContext.ServiceIdentity },
                _serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                _serviceProvider.GetRequiredService<ILogger<ContextServiceClient>>());
    }
}
