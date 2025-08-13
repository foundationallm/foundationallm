using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
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
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddDataSourceResourceProvider(
            this IHostApplicationBuilder builder,
            bool proxyMode = false) =>
            builder.Services.AddDataSourceResourceProvider(
                builder.Configuration,
                proxyMode: proxyMode);

        /// <summary>
        /// Registers the FoundationaLLM.DataSource resource provider with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddDataSourceResourceProvider(
            this IServiceCollection services,
            IConfiguration configuration,
            bool proxyMode = false)
        {
            services.AddDataSourceResourceProviderStorage(configuration);

            // Register validators.
            services.AddSingleton<IValidator<DataSourceBase>, DataSourceBaseValidator>();
            services.AddSingleton<IValidator<OneLakeDataSource>, OneLakeDataSourceValidator>();
            services.AddSingleton<IValidator<AzureDataLakeDataSource>, AzureDataLakeDataSourceValidator>();
            services.AddSingleton<IValidator<AzureSQLDatabaseDataSource>, AzureSQLDatabaseDataSourceValidator>();
            services.AddSingleton<IValidator<SharePointOnlineSiteDataSource>, SharePointOnlineSiteDataSourceValidator>();

            services.AddSingleton<IResourceProviderService, DataSourceResourceProviderService>(sp =>
                new DataSourceResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>(),
                    proxyMode: proxyMode));
            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
