using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Analytics;
using FoundationaLLM.Common.Services.Analytics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Dependency injection configuration for analytics services.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the Analytics Services implementations with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddAnalyticsServices(this IHostApplicationBuilder builder) =>
            builder.Services.AddAnalyticsServices(
                builder.Configuration);

        /// <summary>
        /// Registers the Analytics Services implementations with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddAnalyticsServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<AnalyticsSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Analytics));

            services.AddScoped<IAnonymizationService, AnonymizationService>(sp => new AnonymizationService(
                sp.GetRequiredService<IOptions<AnalyticsSettings>>(),
                sp.GetRequiredService<ILogger<AnonymizationService>>()));
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IAbuseDetectionService, AbuseDetectionService>(sp => new AbuseDetectionService(
                sp.GetRequiredService<IAnalyticsService>(),
                sp.GetRequiredService<IOptions<AnalyticsSettings>>(),
                sp.GetRequiredService<ILogger<AbuseDetectionService>>()));
        }
    }
}
