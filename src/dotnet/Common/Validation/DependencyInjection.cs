using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Validation;
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
        /// Registers the resource validator factory with the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddResourceValidatorFactory(this IHostApplicationBuilder builder) =>
            builder.Services.AddResourceValidatorFactory();

        /// <summary>
        /// Registers the resource validator factory with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddResourceValidatorFactory(this IServiceCollection services) =>
            services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();
    }
}
