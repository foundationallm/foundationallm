using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
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
            DefaultAuthentication.Initialize(false, string.Empty);

            var configRoot = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json", true)
				.AddJsonFile("testsettings.e2e.json", true)
				.AddEnvironmentVariables()
				.AddUserSecrets<Environment>()
				.AddAzureAppConfiguration((Action<AzureAppConfigurationOptions>)(options =>
				{
					var connectionString = Environment.Variable(EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString);
					if (string.IsNullOrEmpty(connectionString))
					{
						throw new InvalidOperationException("Azure App Configuration connection string is not set.");
					}
                    options.Connect(connectionString)
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(DefaultAuthentication.AzureCredential);
                        })
                        // Select all configuration sections
                        .Select("*");
				}))
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
