using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the resource path availability checker service with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddCoreResourcePathAvailabilityCheckerService(this IHostApplicationBuilder builder) =>
            builder.Services.AddCoreResourcePathAvailabilityCheckerService();

        /// <summary>
        /// Registers the resource path availability checker service with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddCoreResourcePathAvailabilityCheckerService(this IServiceCollection services)
        {
            services.AddSingleton<IResourcePathAvailabilityCheckerService, CoreResourcePathAvailabilityCheckerService>();
            services.ActivateSingleton<IResourcePathAvailabilityCheckerService>();
        }
    }
}
