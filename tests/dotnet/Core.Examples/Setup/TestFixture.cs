using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;
using Environment = FoundationaLLM.Core.Examples.Utils.Environment;

namespace FoundationaLLM.Core.Examples.Setup
{
    public class TestFixture : IDisposable
    {
        public List<IServiceProvider> ServiceProviders { get; private set; } = [];

        protected readonly HostApplicationBuilder _hostBuilder;

        public TestFixture()
        {
            _hostBuilder = Host.CreateApplicationBuilder();
            ServiceContext.Initialize(false, string.Empty);

            _hostBuilder.Configuration.Sources.Clear();
            _hostBuilder.Configuration
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
                            kv.SetCredential(ServiceContext.AzureCredential);
                        })
                        // Select all configuration sections
                        .Select("*");
                }))
                .Build();
        }

        public void ConfigureServiceProviders(
            int virtualHostsCount,
            ITestOutputHelper output,
            Func<HostApplicationBuilder, ITestOutputHelper, int, List<IServiceProvider>> serviceProviderBuilder)
        {
            if (ServiceProviders.Count == 0)
                ServiceProviders.AddRange(
                    serviceProviderBuilder(_hostBuilder, output, virtualHostsCount));
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
