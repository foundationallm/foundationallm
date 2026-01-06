using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Infrastructure.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Infrastructure resource provider service implementation of resource provider dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Infrastructure resource provider and its related services to the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddInfrastructureResourceProvider(
            this IHostApplicationBuilder builder,
            bool proxyMode = false) =>
            builder.Services.AddInfrastructureResourceProvider(
                builder.Configuration,
                proxyMode: proxyMode);

        /// <summary>
        /// Registers the FoundationaLLM.Infrastructure resource provider with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddInfrastructureResourceProvider(
            this IServiceCollection services,
            IConfiguration configuration,
            bool proxyMode = false)
        {
            services.AddInfrastructureResourceProviderStorage(configuration);

            services.AddSingleton<IResourceProviderService, InfrastructureResourceProviderService>(sp =>
                new InfrastructureResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Infrastructure),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>(),
                    proxyMode: proxyMode));

            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
