using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipelineEngine.Clients;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.DataPipelineEngine.Services;
using FoundationaLLM.DataPipelineEngine.Services.CosmosDB;
using FoundationaLLM.DataPipelineEngine.Services.Queueing;
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
                    resourceProviders: sp.GetRequiredService<IEnumerable<IResourceProviderService>>(),
                    settings: sp.GetRequiredService<IOptions<DataPipelineServiceSettings>>().Value,
                    logger: sp.GetRequiredService<ILogger<DataPipelineService>>()));
        }

        /// <summary>
        /// Registers the Data Pipeline Trigger service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineTriggerService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineTriggerService();

        /// <summary>
        /// Registers the Data Pipeline Trigger service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddDataPipelineTriggerService(this IServiceCollection services) =>
            services.AddHostedService<DataPipelineTriggerService>();

        /// <summary>
        /// Registers the Data Pipeline Runner service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineRunnerService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineRunnerService();

        /// <summary>
        /// Registers the Data Pipeline Runner service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddDataPipelineRunnerService(this IServiceCollection services) =>
            services.AddHostedService<DataPipelineRunnerService>();

        /// <summary>
        /// Registers the Data Pipeline State service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineStateService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineStateService(builder.Configuration);

        /// <summary>
        /// Registers the Data Pipeline State service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataPipelineStateService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineStateServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_DataPipeline_State));

            services.AddSingleton<IAzureCosmosDBDataPipelineService, AzureCosmosDBDataPipelineService>(sp =>
                new AzureCosmosDBDataPipelineService(
                    options: Options.Create<AzureCosmosDBSettings>(
                        sp.GetRequiredService<IOptions<DataPipelineStateServiceSettings>>().Value.CosmosDB),
                    logger: sp.GetRequiredService<ILogger<AzureCosmosDBDataPipelineService>>()));
            services.ActivateSingleton<IAzureCosmosDBDataPipelineService>();

            services.AddSingleton<IDataPipelineStateService, DataPipelineStateService>(sp =>
                new DataPipelineStateService(
                    cosmosDBService: sp.GetRequiredService<IAzureCosmosDBDataPipelineService>(),
                    storageService: new BlobStorageService(
                        storageOptions: Options.Create<BlobStorageServiceSettings>(
                            sp.GetRequiredService<IOptions<DataPipelineStateServiceSettings>>().Value.Storage),
                        logger: sp.GetRequiredService<ILogger<BlobStorageService>>()),
                    logger: sp.GetRequiredService<ILogger<DataPipelineStateService>>()
                    ));
            services.ActivateSingleton<IDataPipelineStateService>();
        }

        /// <summary>
        /// Registers the Data Pipeline Frontend Worker service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineFrontendWorkerService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineFrontendWorkerService(builder.Configuration);

        /// <summary>
        /// Registers the Data Pipeline Frontend Worker service to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataPipelineFrontendWorkerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineWorkerServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_DataPipelineFrontendWorker_Configuration));

            services.AddSingleton<
                IMessageQueueService<DataPipelineRunWorkItemMessage>,
                AzureStorageQueueService<DataPipelineRunWorkItemMessage>>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<DataPipelineWorkerServiceSettings>>().Value;
                    return new AzureStorageQueueService<DataPipelineRunWorkItemMessage>(
                        settings.Storage.AccountName!,
                        settings.Queue,
                        logger: sp.GetRequiredService<ILogger<AzureStorageQueueService<DataPipelineRunWorkItemMessage>>>());
                });

            services.AddHostedService<DataPipelineWorkerService>();
        }

        /// <summary>
        /// Registers the Data Pipeline Backend Worker service to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineBackendWorkerService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineBackendWorkerService(builder.Configuration);

        /// <summary>
        /// Registers the Data Pipeline Backend Worker service to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataPipelineBackendWorkerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineWorkerServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_DataPipelineBackendWorker_Configuration));

            services.AddSingleton<
                IMessageQueueService<DataPipelineRunWorkItemMessage>,
                AzureStorageQueueService<DataPipelineRunWorkItemMessage>>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<DataPipelineWorkerServiceSettings>>().Value;
                    return new AzureStorageQueueService<DataPipelineRunWorkItemMessage>(
                        settings.Storage.AccountName!,
                        settings.Queue,
                        logger: sp.GetRequiredService<ILogger<AzureStorageQueueService<DataPipelineRunWorkItemMessage>>>());
                });

            services.AddHostedService<DataPipelineWorkerService>();
        }

        /// <summary>
        /// Registers the local data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddLocalDataPipelineServiceClient(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddLocalDataPipelineServiceClient();

        /// <summary>
        /// Registers the local data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddLocalDataPipelineServiceClient(
            this IServiceCollection services) =>
            services.AddSingleton<IDataPipelineServiceClient, LocalDataPipelineServiceClient>();

        /// <summary>
        /// Registers the null data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddNulllDataPipelineServiceClient(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddNulllDataPipelineServiceClient();

        /// <summary>
        /// Registers the null data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddNulllDataPipelineServiceClient(
            this IServiceCollection services) =>
            services.AddSingleton<IDataPipelineServiceClient, NullDataPipelineServiceClient>();
    }
}
