using FoundationaLLM.Common.Models.Configuration.Environment;
using FoundationaLLM.Tests;
using FoundationaLLM.Tests.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FoundationaLLM.Testing
{
    /// <summary>
    /// Initializes a dependency injection container with the necessary FoundationaLLM services and dependencies.
    /// </summary>
    public class DependencyInjectionContainerInitializerBase
	{
        /// <summary>
        /// Configures a dependency injection container service collection.
        /// </summary>
        /// <param name="containerId">The dependency injection container identifier.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        /// <param name="testOutputHelper">The <see cref="ITestOutputHelper"/> xUnit test output helper used to log test results.</param>
        /// <param name="testConfiguration">The class derived from <see cref="TestConfigurationBase"/> that contains the test configuration.</param>
        public void InitializeServices(
            int containerId,
			IServiceCollection services,
			IConfiguration configuration,
            ITestOutputHelper testOutputHelper)
		{
            services.AddDIContainerSettings(new DependencyInjectionContainerSettings {
                Id = containerId
            });
            services.AddInstanceProperties(configuration);
            services.AddAuthorizationServiceClient(configuration);

            InitializeTestConfiguration(services, configuration);

            services.AddScoped<IConfiguration>(_ => configuration);

            RegisterLogging(services, configuration, testOutputHelper);
            RegisterHttpClients(services, configuration);
            RegisterClientLibraries(services, configuration);
			RegisterCosmosDB(services, configuration);
            RegisterAzureAIService(services, configuration);
			RegisterServiceManagers(services);

            services.AddResourceProviderCacheSettings(configuration);
            services.AddResourceValidatorFactory();
            RegisterResourceProviders(services, configuration);

            RegisterEventService(services, configuration);
            RegisterOtherServices(services, configuration);
        }

        protected virtual void InitializeTestConfiguration(
            IServiceCollection services,
            IConfiguration configuration)
        {
        }

        protected virtual void RegisterLogging(
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

        protected virtual void RegisterClientLibraries(
            IServiceCollection services,
            IConfiguration configuration)
        {
        }

		protected virtual void RegisterCosmosDB(
            IServiceCollection services,
            IConfiguration configuration)
		{
		}

		protected virtual void RegisterAzureAIService(
            IServiceCollection services,
            IConfiguration configuration)
		{
		}

        protected virtual void RegisterHttpClients(
            IServiceCollection services,
            IConfiguration configuration)
        {
        }

        protected virtual void RegisterServiceManagers(
            IServiceCollection services)
        {
        }

        protected virtual void RegisterResourceProviders(
            IServiceCollection services,
            IConfiguration configuration)
        {
        }

        protected virtual void RegisterEventService(
            IServiceCollection services,
            IConfiguration configuration)
        {
        }

        protected virtual void RegisterOtherServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
        }
    }
}
