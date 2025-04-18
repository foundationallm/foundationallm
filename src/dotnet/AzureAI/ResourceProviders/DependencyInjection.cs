using AzureAI.ResourceProviders;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the FoundationaLLM.AzureAI resource provider as a singleton service.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <remarks>
        /// Requires an <see cref="GatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureAIResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.AddAzureAIResourceProviderStorage();
            builder.Services.AddSingleton<IResourceProviderService, AzureAIResourceProviderService>(sp =>
                new AzureAIResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AzureAI),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureCosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILogger<AzureAIResourceProviderService>>()));

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
