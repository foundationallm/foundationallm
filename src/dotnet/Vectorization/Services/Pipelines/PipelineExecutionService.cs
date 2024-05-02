using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.DataSource.Models;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FoundationaLLM.Vectorization.Services.Pipelines
{
    /// <summary>
    /// Executes active vectorization data pipelines.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services..</param>
    /// <param name="resourceProviderServices">The list of resurce providers registered with the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class PipelineExecutionService(
        IServiceProvider serviceProvider,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILogger<PipelineExecutionService> logger) : IPipelineExecutionService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<PipelineExecutionService> _logger = logger;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProvider))
            {
                _logger.LogError($"Could not retrieve the {ResourceProviderNames.FoundationaLLM_Vectorization} resource provider.");
                return;
            }

            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataSource, out var dataSourceResourceProvider))
            {
                _logger.LogError($"Could not retrieve the {ResourceProviderNames.FoundationaLLM_DataSource} resource provider.");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var pipelines = await vectorizationResourceProvider.HandleGetAsync(
                        $"/{VectorizationResourceTypeNames.VectorizationPipelines}", new UnifiedUserIdentity
                        {
                            Name = "VectorizationAPI",
                            UserId = "VectorizationAPI",
                            Username = "VectorizationAPI"
                        });
                    var activePipelines = (pipelines as List<VectorizationPipeline>)!.Where(p => p.Active &&
                                (p.TriggerType == VectorizationPipelineTriggerType.Event ||
                                (p.TriggerType == VectorizationPipelineTriggerType.Schedule &&
                                CheckNextExecution(p.TriggerCronSchedule)))).ToList();

                    foreach (var activePipeline in activePipelines)
                    {
                        var dataSource = await GetResource<DataSourceBase>(
                            activePipeline.DataSourceObjectId,
                            DataSourceResourceTypeNames.DataSources,
                            dataSourceResourceProvider);
                        var textPartitioningProfile = await GetResource<VectorizationProfileBase>(
                            activePipeline.TextPartitioningProfileObjectId,
                            VectorizationResourceTypeNames.TextPartitioningProfiles,
                            vectorizationResourceProvider);
                        var textEmbeddingProfile = await GetResource<VectorizationProfileBase>(
                            activePipeline.TextEmbeddingProfileObjectId,
                            VectorizationResourceTypeNames.TextEmbeddingProfiles,
                            vectorizationResourceProvider);
                        var indexingProfile = await GetResource<VectorizationProfileBase>(
                            activePipeline.IndexingProfileObjectId,
                            VectorizationResourceTypeNames.IndexingProfiles,
                            vectorizationResourceProvider);

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var vectorizationService = scope.ServiceProvider.GetRequiredService<IVectorizationService>();

                            //TODO: Use dataSource to look for files on storage
                            //TODO: create and submit vectorization request for files found.

                            //_vectorizationService.ProcessRequest(request);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error was encountered while running the pipeline execution cycle.");
                }

                await Task.Delay(TimeSpan.FromSeconds(60));
            }
        }

        private async Task<T> GetResource<T>(string objectId, string resourceTypeName, IResourceProviderService resourceProviderService)
            where T : ResourceBase
        {
            var result = await resourceProviderService.HandleGetAsync(
                $"/{resourceTypeName}/{objectId.Split("/").Last()}", new UnifiedUserIdentity
                {
                    Name = "VectorizationAPI",
                    UserId = "VectorizationAPI",
                    Username = "VectorizationAPI"
                });
            return (result as List<T>)!.First();
        }

        private static bool CheckNextExecution(string? cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            var cronSchedule = new CronExpression(cronExpression);
            cronSchedule.TimeZone = TimeZoneInfo.Utc;

            var currentDate = DateTime.UtcNow;
            return cronSchedule.IsSatisfiedBy(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, 0));
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) =>
            await Task.CompletedTask;
    }
}
