using FluentValidation;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.DataSource.Models;
using FoundationaLLM.DataSource.ResourceProviders;
using FoundationaLLM.DataSource.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Data Source resource provider service implementation of resource provider dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Data Source Rrsource provider and its related services the the dependency injection container.
        /// </summary>
        /// <param name="services">Application builder service collection</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> providing configuration services.</param>
        public static void AddDataSourceResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_DataSource_ResourceProviderService_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource
                };
            });

            // Register validators.
            services.AddSingleton<IValidator<DataSourceBase>, DataSourceBaseValidator>();
            services.AddSingleton<IValidator<AzureDataLakeDataSource>, AzureDataLakeDataSourceValidator>();
            services.AddSingleton<IValidator<AzureSQLDatabaseDataSource>, AzureSQLDatabaseDataSourceValidator>();
            services.AddSingleton<IValidator<SharePointOnlineSiteDataSource>, SharePointOnlineSiteDataSourceValidator>();

            services.AddSingleton<IResourceProviderService, DataSourceResourceProviderService>(sp =>
                new DataSourceResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<ILoggerFactory>()));
            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
