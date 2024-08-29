using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Conversation.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the FoundationaLLM.Conversation resource provider as a singleton service.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <remarks>
        /// Requires an <see cref="ICosmosDBService"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddConversationResourceProvider(this IHostApplicationBuilder builder) =>
            builder.Services.AddConversationResourceProvider(builder.Configuration);

        /// <summary>
        /// Registers the FoundationaLLM.Conversation resource provider as a singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationRoot"/> configuration manager.</param>
        /// <remarks>
        /// Requires an <see cref="ICosmosDBService"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddConversationResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddSingleton<IResourceProviderService, ConversationResourceProviderService>(sp =>
                new ConversationResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationService>(),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<ICosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILogger<ConversationResourceProviderService>>()));

            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
