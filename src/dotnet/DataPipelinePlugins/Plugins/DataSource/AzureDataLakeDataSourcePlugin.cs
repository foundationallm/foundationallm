using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.DataSource;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource
{
    /// <summary>
    /// Implements the Azure Data Lake data source plugin.
    /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    /// </summary>
    public class AzureDataLakeDataSourcePlugin(
        string dataSourceObjectId,
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IDataSourcePlugin
    {
        private readonly string _dataSourceObjectId = dataSourceObjectId;

        protected override string Name => PluginNames.AZUREDATALAKE_DATASOURCE;

        protected readonly IResourceProviderService? _dataSourceResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_DataSource)
            ?? throw new PluginException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

        protected readonly IConfiguration _configuration = serviceProvider
            .GetRequiredService<IConfiguration>()
            ?? throw new PluginException("The configuration service is not available in the dependency injection container.");

        /// <inheritdoc/>
        public async Task<List<DataPipelineContentItem>> GetContentItems()
        {
            var folders = GetFoldersFromParameters();
            var storageService = await GetConfiguredStorageService();

            List<DataPipelineContentItem> contentItems = [];

            foreach (var folder in folders)
            {
                // Parse folder path: format is [container_name]/[folder_path]
                var parts = folder.Split('/', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 0)
                    continue;

                var containerName = parts[0];
                var directoryPath = parts.Length > 1 ? parts[1] : null;

                try
                {
                    var filePaths = await storageService.GetFilePathsAsync(
                        containerName,
                        directoryPath,
                        recursive: true);

                    foreach (var filePath in filePaths)
                    {
                        var fileName = Path.GetFileName(filePath);
                        var canonicalId = $"{containerName}/{filePath}";

                        contentItems.Add(new DataPipelineContentItem
                        {
                            Id = $"content-item-{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid().ToBase64String()}",
                            DataSourceObjectId = _dataSourceObjectId,
                            ContentIdentifier = new ContentIdentifier
                            {
                                MultipartId = [containerName, filePath],
                                CanonicalId = canonicalId
                            },
                            ContentAction = ContentItemActions.AddOrUpdate
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "The {PluginName} plugin encountered an error while retrieving files from folder {Folder}.",
                        Name,
                        folder);
                    throw new PluginException(
                        $"The {Name} plugin encountered an error while retrieving files from folder {folder}.", ex);
                }
            }

            return contentItems;
        }

        /// <inheritdoc/>
        public async Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            ContentIdentifier contentItemIdentifier)
        {
            var contentItemCanonicalId = contentItemIdentifier.CanonicalId;
            var storageService = await GetConfiguredStorageService();

            // Parse canonical ID: format is [container_name]/[file_path]
            var parts = contentItemCanonicalId.Split('/', 2);
            if (parts.Length < 2)
                throw new PluginException($"Invalid content item canonical ID: {contentItemCanonicalId}");

            var containerName = parts[0];
            var filePath = parts[1];

            try
            {
                var binaryData = await storageService.ReadFileAsync(containerName, filePath);
                var fileName = Path.GetFileName(filePath);
                var contentTypeResult = FileUtils.GetFileContentType(fileName, binaryData);

                if (!contentTypeResult.IsSupported
                    || !contentTypeResult.MatchesExtension)
                {
                    _logger.LogWarning(
                        "The {PluginName} plugin cannot process the content item with identifier {ContentItemIdentifier} because the file type is not supported or does not match the file extension.",
                        Name,
                        contentItemCanonicalId);
                    return new PluginResult<ContentItemRawContent>(
                        null,
                        false,
                        false,
                        ErrorMessage: "The file type is not supported or does not match the file extension.");
                }

                return new PluginResult<ContentItemRawContent>(
                    new ContentItemRawContent
                    {
                        Name = fileName,
                        ContentType = contentTypeResult.ContentType,
                        RawContent = binaryData,
                        Metadata = new Dictionary<string, object>
                        {
                            { "FileId", contentItemCanonicalId },
                            { "FileName", fileName },
                            { "ContainerName", containerName },
                            { "FilePath", filePath }
                        }
                    },
                    true,
                    false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "The {PluginName} plugin encountered an error while retrieving content for item {ContentItemIdentifier}.",
                    Name,
                    contentItemCanonicalId);
                return new PluginResult<ContentItemRawContent>(
                    null,
                    false,
                    false,
                    ErrorMessage: $"Failed to retrieve content: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task HandleUnsafeContentItem(string canonicalContentItemIdentifier)
        {
            // No action needed for Azure Data Lake data source.
            // Access is most certainly read-only, so we will not be able to affect the source content item.
            // Also, deleting or modifying the source content item might have unintended consequences.
            // We don't want to interfere with content that is not managed by FoundationaLLM.

            _logger.LogWarning(
                "The {PluginName} plugin received a notification to handle an unsafe content item with identifier {ContentItemIdentifier}. No action will be taken.",
                Name,
                canonicalContentItemIdentifier);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves and parses the folders parameter from plugin parameters.
        /// </summary>
        /// <returns>An array of folder paths.</returns>
        private string[] GetFoldersFromParameters()
        {
            var foldersList = _pluginParameters[PluginParameterNames.AZUREDATALAKE_DATASOURCE_FOLDERS]?.ToString()
                ?? throw new PluginException($"The {PluginParameterNames.AZUREDATALAKE_DATASOURCE_FOLDERS} parameter is required by the {Name} plugin.");

            return foldersList.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Retrieves the data source configuration and creates a configured DataLakeStorageService.
        /// </summary>
        /// <returns>A configured DataLakeStorageService instance.</returns>
        private async Task<DataLakeStorageService> GetConfiguredStorageService()
        {
            var dataSourceBase = await _dataSourceResourceProvider!.GetResourceAsync<DataSourceBase>(
                _dataSourceObjectId,
                ServiceContext.ServiceIdentity!)
                ?? throw new PluginException($"The Azure Data Lake data source with object ID '{_dataSourceObjectId}' cannot be found.");

            var dataSource = dataSourceBase as AzureDataLakeDataSource
                ?? throw new PluginException($"The data source [{_dataSourceObjectId}] is not a valid Azure Data Lake data source.");

            var dataLakeSettings = dataSource.ConfigurationReferences!
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => _configuration[kvp.Value]);

            return GetStorageService(dataLakeSettings!);
        }

        /// <summary>
        /// Creates a DataLakeStorageService instance based on the provided configuration.
        /// </summary>
        /// <param name="settings">The configuration settings dictionary.</param>
        /// <returns>A configured DataLakeStorageService instance.</returns>
        private DataLakeStorageService GetStorageService(Dictionary<string, string> settings)
        {
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<DataLakeStorageService>();

            // Determine authentication type based on available configuration keys
            BlobStorageServiceSettings storageSettings;

            if (settings.ContainsKey("AccountName") && !settings.ContainsKey("AccountKey") && !settings.ContainsKey("ConnectionString"))
            {
                // Azure Identity authentication
                storageSettings = new BlobStorageServiceSettings
                {
                    AuthenticationType = AuthenticationTypes.AzureIdentity,
                    AccountName = settings["AccountName"]
                };
            }
            else if (settings.ContainsKey("AccountName") && settings.ContainsKey("AccountKey"))
            {
                // Account Key authentication
                storageSettings = new BlobStorageServiceSettings
                {
                    AuthenticationType = AuthenticationTypes.AccountKey,
                    AccountName = settings["AccountName"],
                    AccountKey = settings["AccountKey"]
                };
            }
            else if (settings.ContainsKey("ConnectionString"))
            {
                // Connection String authentication
                storageSettings = new BlobStorageServiceSettings
                {
                    AuthenticationType = AuthenticationTypes.ConnectionString,
                    ConnectionString = settings["ConnectionString"]
                };
            }
            else
            {
                throw new PluginException("Invalid Azure Data Lake configuration. Must provide either AccountName (for Azure Identity), AccountName and AccountKey, or ConnectionString.");
            }

            return new DataLakeStorageService(storageSettings, logger);
        }
    }
}
