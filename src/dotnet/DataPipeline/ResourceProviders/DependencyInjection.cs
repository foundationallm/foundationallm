using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation.Plugins;
using FoundationaLLM.DataPipeline.Clients;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipeline.ResourceProviders;
using FoundationaLLM.DataPipeline.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the FoundationaLLM DataPipeline resource provider with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddDataPipelineResourceProvider(
            this IHostApplicationBuilder builder,
            bool proxyMode = false) =>
            builder.Services.AddDataPipelineResourceProvider(
                builder.Configuration,
                proxyMode: proxyMode);

        /// <summary>
        /// Registers the FoundationaLLM.DataPipeline resource provider with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddDataPipelineResourceProvider(
            this IServiceCollection services,
            IConfiguration configuration,
            bool proxyMode = false)
        {
            services.AddDataPipelineResourceProviderStorage(configuration);

            // Register validators.
            services.AddSingleton<IValidator<DataPipelineDefinition>, DataPipelineDefinitionValidator>();
            services.AddSingleton<IValidator<DataPipelineRun>, DataPipelineRunValidator>();
            services.AddSingleton<IValidator<PluginComponent>, PluginComponentValidator>();

            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            services.AddSingleton<IResourceProviderService>(sp =>
                new DataPipelineResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>(),
                    proxyMode: proxyMode));

            services.ActivateSingleton<IResourceProviderService>();
        }

        /// <summary>
        /// Registers the remote data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddRemoteDataPipelineServiceClient(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddRemoteDataPipelineServiceClient(builder.Configuration);

        /// <summary>
        /// Registers the remote data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        public static void AddRemoteDataPipelineServiceClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IDataPipelineServiceClient, RemoteDataPipelineServiceClient>();
            services.ActivateSingleton<IDataPipelineServiceClient>();
        }
    }
}
