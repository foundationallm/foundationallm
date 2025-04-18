using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for data pipeline triggering.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="resourceValidatorFactory">The factory used to create resource validators.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineTriggerService(
        IAzureCosmosDBDataPipelineService cosmosDBService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<DataPipelineTriggerService> logger) : BackgroundService, IDataPipelineTriggerService
    {
        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;
        private readonly StandardValidator _validator = new(
            resourceValidatorFactory,
            s => new DataPipelineServiceException(s, StatusCodes.Status400BadRequest));
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<DataPipelineTriggerService> _logger = logger;

        private IResourceProviderService _dataPipelineResourceProvider = null!;
        private IResourceProviderService _pluginResourceProvider = null!;

        /// <inheritdoc/>
        public IEnumerable<IResourceProviderService> ResourceProviders
        {
            set
            {
                _dataPipelineResourceProvider = value
                    .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);

                _pluginResourceProvider = value
                    .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
            }
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The Data Pipeline Trigger service is starting...");

            // Wait until the Data Pipeline Resource Provider is set and then
            // wait for its initialization task to complete.
            while (_dataPipelineResourceProvider == null)
            {
                _logger.LogWarning("The Data Pipeline Resource Provider is not set. Waiting for it to be set...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The Data Pipeline Trigger service is stopping.");
                    return;
                }
            }

            _logger.LogInformation("The Data Pipeline Resource Provider is set. Waiting for its initialization task to complete...");
            await _dataPipelineResourceProvider.InitializationTask;
            _logger.LogInformation("The Data Pipeline Resource Provider is initialized.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("The Data Pipeline Trigger service is running.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("The Data Pipeline Trigger service is stopping.");
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> TriggerDataPipeline(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            UnifiedUserIdentity userIdentity)
        {
            var newDataPipelineRunId = $"run-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}-{Guid.Parse(instanceId).ToBase64String()}";
            dataPipelineRun.Id = newDataPipelineRunId;
            dataPipelineRun.Name = newDataPipelineRunId;
            dataPipelineRun.ObjectId = ResourcePath.Join(
                dataPipelineRun.DataPipelineObjectId,
                $"dataPipelineRuns/{newDataPipelineRunId}");
            dataPipelineRun.InstanceId = instanceId;
            dataPipelineRun.TriggeringUPN = userIdentity.UPN!;
            dataPipelineRun.CreatedOn = DateTimeOffset.UtcNow;
            dataPipelineRun.CreatedBy = userIdentity.UPN!;

            await _validator.ValidateAndThrowAsync<DataPipelineRun>(dataPipelineRun);

            var dataPipeline = await _dataPipelineResourceProvider.GetResourceAsync<DataPipelineDefinition>(
                dataPipelineRun.DataPipelineObjectId,
                userIdentity);

            var dataSourcePluginDefinition = await _pluginResourceProvider.GetResourceAsync<PluginDefinition>(
                dataPipeline.DataSource.PluginObjectId,
                userIdentity);

            var pluginPackage = await _pluginResourceProvider.ExecuteResourceActionAsync<PluginPackageDefinition, object, ResourceProviderActionResult<PluginPackageManagerInstance>>(
                instanceId,
                ResourcePath.GetResourcePath(dataSourcePluginDefinition.PluginPackageObjectId).MainResourceId!,
                ResourceProviderActions.LoadPluginPackage,
                null!,
                userIdentity);
            var packageManager = pluginPackage.Resource!.Instance;

            var dataSourcePlugin = packageManager.GetDataSourcePlugin(
                dataSourcePluginDefinition.Name,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"DataSource.{dataPipeline.DataSource.Name}."),
                _serviceProvider);

            var contentItems = dataSourcePlugin.GetContentItems();

            var updatedDataPipelineRun = await _cosmosDBService.UpsertItemAsync<DataPipelineRun>(
                dataPipelineRun.RunId,
                dataPipelineRun);

            return updatedDataPipelineRun;
        }
    }
}
