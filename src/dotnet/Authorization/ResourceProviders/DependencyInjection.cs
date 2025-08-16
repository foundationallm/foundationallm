using FluentValidation;
using FoundationaLLM.Authorization.ResourceProviders;
using FoundationaLLM.Authorization.Validation;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
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
        /// Register the FoundationaLLM.Authorization resource provider with the dependency injection container.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddAuthorizationResourceProvider(
            this IHostApplicationBuilder builder,
            bool proxyMode = false)
        {
            builder.Services.AddSingleton<IValidator<RoleAssignment>, RoleAssignmentValidator>();

            builder.Services.AddSingleton<IResourceProviderService, AuthorizationResourceProviderService>(sp =>
                new AuthorizationResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    Options.Create<ResourceProviderCacheSettings>(new ResourceProviderCacheSettings
                    {
                        EnableCache = false
                    }),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>(),
                    proxyMode: proxyMode));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
