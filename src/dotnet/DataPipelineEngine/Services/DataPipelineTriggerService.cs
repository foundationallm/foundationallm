using Cronos;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for triggering data pipeline runs and scheduling automatic execution based on configured cron schedules.
    /// </summary>
    /// <param name="resourceValidatorFactory">The factory used to create resource validators.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineTriggerService(
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<DataPipelineTriggerService> logger) :
            DataPipelineBackgroundService(
                TimeSpan.FromSeconds(15),
                serviceProvider,
                logger),
            IDataPipelineTriggerService
    {
        protected override string ServiceName => "Data Pipeline Trigger Service";

        private readonly StandardValidator _validator = new(
            resourceValidatorFactory,
            s => new DataPipelineServiceException(s, StatusCodes.Status400BadRequest));
        private IDataPipelineRunnerService _dataPipelineRunnerService = null!;

        // Scheduling state
        private readonly ConcurrentDictionary<string, ScheduledPipelineInfo> _scheduledPipelines = new();
        private DateTimeOffset _lastPipelineRefresh = DateTimeOffset.MinValue;
        private readonly TimeSpan _pipelineRefreshInterval = TimeSpan.FromMinutes(5);

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
            _logger.LogInformation("The {ServiceName} service is executing a schedule evaluation cycle...", ServiceName);

            try
            {
                // Refresh pipeline list periodically
                if (DateTimeOffset.UtcNow - _lastPipelineRefresh > _pipelineRefreshInterval)
                {
                    await RefreshScheduledPipelinesAsync(stoppingToken);
                }

                // Evaluate schedules and trigger pipelines that are due
                await EvaluateAndTriggerSchedulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while evaluating scheduled pipelines in the {ServiceName} service.", ServiceName);
            }
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

        #region Scheduling Methods

        /// <summary>
        /// Refreshes the list of scheduled pipelines from the resource provider.
        /// </summary>
        private async Task RefreshScheduledPipelinesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Refreshing scheduled pipelines list...");

                // Wait for initialization to complete
                await _initializationTask;

                // Get all instances - in a multi-tenant system, we'd need to iterate through all instances
                // For now, we'll use a system identity to query all pipelines
                var systemIdentity = new UnifiedUserIdentity
                {
                    Name = "System",
                    Username = "system",
                    UPN = "system@foundationallm.ai"
                };

                // Query all data pipelines
                var pipelinesResult = await _dataPipelineResourceProvider.GetResourcesAsync<DataPipelineDefinition>(
                    "*", // All instances
                    systemIdentity,
                    new ResourceProviderGetOptions());

                var newScheduledPipelines = new ConcurrentDictionary<string, ScheduledPipelineInfo>();

                foreach (var pipelineResult in pipelinesResult)
                {
                    var pipeline = pipelineResult.Resource;

                    // Skip inactive pipelines
                    if (!pipeline.Active)
                        continue;

                    // Process each trigger of type Schedule
                    foreach (var trigger in pipeline.Triggers.Where(t => t.TriggerType == DataPipelineTriggerType.Schedule))
                    {
                        if (string.IsNullOrWhiteSpace(trigger.TriggerCronSchedule))
                        {
                            _logger.LogWarning(
                                "Pipeline {PipelineName} has a Schedule trigger {TriggerName} without a cron schedule. Skipping.",
                                pipeline.Name,
                                trigger.Name);
                            continue;
                        }

                        try
                        {
                            var cacheKey = $"{pipeline.Name}|{trigger.Name}";
                            
                            // Check if we already have this scheduled pipeline cached
                            if (_scheduledPipelines.TryGetValue(cacheKey, out var existingInfo))
                            {
                                // Preserve the last execution time
                                newScheduledPipelines[cacheKey] = new ScheduledPipelineInfo
                                {
                                    Pipeline = pipeline,
                                    Trigger = trigger,
                                    NextRunTime = CalculateNextRunTime(trigger.TriggerCronSchedule),
                                    LastExecutionTime = existingInfo.LastExecutionTime
                                };
                            }
                            else
                            {
                                // New scheduled pipeline
                                newScheduledPipelines[cacheKey] = new ScheduledPipelineInfo
                                {
                                    Pipeline = pipeline,
                                    Trigger = trigger,
                                    NextRunTime = CalculateNextRunTime(trigger.TriggerCronSchedule),
                                    LastExecutionTime = null
                                };
                            }

                            _logger.LogDebug(
                                "Scheduled pipeline {PipelineName} with trigger {TriggerName} (cron: {CronSchedule}). Next run: {NextRunTime}",
                                pipeline.Name,
                                trigger.Name,
                                trigger.TriggerCronSchedule,
                                newScheduledPipelines[cacheKey].NextRunTime);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to parse cron schedule '{CronSchedule}' for pipeline {PipelineName}, trigger {TriggerName}. Skipping this trigger.",
                                trigger.TriggerCronSchedule,
                                pipeline.Name,
                                trigger.Name);
                        }
                    }
                }

                // Replace the old cache with the new one
                _scheduledPipelines.Clear();
                foreach (var kvp in newScheduledPipelines)
                {
                    _scheduledPipelines[kvp.Key] = kvp.Value;
                }

                _lastPipelineRefresh = DateTimeOffset.UtcNow;
                _logger.LogInformation(
                    "Successfully refreshed {Count} scheduled pipeline triggers.",
                    _scheduledPipelines.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh scheduled pipelines list.");
            }
        }

        /// <summary>
        /// Evaluates all scheduled pipelines and triggers those that are due.
        /// </summary>
        private async Task EvaluateAndTriggerSchedulesAsync(CancellationToken cancellationToken)
        {
            var currentTime = DateTimeOffset.UtcNow;
            var triggeredCount = 0;
            var skippedCount = 0;

            foreach (var kvp in _scheduledPipelines)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var scheduledInfo = kvp.Value;

                try
                {
                    // Check if the pipeline is due to run
                    if (scheduledInfo.NextRunTime.HasValue && currentTime >= scheduledInfo.NextRunTime.Value)
                    {
                        // Check if we've already executed this within the last minute (prevent duplicates)
                        if (scheduledInfo.LastExecutionTime.HasValue &&
                            (currentTime - scheduledInfo.LastExecutionTime.Value).TotalSeconds < 60)
                        {
                            _logger.LogDebug(
                                "Skipping scheduled pipeline {PipelineName} trigger {TriggerName} - already executed at {LastExecutionTime}",
                                scheduledInfo.Pipeline.Name,
                                scheduledInfo.Trigger.Name,
                                scheduledInfo.LastExecutionTime.Value);
                            skippedCount++;
                            continue;
                        }

                        // Trigger the pipeline
                        _logger.LogInformation(
                            "Triggering scheduled pipeline {PipelineName} with trigger {TriggerName} (scheduled: {ScheduledTime})",
                            scheduledInfo.Pipeline.Name,
                            scheduledInfo.Trigger.Name,
                            scheduledInfo.NextRunTime.Value);

                        await TriggerScheduledPipelineAsync(scheduledInfo, cancellationToken);

                        // Update last execution time and calculate next run time
                        scheduledInfo.LastExecutionTime = currentTime;
                        scheduledInfo.NextRunTime = CalculateNextRunTime(scheduledInfo.Trigger.TriggerCronSchedule!);

                        _logger.LogInformation(
                            "Successfully triggered pipeline {PipelineName}. Next run: {NextRunTime}",
                            scheduledInfo.Pipeline.Name,
                            scheduledInfo.NextRunTime);

                        triggeredCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to trigger scheduled pipeline {PipelineName} with trigger {TriggerName}",
                        scheduledInfo.Pipeline.Name,
                        scheduledInfo.Trigger.Name);
                }
            }

            if (triggeredCount > 0 || skippedCount > 0)
            {
                _logger.LogInformation(
                    "Schedule evaluation complete. Triggered: {TriggeredCount}, Skipped: {SkippedCount}",
                    triggeredCount,
                    skippedCount);
            }
        }

        /// <summary>
        /// Triggers a scheduled pipeline execution.
        /// </summary>
        private async Task TriggerScheduledPipelineAsync(
            ScheduledPipelineInfo scheduledInfo,
            CancellationToken cancellationToken)
        {
            // Use system identity for scheduled executions
            var systemIdentity = new UnifiedUserIdentity
            {
                Name = "System Scheduler",
                Username = "system-scheduler",
                UPN = "system-scheduler@foundationallm.ai"
            };

            // Create a DataPipelineRun for the scheduled execution using the factory method
            var dataPipelineRun = DataPipelineRun.Create(
                dataPipelineObjectId: scheduledInfo.Pipeline.ObjectId!,
                triggerName: scheduledInfo.Trigger.Name,
                triggerParameterValues: new Dictionary<string, object>(scheduledInfo.Trigger.ParameterValues),
                upn: systemIdentity.UPN!,
                processor: "Backend"); // Scheduled pipelines always use Backend Worker

            // Create a snapshot of the pipeline definition
            var snapshot = new DataPipelineDefinitionSnapshot
            {
                Name = $"{scheduledInfo.Pipeline.Name}-snapshot-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                ObjectId = scheduledInfo.Pipeline.ObjectId,
                DataPipelineDefinition = scheduledInfo.Pipeline
            };

            // Extract instance ID from the pipeline's object ID
            // Object ID format: instances/{instanceId}/providers/...
            var instanceId = ExtractInstanceIdFromObjectId(scheduledInfo.Pipeline.ObjectId!);

            // Trigger the pipeline through the existing TriggerDataPipeline method
            await TriggerDataPipeline(
                instanceId,
                dataPipelineRun,
                snapshot,
                systemIdentity);
        }

        /// <summary>
        /// Calculates the next run time based on a cron expression.
        /// </summary>
        private DateTimeOffset? CalculateNextRunTime(string cronExpression)
        {
            try
            {
                var cronExpr = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
                var next = cronExpr.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
                return next;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse cron expression: {CronExpression}", cronExpression);
                return null;
            }
        }

        /// <summary>
        /// Extracts the instance ID from a resource object ID.
        /// </summary>
        private string ExtractInstanceIdFromObjectId(string objectId)
        {
            // Object ID format: instances/{instanceId}/providers/...
            var parts = objectId.Split('/');
            if (parts.Length >= 2 && parts[0] == "instances")
            {
                return parts[1];
            }

            _logger.LogWarning("Could not extract instance ID from object ID: {ObjectId}. Using '*' as fallback.", objectId);
            return "*";
        }

        #endregion
    }
}
