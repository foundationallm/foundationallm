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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
        protected override void SetResourceProviders(
            IEnumerable<IResourceProviderService> resourceProviders)
        {
            var hostedServices = _serviceProvider.GetRequiredService<IEnumerable<IHostedService>>();
            _dataPipelineRunnerService =
                (hostedServices.Single(hs => hs is IDataPipelineRunnerService) as IDataPipelineRunnerService)!;

            (_dataPipelineRunnerService as IResourceProviderClient)!.ResourceProviders = resourceProviders;
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
            DataPipelineDefinitionSnapshot dataPipelineSnapshot,
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

            dataPipelineRun.DataPipelineObjectId = dataPipelineSnapshot.ObjectId!;
            var dataPipeline = dataPipelineSnapshot.DataPipelineDefinition;

            // Add the default parameter values if they are not already set.
            var dataPipelineTrigger = dataPipeline.Triggers
                .SingleOrDefault(t => t.Name == dataPipelineRun.TriggerName)
                ?? throw new DataPipelineServiceException(
                    $"The data pipeline trigger with name {dataPipelineRun.TriggerName} does not exist in the data pipeline {dataPipeline.Name}.",
                    StatusCodes.Status400BadRequest);

            foreach (var item in dataPipelineTrigger.ParameterValues)
                if (!dataPipelineRun.TriggerParameterValues.ContainsKey(item.Key))
                    dataPipelineRun.TriggerParameterValues.Add(item.Key, item.Value);

            // If the canonical run id is not set, generate it based on the data pipeline run properties.
            if (string.IsNullOrWhiteSpace(dataPipelineRun.CanonicalRunId))
            {
                // Two data pipeline runs for the same data pipeline and identical relevant trigger parameter values
                // should have the same canonical run id.
                var canonicalRunIdRaw = $"{dataPipeline.Name}|"
                    + JsonSerializer.Serialize(GetParameterValuesForCanonicalId(
                        dataPipelineRun,
                        dataPipelineTrigger));

                dataPipelineRun.CanonicalRunId = Convert.ToBase64String(
                        MD5.HashData(Encoding.UTF8.GetBytes(canonicalRunIdRaw)))
                        .Replace("+", "--")
                        .Replace("/", "--");
            }

            await _validator.ValidateAndThrowAsync<DataPipelineRun>(dataPipelineRun);

            // Before starting the identification of content items,
            // make a preliminary check to see if the run can be started.
            // This is to avoid unnecessary work if the run cannot be started.
            if (!await _dataPipelineRunnerService.CanStartRun(dataPipelineRun))
            {
                _logger.LogError(
                    "The data pipeline with run id {DataPipelineRunId} and canonical run id {DataPipelineCanonicalRunId} is conflicting with an already existing run and cannot be started.",
                    dataPipelineRun.RunId,
                    dataPipelineRun.CanonicalRunId);
                throw new DataPipelineServiceException(
                    $"The data pipeline with run id {dataPipelineRun.RunId} and canonical run id {dataPipelineRun.CanonicalRunId} is conflicting with an already existing run and cannot be started.",
                    StatusCodes.Status400BadRequest);
            }

            var dataSourcePlugin = await _pluginService.GetDataSourcePlugin(
                instanceId,
                dataPipeline.DataSource.PluginObjectId,
                dataPipelineRun.TriggerParameterValues.FilterKeys(
                    $"DataSource.{dataPipeline.DataSource.Name}."),
                dataPipeline.DataSource.DataSourceObjectId,
                userIdentity);

            var contentItems = await dataSourcePlugin.GetContentItems();

            var updatedDataPipelineRun = await _dataPipelineRunnerService.StartRun(
                dataPipelineRun,
                contentItems,
                dataPipeline,
                userIdentity);

            return updatedDataPipelineRun!;
        }

        private Dictionary<string, object> GetParameterValuesForCanonicalId(
            DataPipelineRun dataPipelineRun,
            DataPipelineTrigger dataPipelineTrigger)
        {
            if (dataPipelineTrigger.CanonicalIdParameters == null
                || dataPipelineTrigger.CanonicalIdParameters.Count == 0)
            {
                _logger.LogWarning(
                    "The data pipeline trigger {DataPipelineTriggerName} does not have any canonical id parameters defined. "
                    + "All the trigger parameters will be used to generate the canonical run id for the data pipeline run {DataPipelineRunId}. "
                    + "It is recommended to define a subset of parameters that uniquely identify the run to avoid unnecessary conflicts.",
                    dataPipelineTrigger.Name,
                    dataPipelineRun.RunId);
                return dataPipelineRun.TriggerParameterValues;
            }

            _logger.LogInformation(
                "The data pipeline trigger {DataPipelineTriggerName} has the following canonical id parameters defined: {CanonicalIdParameters}. "
                + "Only these parameters will be used to generate the canonical run id for the data pipeline run {DataPipelineRunId}.",
                dataPipelineTrigger.Name,
                string.Join(", ", dataPipelineTrigger.CanonicalIdParameters),
                dataPipelineRun.RunId);

            return
                dataPipelineTrigger.CanonicalIdParameters
                    .ToDictionary(
                        p => p,
                        p => dataPipelineRun.TriggerParameterValues[p]);
        }
    }
}
