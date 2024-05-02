using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Gateway.Client;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models.Configuration;
using FoundationaLLM.Gateway.Services;
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
        /// Adds the core Gateway service the the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddGatewayCore(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<GatewayCoreSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Gateway));

            builder.Services.AddSingleton<IGatewayCore, GatewayCore>();
            builder.Services.AddHostedService<GatewayWorker>();
        }

        /// <summary>
        /// Adds the Gateway API service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddGatewayService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<GatewayServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_GatewayAPI));

            builder.Services.AddScoped<IGatewayServiceClient, GatewayServiceClient>();
        }
    }
}
