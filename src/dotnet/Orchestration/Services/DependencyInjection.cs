using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.CodeExecution;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using FoundationaLLM.Orchestration.Core.Services;
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
        /// Adds the Orchestration service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddOrchestrationService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<OrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_OrchestrationAPI_Configuration));

            builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();
        }

        /// <summary>
        /// Adds all internal LLM orchestration services and the LLM orchestration service manager to the dependency injection container.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddLLMOrchestrationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ILLMOrchestrationServiceManager, LLMOrchestrationServiceManager>();
            builder.Services.ActivateSingleton<ILLMOrchestrationServiceManager>();

            builder.Services.AddScoped<ILLMOrchestrationService, AzureAIDirectService>();
            builder.Services.AddScoped<ILLMOrchestrationService, AzureOpenAIDirectService>();
            builder.Services.AddScoped<ILLMOrchestrationService, LangChainService>();
            builder.Services.AddScoped<ILLMOrchestrationService, SemanticKernelService>();
        }

        /// <summary>
        /// Adds the semantic cache service to the dependency injection container.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddSemanticCacheService(this IHostApplicationBuilder builder) =>
            builder.Services.AddSingleton<ISemanticCacheService, SemanticCacheService>();

        /// <summary>
        /// Adds the user prompt rewrite service to the dependency injection container.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddUserPromptRewriteService(this IHostApplicationBuilder builder) =>
            builder.Services.AddSingleton<IUserPromptRewriteService, UserPromptRewriteService>();
    }
}
