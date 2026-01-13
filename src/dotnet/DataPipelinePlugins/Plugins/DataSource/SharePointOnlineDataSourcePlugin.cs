using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.DataSource;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using System.Security.Cryptography.X509Certificates;

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
    public class SharePointOnlineDataSourcePlugin(
        string dataSourceObjectId,
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IDataSourcePlugin
    {
        private readonly string _dataSourceObjectId = dataSourceObjectId;

        protected override string Name => PluginNames.SHAREPOINTONLINE_DATASOURCE;

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
            var documentLibraryPathsList = (_pluginParameters[PluginParameterNames.SHAREPOINTONLINE_DATASOURCE_DOCUMENTLIBRARIES] as List<object>)?
                    .Select(dl => dl.ToString()!)
                    .ToList()
                ?? throw new PluginException($"The {PluginParameterNames.SHAREPOINTONLINE_DATASOURCE_DOCUMENTLIBRARIES} parameter is required by the {Name} plugin.");

            var dataSourceBase = await _dataSourceResourceProvider!.GetResourceAsync<DataSourceBase>(
                _dataSourceObjectId,
                ServiceContext.ServiceIdentity!)
                ?? throw new PluginException($"The SharePoint Online data source with object ID '{_dataSourceObjectId}' cannot be found.");

            var dataSource = dataSourceBase as SharePointOnlineSiteDataSource
                ?? throw new PluginException($"The data source [{_dataSourceObjectId}] is not a valid SharePoint Online Site data source.");

            var sharePointSettings = dataSource.ConfigurationReferences!
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => _configuration[kvp.Value]);

            var pnpServiceProvider = await GetServiceProvider(
                dataSource.SiteUrl!,
                sharePointSettings!);

            List<DataPipelineContentItem> contentItems = [];
            using (var scope = pnpServiceProvider.CreateScope())
            {
                var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

                using var context = await pnpContextFactory.CreateAsync("Default");
                foreach (var documentLibraryPath in documentLibraryPathsList)
                {
                    var documentLibrary = await context.Web.Lists.GetByServerRelativeUrlAsync(
                        documentLibraryPath);

                    var siteUri = new Uri(dataSource.SiteUrl!);
                    foreach (var item in documentLibrary.Items.QueryProperties(
                            p => p.Title,
                            p => p.FileSystemObjectType,
                            p => p.File.QueryProperties(f => f.Name, f => f.ServerRelativeUrl)))
                    {
                        if (item.FileSystemObjectType == FileSystemObjectType.Folder)
                            continue; // Skip folders, only process files

                        var canonicalId = item.File.ServerRelativeUrl;
                        contentItems.Add(new DataPipelineContentItem
                        {
                            Id = $"content-item-{Path.GetFileNameWithoutExtension(item.File.Name)}-{Guid.NewGuid().ToBase64String()}",
                            DataSourceObjectId = _dataSourceObjectId,
                            ContentIdentifier = new ContentIdentifier
                            {
                                MultipartId =
                                    [
                                        siteUri.Host,
                                            siteUri.PathAndQuery.Trim('/'),
                                            item.File.ServerRelativeUrl
                                                [..^item.File.Name.Length]
                                                [siteUri.PathAndQuery.Length..]
                                                .Trim('/'),
                                            item.File.Name
                                    ],
                                CanonicalId = canonicalId
                            },
                            ContentAction = ContentItemActions.AddOrUpdate
                        });
                    }
                }
            }

            return contentItems;
        }

        /// <inheritdoc/>
        public async Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            ContentIdentifier contentItemIdentifier)
        {
            var contentItemCanonicalId = contentItemIdentifier.CanonicalId;

            var documentLibraryPathsList = _pluginParameters[PluginParameterNames.SHAREPOINTONLINE_DATASOURCE_DOCUMENTLIBRARIES]?.ToString()
                ?? throw new PluginException($"The {PluginParameterNames.SHAREPOINTONLINE_DATASOURCE_DOCUMENTLIBRARIES} parameter is required by the {Name} plugin.");

            var documentLibraryPaths = documentLibraryPathsList.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var dataSourceBase = await _dataSourceResourceProvider!.GetResourceAsync<DataSourceBase>(
                _dataSourceObjectId,
                ServiceContext.ServiceIdentity!)
                ?? throw new PluginException($"The SharePoint Online data source with object ID '{_dataSourceObjectId}' cannot be found.");

            var dataSource = dataSourceBase as SharePointOnlineSiteDataSource
                ?? throw new PluginException($"The data source [{_dataSourceObjectId}] is not a valid SharePoint Online Site data source.");

            var sharePointSettings = dataSource.ConfigurationReferences!
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => _configuration[kvp.Value]);

            var pnpServiceProvider = await GetServiceProvider(
                dataSource.SiteUrl!,
                sharePointSettings!);

            List<DataPipelineContentItem> contentItems = [];
            using var scope = pnpServiceProvider.CreateScope();
            var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

            using var context = await pnpContextFactory.CreateAsync("Default");

            // Get a reference to the file
            var document = await context.Web.GetFileByServerRelativeUrlAsync(contentItemCanonicalId);

            Stream downloadedContentStream = await document.GetContentAsync(true);
            var binaryData = await BinaryData.FromStreamAsync(downloadedContentStream, default);
            var contentTypeResult = FileUtils.GetFileContentType(document.Name, binaryData);

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
                    Name = document.Name,
                    ContentType = contentTypeResult.ContentType,
                    RawContent = binaryData,
                    Metadata = new Dictionary<string, object>
                    {
                        { "FileId", contentItemCanonicalId },
                        { "FileName", document.Name }
                    }
                },
                true,
                false);
        }

        /// <inheritdoc/>
        public async Task HandleUnsafeContentItem(string canonicalContentItemIdentifier)
        {
            // No action needed for SharePoint Online data source.
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
        /// Retrieves a X.509 certificate from the specified Azure KeyVault.
        /// </summary>
        /// <returns>The X.509 certificate.</returns>
        private async Task<X509Certificate2> GetCertificate(
            Dictionary<string, string> settings)
        {
            var certificateClient = new CertificateClient(
                new Uri(settings[DataSourceConfigurationReferenceKeys.KeyVaultUrl]),
                ServiceContext.AzureCredential);
            var certificateWithPolicy = await certificateClient.GetCertificateAsync(
                settings[DataSourceConfigurationReferenceKeys.CertificateName]);
            var certificateIdentifier = new KeyVaultSecretIdentifier(
                certificateWithPolicy.Value.SecretId);

            var secretClient = new SecretClient(
                new Uri(settings[DataSourceConfigurationReferenceKeys.KeyVaultUrl]),
                ServiceContext.AzureCredential);
            var secret = await secretClient.GetSecretAsync(
                certificateIdentifier.Name,
                certificateIdentifier.Version);
            var secretBytes = Convert.FromBase64String(
                secret.Value.Value);

            return new X509Certificate2(secretBytes);
        }

        private async Task<IServiceProvider> GetServiceProvider(
            string siteUrl,
            Dictionary<string, string> settings)
        {
            var certificate = await GetCertificate(settings);
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPnPCore(options =>
            {
                var authProvider = new X509CertificateAuthenticationProvider(
                    settings[DataSourceConfigurationReferenceKeys.ClientId],
                    settings[DataSourceConfigurationReferenceKeys.TenantId],
                    certificate);
                options.DefaultAuthenticationProvider = authProvider;
                options.Sites.Add("Default",
                    new PnPCoreSiteOptions
                    {
                        SiteUrl = siteUrl,
                        AuthenticationProvider = authProvider
                    });
            });
            services.AddPnPContextFactory();
            return services.BuildServiceProvider();
        }
    }
}
