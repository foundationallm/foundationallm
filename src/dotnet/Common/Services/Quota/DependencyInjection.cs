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

            services.AddSingleton<IQuotaService, QuotaService>(sp =>
                new QuotaService(
                    sp.GetRequiredService<DependencyInjectionContainerSettings>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_Quota),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<ILoggerFactory>()));
            services.ActivateSingleton<IQuotaService>();
        }
    }
}
