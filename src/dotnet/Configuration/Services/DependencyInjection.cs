using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Graph;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.Graph;
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
        /// Register the handler as a hosted service, passing the step name to the handler ctor.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        public static void AddConfigurationResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.Services.AddAzureClients(clientBuilder =>
            {
                var keyVaultUri = builder.Configuration[AppConfigurationKeys.FoundationaLLM_Configuration_KeyVaultURI];
                clientBuilder.AddSecretClient(new Uri(keyVaultUri!))
                    .WithCredential(DefaultAuthentication.GetAzureCredential());
                clientBuilder.AddConfigurationClient(
                    builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
            });
            // Configure logging to filter out Azure Core and Azure Key Vault informational logs.
            builder.Logging.AddFilter("Azure.Core", LogLevel.Warning);
            builder.Logging.AddFilter("Azure.Security.KeyVault.Secrets", LogLevel.Warning);

            builder.Services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService>();
            builder.Services.AddSingleton<IAzureAppConfigurationService, AzureAppConfigurationService>();
            builder.Services.AddSingleton<IConfigurationHealthChecks, ConfigurationHealthChecks>();
            builder.Services.AddHostedService<ConfigurationHealthCheckService>();

            builder.Services.AddOptions<GraphServiceSettings>()
               .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_GraphAPI));
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Configuration_ResourceProviderService_Storage));

            builder.Services.AddSingleton<IGraphService, GraphService>();
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
