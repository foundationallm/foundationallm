using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
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
        ILogger<DataPipelineTriggerService> logger) :
            DataPipelineBackgroundService(
                TimeSpan.FromMinutes(1),
                serviceProvider,
                logger),
            IDataPipelineTriggerService
    {
        protected override string ServiceName => "Data Pipeline Trigger Service";

        private readonly StandardValidator _validator = new(
            resourceValidatorFactory,
            s => new DataPipelineServiceException(s, StatusCodes.Status400BadRequest));
        private IDataPipelineRunnerService _dataPipelineRunnerService = null!;

        #region Initialization

        /// <inheritdoc/>
        protected override void EnsureDependencyInjectionResolutionInternal(
            CancellationToken stoppingToken)
        {
            var hostedServices = _serviceProvider.GetRequiredService<IEnumerable<IHostedService>>();
            _dataPipelineRunnerService =
                (hostedServices.Single(hs => hs is IDataPipelineRunnerService) as IDataPipelineRunnerService)!;

            _dataPipelineRunnerService.ResourceProviders = [
                _dataPipelineResourceProvider,
                _pluginResourceProvider
            ];
        }

        #endregion

        protected override async Task ExecuteAsyncInternal(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The {ServiceName} service is executing...", ServiceName);
            await Task.CompletedTask;
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

            var dataSourcePlugin = await GetDataSourcePlugin(
                instanceId,
                dataPipeline.DataSource.PluginObjectId,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"DataSource.{dataPipeline.DataSource.Name}."),
                dataPipeline.DataSource.DataSourceObjectId,
                userIdentity);

            var contentItems = dataSourcePlugin.GetContentItems();

            var updatedDataPipelineRun = await _dataPipelineRunnerService.StartRun(
                dataPipelineRun,
                contentItems,
                dataPipeline,
                userIdentity);

            return updatedDataPipelineRun!;
        }
    }
}
