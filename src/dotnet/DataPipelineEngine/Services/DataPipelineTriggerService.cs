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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for triggering data pipeline runs.
    /// </summary>
    /// <param name="resourceValidatorFactory">The factory used to create resource validators.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineTriggerService(
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<DataPipelineTriggerService> logger) : BackgroundService, IDataPipelineTriggerService
    {
        private readonly StandardValidator _validator = new(
            resourceValidatorFactory,
            s => new DataPipelineServiceException(s, StatusCodes.Status400BadRequest));
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<DataPipelineTriggerService> _logger = logger;

        private Task _initializationTask = null!;

        private IResourceProviderService _dataPipelineResourceProvider = null!;
        private IResourceProviderService _pluginResourceProvider = null!;
        private IDataPipelineRunnerService _dataPipelineRunnerService = null!;

        #region Initialization

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

        private async Task EnsureDependencyInjectionResolution(
            CancellationToken stoppingToken)
        {
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

            var hostedServices = _serviceProvider.GetRequiredService<IEnumerable<IHostedService>>();
            _dataPipelineRunnerService =
                (hostedServices.Single(hs => hs is IDataPipelineRunnerService) as IDataPipelineRunnerService)!;

            _dataPipelineRunnerService.ResourceProviders = [
                _dataPipelineResourceProvider,
                _pluginResourceProvider
            ];
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The Data Pipeline Trigger service is starting...");

            // Perform dependency injection resolution and wait for the initialization task to complete.
            // This is necessary to ensure that all dependencies are resolved before the service starts and
            // cirucular references are avoided.
            _initializationTask = EnsureDependencyInjectionResolution(stoppingToken);
            await _initializationTask;

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
            // Wait for the initialization task to complete before proceeding.
            // At this point this task should be completed and the await should have no performance impact.
            // This is necessary to avoid any issues during the initialization of the service.
            await _initializationTask;

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

            var dataPipelineSnapshot = await _dataPipelineResourceProvider.GetResourceAsync<DataPipelineDefinitionSnapshot>(
                $"{dataPipelineRun.DataPipelineObjectId}/{DataPipelineResourceTypeNames.DataPipelineSnapshots}/latest",
                userIdentity);
            dataPipelineRun.DataPipelineObjectId = dataPipelineSnapshot.ObjectId!;
            var dataPipeline = dataPipelineSnapshot.DataPipelineDefinition;

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
                dataPipeline.DataSource.DataSourceObjectId,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"DataSource.{dataPipeline.DataSource.Name}."),
                _serviceProvider);

            var contentItems = dataSourcePlugin.GetContentItems();

            var updatedDataPipelineRun = await _dataPipelineRunnerService.StartRun(
                dataPipelineRun,
                contentItems,
                dataPipeline);

            return updatedDataPipelineRun;
        }
    }
}
