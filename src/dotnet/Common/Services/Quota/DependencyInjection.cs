using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Environment;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Quota;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds the FoundationaLLM quota storage service to the dependency injection container.
        /// Use this when you only need access to the quota storage (e.g., in Management API)
        /// without the full QuotaService.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddQuotaStorage(this IHostApplicationBuilder builder) =>
            AddQuotaStorage(builder.Services, builder.Configuration);

        /// <summary>
        /// Adds the FoundationaLLM quota storage service to the dependency injection container.
        /// Use this when you only need access to the quota storage (e.g., in Management API)
        /// without the full QuotaService.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddQuotaStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_Quota)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Quota_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_Quota);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_Quota
                };
            });
        }

        /// <summary>
        /// Adds the FoundationaLLM quota service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddQuotaService(this IHostApplicationBuilder builder) =>
            AddQuotaService(builder.Services, builder.Configuration);

        /// <summary>
        /// Adds the FoundationaLLM quota service to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddQuotaService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the quota storage if not already registered
            AddQuotaStorage(services, configuration);

            services.AddSingleton<IQuotaService, QuotaService>(sp =>
            {
                // Try to get Cosmos DB service if available (optional)
                IAzureCosmosDBService? cosmosDBService = null;
                try
                {
                    cosmosDBService = sp.GetService<IAzureCosmosDBService>();
                }
                catch
                {
                    // Cosmos DB service not available, continue without it
                }

                return new QuotaService(
                    sp.GetRequiredService<DependencyInjectionContainerSettings>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_Quota),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<ILoggerFactory>(),
                    cosmosDBService);
            });
            services.ActivateSingleton<IQuotaService>();
        }
    }
}
