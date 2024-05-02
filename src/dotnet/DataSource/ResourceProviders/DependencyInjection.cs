using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.DataSource.Models;
using FoundationaLLM.DataSource.ResourceProviders;
using FoundationaLLM.DataSource.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        /// <param name="builder">The application builder.</param>
        public static void AddDataSourceResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_DataSource_ResourceProviderService_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
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
            builder.Services.AddSingleton<IValidator<DataSourceBase>, DataSourceBaseValidator>();
            builder.Services.AddSingleton<IValidator<OneLakeDataSource>, OneLakeDataSourceValidator>();
            builder.Services.AddSingleton<IValidator<AzureDataLakeDataSource>, AzureDataLakeDataSourceValidator>();
            builder.Services.AddSingleton<IValidator<AzureSQLDatabaseDataSource>, AzureSQLDatabaseDataSourceValidator>();
            builder.Services.AddSingleton<IValidator<SharePointOnlineSiteDataSource>, SharePointOnlineSiteDataSourceValidator>();

            builder.Services.AddSingleton<IResourceProviderService, DataSourceResourceProviderService>(sp =>
                new DataSourceResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationService>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>()));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
