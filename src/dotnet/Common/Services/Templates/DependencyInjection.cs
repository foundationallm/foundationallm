using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Templates;
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
        /// Registers the <see cref="ITemplatingService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddRegexTemplatingService(this IHostApplicationBuilder builder) =>
            builder.Services.AddSingleton<ITemplatingService, RegexTemplatingService>();

        /// <summary>
        /// Registers the <see cref="ITemplatingService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddRegexTemplatingService(this IServiceCollection services) =>
            services.AddSingleton<ITemplatingService, RegexTemplatingService>();
    }
}
