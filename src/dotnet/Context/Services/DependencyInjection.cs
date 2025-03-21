using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides dependency injection extensions for the FoundationaLLM Context service.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds the <see cref="IFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddFileService(this IHostApplicationBuilder builder) =>
            builder.Services.AddFileService(builder.Configuration);

        /// <summary>
        /// Adds the <see cref="IFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddFileService(this IServiceCollection services, IConfiguration configuration) =>
            services.AddScoped<IFileService, FileService>();
    }
}
