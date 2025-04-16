using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation.Plugins;
using FoundationaLLM.DataPipeline.Clients;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipeline.ResourceProviders;
using FoundationaLLM.DataPipeline.Validation;
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
        /// Register the FoundationaLLM DataPipeline resource provider with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="remoteClient">The flag the controls which Data Pipeline API client should the resource provider use.
        /// Defaults to <c>true</c> which means the remote client will be used (HTTP calls to the Data Pipeline API).</param>
        public static async Task AddDataPipelineResourceProvider(
            this IHostApplicationBuilder builder,
            bool remoteClient = true)
        {
            builder.AddDataPipelineResourceProviderStorage();

            // Register validators.
            builder.Services.AddSingleton<IValidator<DataPipelineDefinition>, DataPipelineDefinitionValidator>();
            builder.Services.AddSingleton<IValidator<DataPipelineRun>, DataPipelineRunValidator>();
            builder.Services.AddSingleton<IValidator<PluginComponent>, PluginComponentValidator>();

            IDataPipelineServiceClient dataPipelineServiceClient = remoteClient
                ? await RemoteDataPipelineServiceClient.CreateAsync(
                    builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactoryService>())
                : new LocalDataPipelineServiceClient();

            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            builder.Services.AddSingleton<IResourceProviderService>(sp => 
                new DataPipelineResourceProviderService(                   
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    dataPipelineServiceClient,
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,                    
                    sp.GetRequiredService<ILoggerFactory>()));          

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
