using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
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
            builder.Services.AddOptions<ResourceProviderCacheSettings>()
                .Bind(builder.Configuration.GetSection(
                    AppConfigurationKeySections.FoundationaLLM_ResourceProvidersCache));
    }
}
