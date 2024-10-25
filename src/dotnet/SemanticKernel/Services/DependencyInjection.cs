using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Tasks;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Semantic Kernel service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddSemanticKernelService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_SemanticKernelAPI_Configuration));

            builder.Services.AddScoped<ISemanticKernelService, SemanticKernelService>();

            builder.Services.AddSingleton<ConcurrentTaskPool, ConcurrentTaskPool>(sp =>
                new ConcurrentTaskPool(
                    sp.GetRequiredService<IOptions<SemanticKernelServiceSettings>>().Value.MaxConcurrentCompletions,
                    sp.GetRequiredService<ILogger<ConcurrentTaskPool>>()));
            builder.Services.ActivateSingleton<ConcurrentTaskPool>();
        }
    }
}
