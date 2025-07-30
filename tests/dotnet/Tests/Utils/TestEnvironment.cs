using FoundationaLLM.Common.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FoundationaLLM.Tests.Utils
{
    public sealed class TestEnvironment
	{
		private IServiceProvider _serviceProvider = null!;
		private readonly IConfigurationRoot _configuration;

		public IConfigurationRoot Configuration => _configuration;

        public TestEnvironment()
		{
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<TestEnvironment>()
                .AddJsonFile("testsettings.json", true)
                .Build();
        }

        public void InitializeServices(
            ITestOutputHelper testOutputHelper)
        {
            ServiceContext.Initialize(false, "TestEnvironment");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                builder.AddConsole();
                builder.AddConfiguration(_configuration.GetSection("Logging"));
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        public T GetRequiredService<T>() where T : notnull =>
            _serviceProvider.GetRequiredService<T>();
    }
}
