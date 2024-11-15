using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Environment = FoundationaLLM.Core.Examples.Utils.Environment;

namespace FoundationaLLM.Core.Examples.LoadTests.Setup
{
    /// <summary>
    /// Test fixture to initialize services for the tests.
    /// </summary>
    public class LoadTestFixture : IDisposable
	{
		public List<IServiceProvider> ServiceProviders { get; private set; }

		public LoadTestFixture()
		{
			var builder = Host.CreateApplicationBuilder();
            DefaultAuthentication.Initialize(false, string.Empty);

            builder.Configuration.Sources.Clear();
            builder.Configuration
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

			// The number of service hosts to create
			int hostsCount = 1;

			ServiceProviders = LoadTestServicesInitializer.InitializeServices(builder, hostsCount);

			var app = builder.Build();
		}

		public void Dispose()
		{
			foreach (var serviceProvider in ServiceProviders)
				if (serviceProvider is IDisposable disposable)
				{
					disposable.Dispose();
				}
		}
	}
}
