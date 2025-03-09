using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using FoundationaLLM.Plugin.ResourceProviders;
using FoundationaLLM.Plugin.Validation;
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
        /// Register the FoundationaLLM Plugin resource provider with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddPluginResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.AddPluginResourceProviderStorage();

            // Register validators.
            builder.Services.AddSingleton<IValidator<PluginPackageDefinition>, PluginPackageDefinitionValidator>();
            builder.Services.AddSingleton<IValidator<PluginDefinition>, PluginDefinitionValidator>();

            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            builder.Services.AddSingleton<IResourceProviderService>(sp => 
                new PluginResourceProviderService(                   
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,                    
                    sp.GetRequiredService<ILoggerFactory>()));          

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
