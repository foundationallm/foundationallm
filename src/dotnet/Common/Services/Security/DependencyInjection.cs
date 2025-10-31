using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Security;
using FoundationaLLM.Common.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System;

namespace FoundationaLLM
{
    /// <summary>
    /// Dependency injection services for security services.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add group membership services to dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddIdentitiyManagement(this IHostApplicationBuilder builder) =>
            builder.Services.AddIdentitiyManagement(builder.Configuration);

        /// <summary>
        /// Registers the FoundationaLLM.Conversation resource provider as a singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <remarks>
        /// Requires an <see cref="IAzureCosmosDBService"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddIdentitiyManagement(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<MicrosoftGraphIdentityManagementServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_IdentityManagement_MicrosoftGraph));

            services.AddSingleton<IIdentityManagementService, MicrosoftGraphIdentityManagementService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MicrosoftGraphIdentityManagementServiceSettings>>().Value;
                var httpClient = GraphClientFactory.Create();
                httpClient.Timeout = TimeSpan.FromMinutes(15);

                return new MicrosoftGraphIdentityManagementService(
                    settings,
                    new GraphServiceClient(
                        httpClient,
                        ServiceContext.AzureCredential,
                        new[] { "https://graph.microsoft.com/.default" }),
                    sp.GetRequiredService<ILogger<MicrosoftGraphIdentityManagementService>>());
            });
        }
    }
}
