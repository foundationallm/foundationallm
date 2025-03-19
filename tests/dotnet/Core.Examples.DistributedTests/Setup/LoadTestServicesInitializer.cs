using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Core.Examples.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.DistributedTests.Setup
{
    /// <summary>
    /// Configure base services and dependencies for the tests.
    /// </summary>
    public class LoadTestServicesInitializer
    {
        /// <summary>
        /// Configure base services and dependencies for the tests.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> that is used to build the application host.</param>
        /// <param name="configRoot">The <see cref="IConfigurationRoot"/> that provides configuration services.</param>
        public static List<IServiceProvider> InitializeServices(
            IHostApplicationBuilder builder,
            ITestOutputHelper testOutputHelper,
            int hostsCount)
        {
            LoadTestConfiguration.Initialize((IConfigurationRoot)builder.Configuration, builder.Services);
            
            return Enumerable.Range(1, hostsCount)
                .Select(_ => CreateDIContainer(builder, testOutputHelper))
                .ToList();
        }

        private static IServiceProvider CreateDIContainer(
            IHostApplicationBuilder builder,
            ITestOutputHelper testOutputHelper)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(builder.Configuration);

            // Register services

            serviceCollection.AddInstanceProperties(builder.Configuration);
            serviceCollection.AddAuthorizationServiceClient(builder.Configuration);
            serviceCollection.AddHttpClientFactoryService();
            serviceCollection.AddAzureResourceManager();
            // Using the Management API configuration section for the Event Grid profile
            // to make sure no events are being watched.
            serviceCollection.AddAzureEventGridEvents(builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_Profiles_ManagementAPI);
            serviceCollection.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

            serviceCollection.AddAzureOpenAIResourceProvider(builder.Configuration);

            RegisterLogging(serviceCollection, builder.Configuration, testOutputHelper);

            // Build the child service provider
            return serviceCollection.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });
        }

        private static void RegisterLogging(
            IServiceCollection services,
            IConfiguration config,
            ITestOutputHelper testOutputHelper)
        {
            services.AddLogging(builder =>
            {
                builder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                builder.AddConsole();
                builder.AddConfiguration(config.GetSection("Logging"));
            });
        }
    }
}
