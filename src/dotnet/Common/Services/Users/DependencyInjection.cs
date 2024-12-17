using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Users;
using Microsoft.Extensions.Configuration;
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
        /// Registers the <see cref="IUserProfileService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddUserProfileService(this IHostApplicationBuilder builder) =>
            builder.Services.AddUserProfileService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="IUserProfileService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddUserProfileService(this IServiceCollection services, IConfigurationManager configuration) =>
            services.AddScoped<IUserProfileService, UserProfileService>();
    }
}
