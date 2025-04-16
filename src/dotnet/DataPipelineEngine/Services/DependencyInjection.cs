using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.DataPipelineEngine.Services;
using FoundationaLLM.DataPipelineEngine.Services.CosmosDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides dependency injection extensions for the FoundationaLLM Data Pipeline service.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the <see cref="IDataPipelineService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="IDataPipelineService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataPipelineService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_DataPipelineAPI_Configuration));

            services.AddScoped<IDataPipelineService, DataPipelineService>(sp =>
                new DataPipelineService(
                    cosmosDBService: sp.GetRequiredService<IAzureCosmosDBDataPipelineService>(),
                    settings: sp.GetRequiredService<IOptions<DataPipelineServiceSettings>>().Value,
                    resourceValidatorFactory: sp.GetRequiredService<IResourceValidatorFactory>(),
                    logger: sp.GetRequiredService<ILogger<DataPipelineService>>()));
        }

        /// <summary>
        /// Registers the Azure Cosmos DB service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddAzureCosmosDBDataPipelineService(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureCosmosDBDataPipelineService(builder.Configuration);

        /// <summary>
        /// Registers the Azure Cosmos DB service used by the DataPipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAzureCosmosDBDataPipelineService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_DataPipelineAPI_Configuration));

            services.AddSingleton<IAzureCosmosDBDataPipelineService, AzureCosmosDBDataPipelineService>(sp =>
                new AzureCosmosDBDataPipelineService(
                    options: Options.Create<AzureCosmosDBSettings>(
                        sp.GetRequiredService<IOptions<DataPipelineServiceSettings>>().Value.CosmosDB),
                    logger: sp.GetRequiredService<ILogger<AzureCosmosDBDataPipelineService>>()));
            services.ActivateSingleton<IAzureCosmosDBDataPipelineService>();
        }
    }
}
