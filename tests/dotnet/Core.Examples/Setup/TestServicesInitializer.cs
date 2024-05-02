using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.AzureAI;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.Examples.Setup
{
    public static class TestServicesInitializer
	{
		/// <summary>
		/// Configure base services and dependencies for the tests.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configRoot"></param>
		public static void InitializeServices(
			IServiceCollection services,
			IConfigurationRoot configRoot)
		{
			TestConfiguration.Initialize(configRoot, services);

			RegisterHttpClients(services);
			RegisterCosmosDb(services, configRoot);
			RegisterLogging(services);
			RegisterAzureAIService(services, configRoot);
		}

		private static void RegisterHttpClients(IServiceCollection services)
		{
			var coreApiUri = TestConfiguration.GetAppConfigValueAsync(AppConfigurationKeys.FoundationaLLM_APIs_CoreAPI_APIUrl).GetAwaiter().GetResult();
			
			services
				.AddHttpClient(
					HttpClients.CoreAPI,
					client => { 
						client.BaseAddress = new Uri(coreApiUri);
						client.Timeout = TimeSpan.FromSeconds(120);
					})
				.AddResilienceHandler(
					"DownstreamPipeline",
					static strategyBuilder =>
					{
						CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
					});
		}

		private static void RegisterCosmosDb(IServiceCollection services, IConfiguration configuration)
		{
			services.AddOptions<CosmosDbSettings>()
				.Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));

			services.AddSingleton<CosmosClient>(serviceProvider =>
			{
				var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
				return new CosmosClientBuilder(settings.Endpoint, DefaultAuthentication.GetAzureCredential())
					.WithSerializerOptions(new CosmosSerializationOptions
					{
						PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
					})
					.WithConnectionModeGateway()
					.Build();
			});

			services.AddScoped<ICosmosDbService, CosmosDbService>();
		}

		private static void RegisterAzureAIService(IServiceCollection services, IConfiguration configuration)
		{
			services.AddOptions<AzureAISettings>()
				.Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_AzureAIStudio));
			services.AddOptions<BlobStorageServiceSettings>()
				.Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_AzureAIStudio_BlobStorageServiceSettings));

			services.AddScoped<IAzureAIService, AzureAIService>();
			services.AddSingleton<IStorageService, BlobStorageService>();
		}

		private static void RegisterLogging(IServiceCollection services)
		{
			services.AddLogging(builder =>
			{
				builder.AddConsole();
			});
		}
	}

}
