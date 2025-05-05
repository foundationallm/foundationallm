using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Extensions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Services.DataSources;
using FoundationaLLM.Vectorization.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using FoundationaLLM.Vectorization.Services.DataSources.Configuration.SQLDatabase;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using Microsoft.Extensions.Options;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Extensions;

namespace FoundationaLLM.Vectorization.Services.Pipelines
{
    /// <summary>
    /// Executes active vectorization data pipelines.
    /// </summary>
    /// <param name="instanceOptions">The <see cref="IOptions{TOptions}"/> value providing <see cref="InstanceSettings"/> settings.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services..</param>
    /// <param name="resourceProviderServices">The list of resurce providers registered with the main dependency injection container.</param>
    /// <param name="loggerFactory">Factory responsible for creating loggers.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class PipelineExecutionService(
        IOptions<InstanceSettings> instanceOptions,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILoggerFactory loggerFactory,
        ILogger<PipelineExecutionService> logger) : IPipelineExecutionService
    {
        private readonly InstanceSettings _instanceSettings = instanceOptions.Value;
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<PipelineExecutionService> _logger = logger;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService))
            {
                _logger.LogError($"Could not retrieve the {ResourceProviderNames.FoundationaLLM_Vectorization} resource provider.");
                return;
            }
            //cast for extension methods
            var vectorizationResourceProvider = (VectorizationResourceProviderService)vectorizationResourceProviderService;

            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataSource, out var dataSourceResourceProvider))
            {
                _logger.LogError($"Could not retrieve the {ResourceProviderNames.FoundationaLLM_DataSource} resource provider.");
                return;
            }

            var stateService = _serviceProvider.GetRequiredService<IVectorizationStateService>();
            if (stateService is null)
            {
                _logger.LogError("Could not retrieve the vectorization state service.");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if(!vectorizationResourceProvider.IsInitialized)
                    {
                        _logger.LogInformation("Vectorization resource provider has not finished initializing.");
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                        continue;
                    }
                    var activePipelines = await vectorizationResourceProvider.GetActivePipelines();

                    foreach (var activePipeline in activePipelines)
                    {
                        //deactivate the pipeline being processed.
                        await vectorizationResourceProvider.TogglePipelineActivation(
                            activePipeline.ObjectId!,
                            false,
                            ServiceContext.ServiceIdentity!);

                        // initialize pipeline execution state
                        var refTime = DateTimeOffset.UtcNow;
                        var pipelineName = activePipeline.Name;
                        var pipelineExecutionId =
                            $"{pipelineName}-{refTime:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
                        
                        var pipelineExecution = new VectorizationPipelineExecution
                        {
                            Name = pipelineExecutionId,
                            PipelineObjectId = activePipeline.ObjectId!,
                            ExecutionStart = refTime
                        };

                        await stateService.SavePipelineState(pipelineExecution, null);

                        activePipeline.LatestExecutionId = pipelineExecution.Name;
                        await vectorizationResourceProvider.UpsertResourceAsync<VectorizationPipeline, ResourceProviderUpsertResult<VectorizationPipeline>>(
                            _instanceSettings.Id,
                            activePipeline,
                            ServiceContext.ServiceIdentity!);

                        var pipelineExecutionDetail = new VectorizationPipelineExecutionDetail
                        {
                            PipelineObjectId = activePipeline.ObjectId!,
                            ExecutionId = pipelineExecutionId
                        };

                        try
                        {
                            _logger.LogInformation("Executing pipeline {PipelineName} with execution identifier {PipelineExecutionId}.",
                                pipelineName, pipelineExecutionId);

                            var dataSource = await GetResource<DataSourceBase>(
                                activePipeline.DataSourceObjectId,
                                DataSourceResourceTypeNames.DataSources,
                                dataSourceResourceProvider,
                                ServiceContext.ServiceIdentity!);
                            var textPartitioningProfile = await GetResource<TextPartitioningProfile>(
                                activePipeline.TextPartitioningProfileObjectId,
                                VectorizationResourceTypeNames.TextPartitioningProfiles,
                                vectorizationResourceProvider,
                                ServiceContext.ServiceIdentity!);
                            var textEmbeddingProfile = await GetResource<TextEmbeddingProfile>(
                                activePipeline.TextEmbeddingProfileObjectId,
                                VectorizationResourceTypeNames.TextEmbeddingProfiles,
                                vectorizationResourceProvider,
                                ServiceContext.ServiceIdentity!);
                            var indexingProfile = await GetResource<IndexingProfile>(
                                activePipeline.IndexingProfileObjectId,
                                VectorizationResourceTypeNames.IndexingProfiles,
                                vectorizationResourceProvider,
                                ServiceContext.ServiceIdentity!);

                            if (dataSource is null)
                            {
                                continue;
                            }
                            switch (dataSource.Type)
                            {
                                case DataSourceTypes.AzureDataLake:
                                    // resolve configuration references
                                    var blobStorageServiceSettings = new BlobStorageServiceSettings { AuthenticationType = AuthenticationTypes.Unknown };
                                    _configuration.Bind(
                                        $"{AppConfigurationKeySections.FoundationaLLM_DataSources}:{dataSource.Name}",
                                        blobStorageServiceSettings);

                                    AzureDataLakeDataSourceService svc = new AzureDataLakeDataSourceService(
                                                                        (AzureDataLakeDataSource)dataSource!,
                                                                        blobStorageServiceSettings,
                                                                        _loggerFactory);

                                    if (string.IsNullOrEmpty(blobStorageServiceSettings.AccountName))
                                    {
                                        // extract the account from the connection string
                                        var accountName = blobStorageServiceSettings.ConnectionString!.Split(';')
                                            .FirstOrDefault(s => s.StartsWith("AccountName="))?.Split('=')[1];
                                        blobStorageServiceSettings.AccountName = accountName;
                                    }

                                    var files = await svc.GetFilesListAsync();
                                    var firstMultipartToken = $"{blobStorageServiceSettings.AccountName}.dfs.core.windows.net";
                                    if (blobStorageServiceSettings.AccountName!.Equals("onelake"))
                                    {
                                        firstMultipartToken = $"{blobStorageServiceSettings.AccountName}.dfs.fabric.microsoft.com";
                                    }

                                    var dataSourceFileCount = 0;
                                    foreach (var file in files)
                                    {
                                        dataSourceFileCount++;

                                        //first token is the container name
                                        var containerName = file.Split("/")[0];
                                        //remove the first token from the path
                                        var path = file.Substring(file.IndexOf('/') + 1);
                                        //path minus the file extension
                                        var canonical = path.Substring(0, path.LastIndexOf('.'));
                                        var vectorizationRequest = new VectorizationRequest()
                                        {
                                            Name = $"{pipelineExecution.ExecutionStart:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}",
                                            PipelineExecutionId = pipelineExecutionId,
                                            PipelineExecutionStart = pipelineExecution.ExecutionStart,
                                            PipelineObjectId = activePipeline.ObjectId!,
                                            PipelineName = activePipeline.Name,
                                            CostCenter = activePipeline.CostCenter,
                                            ContentIdentifier = new ContentIdentifier()
                                            {
                                                DataSourceObjectId = dataSource.ObjectId!,
                                                MultipartId = new List<string> { firstMultipartToken, containerName, path },
                                                CanonicalId = canonical
                                            },
                                            ProcessingType = VectorizationProcessingType.Asynchronous,
                                            ProcessingState = VectorizationProcessingState.New,
                                            Steps =
                                            [
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Extract,
                                                    Parameters = new Dictionary<string, string>()
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Partition,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_partitioning_profile_name", textPartitioningProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Embed,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_embedding_profile_name", textEmbeddingProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Index,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"indexing_profile_name", indexingProfile.Name }
                                                    }
                                                }
                                            ],
                                            CompletedSteps = [],
                                            RemainingSteps = ["extract", "partition", "embed", "index"]
                                        };

                                        // submit the vectorization request, if an error occurs on a single file, record it and continue with the next file.
                                        // this does not result in the failure of the entire pipeline.
                                        try
                                        {
                                            pipelineExecution.VectorizationRequestCount++;

                                            //create the vectorization request
                                            await vectorizationRequest.UpdateVectorizationRequestResource(
                                                _instanceSettings.Id,
                                                vectorizationResourceProvider,
                                                ServiceContext.ServiceIdentity!);
                                            pipelineExecutionDetail.VectorizationRequestObjectIds.Add(vectorizationRequest.ObjectId!);
                                            //issue process action on the created vectorization request
                                            await vectorizationRequest.ProcessVectorizationRequest(vectorizationResourceProvider);
                                        }
                                        catch (Exception ex)
                                        {
                                            var errorMessage = $"An error was encountered while creating the vectorization request for file: {string.Join('/', vectorizationRequest.ContentIdentifier.MultipartId)}, exception: {ex.Message}";
                                            _logger.LogError(ex, errorMessage);

                                            pipelineExecution.VectorizationRequestFailuresCount++;
                                            pipelineExecution.ErrorMessages.Add(errorMessage);
                                            
                                        }

                                        if (dataSourceFileCount % 100 == 0)
                                            await stateService.SavePipelineState(pipelineExecution, pipelineExecutionDetail);
                                    }

                                    if (dataSourceFileCount % 100 != 0)
                                        await stateService.SavePipelineState(pipelineExecution, pipelineExecutionDetail);

                                    break;

                                case DataSourceTypes.AzureSQLDatabase:

                                    var sqlDataSourceServiceSettings = new SQLDatabaseServiceSettings { ConnectionString = String.Empty };
                                    _configuration.Bind(
                                        $"{AppConfigurationKeySections.FoundationaLLM_DataSources}:{dataSource.Name}",
                                        sqlDataSourceServiceSettings);
                                    AzureSQLDatabaseDataSourceService sqlSvc = new AzureSQLDatabaseDataSourceService(
                                        (AzureSQLDatabaseDataSource)dataSource!,
                                        sqlDataSourceServiceSettings,
                                        _loggerFactory);
                                    List<List<string>> multipartIds = new List<List<string>>();
                                    if (!String.IsNullOrWhiteSpace(sqlDataSourceServiceSettings.MultiPartQuery))
                                    {
                                        var delimitedMultipartIds = await sqlSvc.ExecuteMultipartQueryAsync(cancellationToken);
                                        foreach (var delimitedMultipartId in delimitedMultipartIds)
                                        {
                                            multipartIds.Add(delimitedMultipartId.Split('|').ToList());
                                        }
                                    }

                                    var sqlFileCount = 0;
                                    foreach (var multipartId in multipartIds)
                                    {
                                        sqlFileCount++;

                                        var canonical = $"{dataSource.Name}/{string.Join('/', multipartId)}";
                                        var vectorizationRequest = new VectorizationRequest()
                                        {
                                            Name = Guid.NewGuid().ToString(),
                                            PipelineExecutionId = pipelineExecutionId,
                                            PipelineExecutionStart = pipelineExecution.ExecutionStart,
                                            PipelineObjectId = activePipeline.ObjectId!,
                                            PipelineName = activePipeline.Name,
                                            CostCenter = activePipeline.CostCenter,
                                            ContentIdentifier = new ContentIdentifier()
                                            {
                                                DataSourceObjectId = dataSource.ObjectId!,
                                                MultipartId = multipartId,
                                                CanonicalId = canonical
                                            },
                                            ProcessingType = VectorizationProcessingType.Asynchronous,
                                            ProcessingState = VectorizationProcessingState.New,
                                            Steps =
                                            [
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Extract,
                                                    Parameters = new Dictionary<string, string>()
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Partition,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_partitioning_profile_name", textPartitioningProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Embed,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_embedding_profile_name", textEmbeddingProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Index,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"indexing_profile_name", indexingProfile.Name }
                                                    }
                                                }
                                            ],
                                            CompletedSteps = [],
                                            RemainingSteps = ["extract", "partition", "embed", "index"]
                                        };

                                        // submit the vectorization request, if an error occurs on a single file, record it and continue with the next file.
                                        // this does not result in the failure of the entire pipeline.
                                        try
                                        {
                                            pipelineExecution.VectorizationRequestCount++;

                                            //create the vectorization request
                                            await vectorizationRequest.UpdateVectorizationRequestResource(
                                                _instanceSettings.Id,
                                                vectorizationResourceProvider,
                                                ServiceContext.ServiceIdentity!);
                                            pipelineExecutionDetail.VectorizationRequestObjectIds.Add(vectorizationRequest.ObjectId!);

                                            //issue process action on the created vectorization request
                                            var processResult = await vectorizationRequest.ProcessVectorizationRequest(vectorizationResourceProvider);
                                            if(processResult.IsSuccess==false)
                                            {
                                                vectorizationRequest.ProcessingState = VectorizationProcessingState.Failed;
                                                pipelineExecution.ErrorMessages.Add($"Error while submitting process action on vectorization request {vectorizationRequest.Name} in pipeline {pipelineName}: {processResult.ErrorMessage!}");
                                            }
                                            await vectorizationRequest.UpdateVectorizationRequestResource(
                                                _instanceSettings.Id,
                                                vectorizationResourceProvider,
                                                ServiceContext.ServiceIdentity!);
                                        }
                                        catch (Exception ex)
                                        {
                                            var errorMessage = $"An error was encountered while creating the vectorization request for file {string.Join('/', vectorizationRequest.ContentIdentifier.MultipartId)}, exception: {ex.Message}";
                                            _logger.LogError(ex, errorMessage);

                                            pipelineExecution.VectorizationRequestFailuresCount++;
                                            pipelineExecution.ErrorMessages.Add(errorMessage);                                           
                                        }

                                        if (sqlFileCount % 100 == 0)
                                            await stateService.SavePipelineState(pipelineExecution, pipelineExecutionDetail);
                                    }

                                    if (sqlFileCount % 100 != 0)
                                        await stateService.SavePipelineState(pipelineExecution, pipelineExecutionDetail);

                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error was encountered while activating or executing pipeline {PipelineName}.",
                                activePipeline.Name);

                            pipelineExecution.ErrorMessages.Add($"An error was encountered while activating or executing pipeline {activePipeline.Name}: {ex.Message}.");                           
                        }
                        finally
                        {
                            if(pipelineExecutionDetail.VectorizationRequestObjectIds.Count == 0)
                            {
                                pipelineExecution.ExecutionEnd = DateTimeOffset.UtcNow;
                            }                           

                            await stateService.SavePipelineState(pipelineExecution, pipelineExecutionDetail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error was encountered while running the pipeline execution cycle.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }

        /// <summary>
        /// Retrieves a resource of the specified type from the resource provider service.
        /// </summary>
        /// <typeparam name="T">Type of the resource to retrieve.</typeparam>
        /// <param name="objectId">The object id/resource path of the resource to retrieve.</param>
        /// <param name="resourceTypeName">The type of resource.</param>
        /// <param name="resourceProviderService">The resource provider service.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>The requested resource object.</returns>
        private static async Task<T> GetResource<T>(string objectId, string resourceTypeName, IResourceProviderService resourceProviderService, UnifiedUserIdentity userIdentity)
            where T : ResourceBase =>
          await resourceProviderService.GetResourceAsync<T>($"/{resourceTypeName}/{objectId.Split("/").Last()}", userIdentity);

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) =>
            await Task.CompletedTask;
    }
}
