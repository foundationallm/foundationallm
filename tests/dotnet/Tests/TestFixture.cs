using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Tests.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;


namespace FoundationaLLM.Tests 
{ 
    public class TestFixture : IDisposable
    {
        protected readonly HostApplicationBuilder _hostBuilder;

        public IHostApplicationBuilder HostBuilder => _hostBuilder;

        public TestFixture()
        {
            _hostBuilder = Host.CreateApplicationBuilder();
            ServiceContext.Initialize(false, string.Empty);

            _hostBuilder.Configuration.Sources.Clear();
            _hostBuilder.Configuration
                .AddJsonFile("testsettings.json", true)
                .AddJsonFile("testsettings.e2e.json", true)
                .AddEnvironmentVariables()
                .AddUserSecrets<TestEnvironment>()
                .AddAzureAppConfiguration((Action<AzureAppConfigurationOptions>)(options =>
                {
                    var connectionString = TestEnvironment.Variable(EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("Azure App Configuration connection string is not set.");
                    }
                    options.Connect(connectionString)
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(ServiceContext.AzureCredential);
                        })
                        // Select all configuration sections
                        .Select("*");
                }))
                .Build();
        }

        public void Dispose()
        {
        }
    }
}
