using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Plugin.Validation;
using FoundationaLLM.Vector.ResourceProviders;
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
        /// Register the FoundationaLLM.Vector resource provider with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddVectorResourceProvider(
            this IHostApplicationBuilder builder,
            bool proxyMode = false) =>
            builder.Services.AddVectorResourceProvider(
                builder.Configuration,
                proxyMode: proxyMode);

        /// <summary>
        /// Registers the FoundationaLLM.Vector resource provider with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddVectorResourceProvider(
            this IServiceCollection services,
            IConfiguration configuration,
            bool proxyMode = false)
        {
            services.AddVectorResourceProviderStorage(configuration);

            // Register validators.
            services.AddSingleton<IValidator<VectorDatabase>, VectorDatabaseValidator>();

            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            services.AddSingleton<IResourceProviderService>(sp =>
                new VectorResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vector),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>(),
                    proxyMode: proxyMode));

            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
