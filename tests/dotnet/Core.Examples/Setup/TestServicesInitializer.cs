using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.AzureAI;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Exceptions;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Services;
using FoundationaLLM.Core.Examples.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Setup
{
    public static class TestServicesInitializer
	{
        /// <summary>
        /// Configure base services and dependencies for the tests.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void InitializeServices(
			IServiceCollection services,
			IConfiguration configuration,
            ITestOutputHelper testOutputHelper)
		{
			TestConfiguration.Initialize(configuration, services);

            services.AddOptions<BlobStorageServiceSettings>(
                    DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization_Storage)
                .Bind(configuration.GetSection("FoundationaLLM:ResourceProviders:Vectorization:Storage"));

            services.AddScoped<IConfiguration>(_ => configuration);

            RegisterLogging(services, configuration, testOutputHelper);

            RegisterInstance(services, configuration);
            RegisterHttpClients(services, configuration);
            RegisterClientLibraries(services, configuration);
			RegisterCosmosDb(services, configuration);
            RegisterAzureAIService(services, configuration);
			RegisterServiceManagers(services);

            services.AddAPIRequestQuotaService(configuration);
        }

        private static void RegisterLogging(
            IServiceCollection services,
            IConfiguration configuration,
            ITestOutputHelper testOutputHelper)
        {
            services.AddLogging(builder =>
            {
                builder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                builder.AddConsole();
                builder.AddConfiguration(configuration.GetSection("Logging"));
            });
        }

        private static void RegisterInstance(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<InstanceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
        }

        private static void RegisterClientLibraries(IServiceCollection services, IConfiguration configuration)
        {
            var instanceId = configuration.GetValue<string>(AppConfigurationKeys.FoundationaLLM_Instance_Id);
            services.AddCoreClient(
                configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl]!,
                ServiceContext.AzureCredential!,
                instanceId);
            services.AddManagementClient(
                configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_ManagementAPI_Essentials_APIUrl]!,
                ServiceContext.AzureCredential!,
                instanceId);
        }

		private static void RegisterCosmosDb(IServiceCollection services, IConfiguration configuration)
		{
			services.AddOptions<CosmosDbSettings>()
				.Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB));

			services.AddSingleton<CosmosClient>(serviceProvider =>
			{
				var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
				return new CosmosClientBuilder(settings.Endpoint, ServiceContext.AzureCredential)
					.WithSerializerOptions(new CosmosSerializationOptions
					{
						PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
					})
					.WithConnectionModeGateway()
					.Build();
			});

			services.AddScoped<IAzureCosmosDBService, AzureCosmosDBService>();
		}

		private static void RegisterAzureAIService(IServiceCollection services, IConfiguration configuration)
		{
            try
            {
                var completionQualityMeasurementConfiguration = TestConfiguration.CompletionQualityMeasurementConfiguration;
                if (completionQualityMeasurementConfiguration is { AgentPrompts: not null })
                {
                    services.AddOptions<AzureAISettings>()
                        .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_AzureAIStudio_Configuration));
                    services.AddOptions<BlobStorageServiceSettings>()
                        .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_AzureAIStudio_Configuration_Storage));

                    services.AddScoped<IAzureAIService, AzureAIService>();
                    services.AddSingleton<IStorageService, BlobStorageService>();
                }
                else
                {
                    Console.WriteLine($"Skipping Azure AI Service initialization. No agent prompts defined in the {nameof(CompletionQualityMeasurementConfiguration)} configuration section.");
                }
            }
            catch (ConfigurationNotFoundException cex)
            {
                Console.WriteLine($"Skipping Azure AI Service initialization. {cex.Message}");
            }
		}

        private static void RegisterHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HttpClientOptions>(HttpClientNames.CoreAPI, options =>
            {
                options.BaseUri = configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl]!;
                options.Scope = configuration[AppConfigurationKeys.FoundationaLLM_UserPortal_Authentication_Entra_Scopes]!;
                options.Timeout = TimeSpan.FromSeconds(120);
            });

            services.Configure<HttpClientOptions>(HttpClientNames.ManagementAPI, options =>
            {
                options.BaseUri = configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_ManagementAPI_Essentials_APIUrl]!;
                options.Scope = configuration[AppConfigurationKeys.FoundationaLLM_ManagementPortal_Authentication_Entra_Scopes]!;
                options.Timeout = TimeSpan.FromSeconds(120);
            });

            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = []
            };

            services.AddHttpClient(HttpClientNames.CoreAPI)
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                    var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpClientOptions>>().Get(HttpClientNames.CoreAPI);
                    client.BaseAddress = new Uri(options.BaseUri!);
                    if (options.Timeout != null) client.Timeout = (TimeSpan)options.Timeout;
                })
                .AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
                {
                    CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                });

            services.AddHttpClient(HttpClientNames.ManagementAPI)
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                    var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpClientOptions>>().Get(HttpClientNames.ManagementAPI);
                    client.BaseAddress = new Uri(options.BaseUri!);
                    if (options.Timeout != null) client.Timeout = (TimeSpan)options.Timeout;
                })
                .AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
                {
                    CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                });

            services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);

            services.Configure<DownstreamAPISettings>(configuration.GetSection("DownstreamAPIs"));
        }

        private static void RegisterServiceManagers(IServiceCollection services)
        {
            services.AddScoped<ICoreAPITestManager, CoreAPITestManager>();
			services.AddScoped<IManagementAPITestManager, ManagementAPITestManager>();
            services.AddScoped<IAuthenticationService, MicrosoftEntraIDAuthenticationService>();
            services.AddScoped<IHttpClientManager, HttpClientManager>();
			services.AddScoped<IAgentConversationTestService, AgentConversationTestService>();
            services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();
            services.AddScoped<IVectorizationTestService, VectorizationTestService>();
        }
    }
}
