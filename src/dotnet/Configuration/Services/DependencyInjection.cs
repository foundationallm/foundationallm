using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Configuration.Interfaces;
using FoundationaLLM.Configuration.Services;
using FoundationaLLM.Configuration.Validation;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register the FoundatiionaLLM.Configuration resource provider with the dependency injection container.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        public static void AddConfigurationResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.AddAzureKeyVaultService(AppConfigurationKeys.FoundationaLLM_Configuration_KeyVaultURI);

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddConfigurationClient(
                    builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
            });

            builder.Services.AddSingleton<IAzureAppConfigurationService, AzureAppConfigurationService>();
            builder.Services.AddSingleton<IConfigurationHealthChecks, ConfigurationHealthChecks>();
            builder.Services.AddHostedService<ConfigurationHealthCheckService>();

            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Configuration_ResourceProviderService_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration
                };
            });

            builder.Services.AddSingleton<IResourceProviderService, ConfigurationResourceProviderService>(sp =>
                new ConfigurationResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationService>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureAppConfigurationService>(),
                    sp.GetRequiredService<IAzureKeyVaultService>(),
                    builder.Configuration,
                    sp.GetRequiredService<ILogger<ConfigurationResourceProviderService>>()));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
