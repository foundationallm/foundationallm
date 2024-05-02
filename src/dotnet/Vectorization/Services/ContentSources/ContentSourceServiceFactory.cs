using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.DataSource.Models;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Manages content sources registered for use in the vectorization pipelines.
    /// </summary>
    /// <param name="resourceProviderServices">The resource provider services registered with the dependency injection container.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class ContentSourceServiceFactory(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IConfiguration configuration,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<IContentSourceService>
    {
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        private readonly IConfiguration _configuration = configuration;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public IContentSourceService GetService(string serviceName)
        {
            var (service, _) = this.GetServiceWithResource(serviceName);
            return service;
        }

        /// <inheritdoc/>
        public (IContentSourceService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            // serviceName is the data_source_object_id of the request
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataSource, out var dataSourceResourceProviderService);
            if (dataSourceResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var dataSource = dataSourceResourceProviderService.GetResource<DataSourceBase>(serviceName);
            return dataSource == null
                ? throw new VectorizationException($"The data source {serviceName} was not found.")
                : dataSource.Type switch
                    {
                        DataSourceTypes.AzureDataLake => (CreateAzureDataLakeContentSourceService(dataSource.Name), dataSource),
                        DataSourceTypes.SharePointOnlineSite => (CreateSharePointOnlineContentSourceService(dataSource.Name), dataSource),
                        DataSourceTypes.AzureSQLDatabase => (CreateAzureSQLDatabaseContentSourceService(dataSource.Name), dataSource),
                        DataSourceTypes.WebSite => (CreatePageContentSourceService(dataSource.Name), dataSource),
                        _ => throw new VectorizationException($"The data source type {dataSource.Type} is not supported."),
                    };
        }


        private DataLakeContentSourceService CreateAzureDataLakeContentSourceService(string serviceName)
        {
            var blobStorageServiceSettings = new BlobStorageServiceSettings { AuthenticationType = BlobStorageAuthenticationTypes.Unknown };
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{serviceName}",
                blobStorageServiceSettings);

            return new DataLakeContentSourceService(
                blobStorageServiceSettings,
                _loggerFactory);
        }

        private SharePointOnlineContentSourceService CreateSharePointOnlineContentSourceService(string serviceName)
        {
            var sharePointOnlineContentSourceServiceSettings = new SharePointOnlineContentSourceServiceSettings();
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{serviceName}",
                sharePointOnlineContentSourceServiceSettings);

            return new SharePointOnlineContentSourceService(
                sharePointOnlineContentSourceServiceSettings,
                _loggerFactory);
        }

        private AzureSQLDatabaseContentSourceService CreateAzureSQLDatabaseContentSourceService(string serviceName)
        {
            var azureSQLDatabaseContentSourceServiceSettings = new AzureSQLDatabaseContentSourceServiceSettings();
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{serviceName}",
                azureSQLDatabaseContentSourceServiceSettings);

            return new AzureSQLDatabaseContentSourceService(
                azureSQLDatabaseContentSourceServiceSettings,
                _loggerFactory);
        }

        private WebContentSourceService CreatePageContentSourceService(string serviceName)
            => new WebContentSourceService(_loggerFactory);
    }
}
