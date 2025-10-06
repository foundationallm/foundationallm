using FluentValidation;
using FoundationaLLM.Agent.ResourceProviders;
using FoundationaLLM.Agent.Validation.Metadata;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Services.ResourceProviders.Agent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Agent resource provider service implementation of resource provider dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Agent resource provider and its related services the the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddAgentResourceProvider(
            this IHostApplicationBuilder builder,
            bool proxyMode = false) =>
            builder.Services.AddAgentResourceProvider(
                builder.Configuration,
                proxyMode);

        /// <summary>
        /// Registers the FoundationaLLM.Agent resource provider with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
        public static void AddAgentResourceProvider(
            this IServiceCollection services,
            IConfiguration configuration,
            bool proxyMode = false)
        {
            services.AddSingleton<IAgentTemplateService, AgentTemplateService>();
            services.ActivateSingleton<IAgentTemplateService>();

            services.AddAgentResourceProviderStorage(configuration);

            // Register validators.
            services.AddSingleton<IValidator<AgentBase>, AgentBaseValidator>();
            services.AddSingleton<IValidator<KnowledgeManagementAgent>, KnowledgeManagementAgentValidator>();
            services.AddSingleton<IValidator<Workflow>, WorkflowValidator>();
            services.AddSingleton<IValidator<Tool>, ToolValidator>();

            services.AddSingleton<IResourceProviderService, AgentResourceProviderService>(sp =>
                new AgentResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAgentTemplateService>(),
                    sp.GetRequiredService<IAzureCosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>(),
                    proxyMode: proxyMode));
            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
