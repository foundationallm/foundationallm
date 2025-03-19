using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// Global dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the resource provider settings with the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddResourceProviderCacheSettings(this IHostApplicationBuilder builder) =>
            AddResourceProviderCacheSettings(builder.Services, builder.Configuration);

        /// <summary>
        /// Registers the resource provider settings with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddResourceProviderCacheSettings(this IServiceCollection services, IConfiguration configuration) =>
            services.AddOptions<ResourceProviderCacheSettings>()
                .Bind(configuration.GetSection(
                    AppConfigurationKeySections.FoundationaLLM_ResourceProvidersCache));
    }
}
