using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Environment = FoundationaLLM.Core.Examples.Utils.Environment;

namespace FoundationaLLM.Core.Examples.Setup
{
	/// <summary>
	/// Test fixture to initialize services for the tests.
	/// </summary>
	public class TestFixture : IDisposable
	{
		public ServiceProvider ServiceProvider { get; private set; }

		public TestFixture()
		{
			var serviceCollection = new ServiceCollection();

			var configRoot = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json", true)
				.AddEnvironmentVariables()
				.AddUserSecrets<Environment>()
				.AddAzureAppConfiguration(options =>
				{
					var connectionString = Environment.Variable(EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString);
					if (string.IsNullOrEmpty(connectionString))
					{
						throw new InvalidOperationException("Azure App Configuration connection string is not set.");
					}
					options.Connect(connectionString)
						.ConfigureKeyVault(kv =>
						{
							kv.SetCredential(DefaultAuthentication.GetAzureCredential());
						})
						// Select the configuration sections to load:
						.Select(AppConfigurationKeyFilters.FoundationaLLM_CosmosDB)
						.Select(AppConfigurationKeyFilters.FoundationaLLM_AzureAIStudio)
						.Select(AppConfigurationKeyFilters.FoundationaLLM_AzureAIStudio_BlobStorageServiceSettings);
				})
				.Build();

			TestServicesInitializer.InitializeServices(serviceCollection, configRoot);

			ServiceProvider = serviceCollection.BuildServiceProvider();
		}

		public void Dispose()
		{
			if (ServiceProvider is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}
}
