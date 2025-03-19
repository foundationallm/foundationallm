using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register the <see cref="InstanceSettings"/> providing the FoundationaLLM instance properties with the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddInstanceProperties(this IHostApplicationBuilder builder) =>
            builder.Services.AddInstanceProperties(builder.Configuration);

        /// <summary>
        /// Register the <see cref="InstanceSettings"/> providing the FoundationaLLM instance properties with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddInstanceProperties(this IServiceCollection services, IConfiguration configuration) =>
            services.AddOptions<InstanceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
    }
}
