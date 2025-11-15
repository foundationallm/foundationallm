using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Dependency injection configuration for analytics services.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds analytics services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
        {
            services.AddScoped<IAnonymizationService, AnonymizationService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IAbuseDetectionService, AbuseDetectionService>();

            return services;
        }
    }
}
