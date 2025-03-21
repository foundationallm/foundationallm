using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using FoundationaLLM.Context.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides dependency injection extensions for the FoundationaLLM Context service.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds the <see cref="IFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddFileService(this IHostApplicationBuilder builder) =>
            builder.Services.AddFileService(builder.Configuration);

        /// <summary>
        /// Adds the <see cref="IFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddFileService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ContextServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_ContextAPI_Configuration));

            services.AddScoped<IFileService, FileService>(sp =>
                new FileService(
                    azureCosmosDBFileService: sp.GetRequiredService<IAzureCosmosDBFileService>(),
                    storageService: new BlobStorageService(
                        Options.Create<BlobStorageServiceSettings>(
                            sp.GetRequiredService<IOptions<ContextServiceSettings>>().Value.FileService.Storage),
                        sp.GetRequiredService<ILogger<BlobStorageService>>()),
                    logger: sp.GetRequiredService<ILogger<FileService>>()));
        }

        /// <summary>
        /// Adds the <see cref="IAzureCosmosDBFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddAzureCosmosDBFileService(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureCosmosDBFileService(builder.Configuration);

        /// <summary>
        /// Adds the <see cref="IAzureCosmosDBFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAzureCosmosDBFileService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ContextServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_ContextAPI_Configuration));

            services.AddSingleton<IAzureCosmosDBFileService, AzureCosmosDBFileService>(sp =>
                new AzureCosmosDBFileService(
                    options: Options.Create<AzureCosmosDBSettings>(
                        sp.GetRequiredService<IOptions<ContextServiceSettings>>().Value.FileService.CosmosDB),
                    logger: sp.GetRequiredService<ILogger<AzureCosmosDBFileService>>()));
        }
    }
}
