using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Parquet.Serialization;
using System.IO;
using System.Text.Json;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides capabilities for data pipeline state management.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The storage service providing blob storage capabilities.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineStateService(
        IAzureCosmosDBDataPipelineService cosmosDBService,
        IStorageService storageService,
        ILogger<DataPipelineStateService> logger) : IDataPipelineStateService
    {
        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;
        private readonly IStorageService _storageService = storageService;
        private readonly ILogger<DataPipelineStateService> _logger = logger;

        /// <inheritdoc/>
        public async Task<bool> InitializeDataPipelineRunState(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems)
        {
            #region Save content items list to run root

            var runContentItemsRegistryPath = string.Join('/',
                [
                    GetDataPipelineRunRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    "content-items",
                    "content-items.json"
                ]);

            var runContentItemsRegistry = new ContentItemsRegistry
            {
                ToAddOrUpdate = new SortedDictionary<string, ContentItemsRegistryEntry>(
                    contentItems
                    .Where(ci => ci.ContentAction == ContentItemActions.AddOrUpdate)
                    .Select(ci => new ContentItemsRegistryEntry
                    {
                        ContentItemCanonicalId = ci.ContentIdentifier.CanonicalId,
                        LastContentAction = ci.ContentAction,
                        LastModifiedAt = DateTimeOffset.UtcNow
                    })
                    .ToDictionary(
                        entry => entry.ContentItemCanonicalId,
                        entry => entry)),
                ToRemove = new SortedDictionary<string, ContentItemsRegistryEntry>(
                    contentItems
                    .Where(ci => ci.ContentAction == ContentItemActions.Remove)
                    .Select(ci => new ContentItemsRegistryEntry
                    {
                        ContentItemCanonicalId = ci.ContentIdentifier.CanonicalId,
                        LastContentAction = ci.ContentAction,
                        LastModifiedAt = DateTimeOffset.UtcNow
                    })
                    .ToDictionary(
                        entry => entry.ContentItemCanonicalId,
                        entry => entry))
            };
            var runContentItemsRegistryContent = JsonSerializer.Serialize(
                runContentItemsRegistry);

            await _storageService.WriteFileAsync(
                dataPipelineRun.InstanceId,
                runContentItemsRegistryPath,
                runContentItemsRegistryContent,
                "application/json",
                default);

            _logger.LogInformation("Initialized data pipeline run {DataPipelineRunId} state with {ContentItemsCount} content items.",
                dataPipelineRun.RunId,
                contentItems.Count);

            #endregion

            #region Update content items list in the canonical root

            var canonicalContentItemsRegistryPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    "content-items",
                    "content-items.json"
                ]);

            string updatedCanonicalContentItemsRegistryContent = string.Empty;

            if (await _storageService.FileExistsAsync(
                dataPipelineRun.InstanceId,
                canonicalContentItemsRegistryPath,
                default))
            {
                var canonicalContentItemsRegistryRawContent = await _storageService.ReadFileAsync(
                    dataPipelineRun.InstanceId,
                    canonicalContentItemsRegistryPath,
                    default);
                var canonicalContentItemsRegistry = JsonSerializer.Deserialize<ContentItemsRegistry>(
                    canonicalContentItemsRegistryRawContent.ToStream())!;

                foreach (var runAddOrUpdateEntry in runContentItemsRegistry.ToAddOrUpdate.Values)
                {
                    if (canonicalContentItemsRegistry.ToRemove.ContainsKey(
                        runAddOrUpdateEntry.ContentItemCanonicalId))
                        canonicalContentItemsRegistry.ToRemove.Remove(
                            runAddOrUpdateEntry.ContentItemCanonicalId);
                    canonicalContentItemsRegistry.ToAddOrUpdate[runAddOrUpdateEntry.ContentItemCanonicalId] = runAddOrUpdateEntry;
                }

                foreach (var runRemoveEntry in runContentItemsRegistry.ToRemove.Values)
                {
                    if (canonicalContentItemsRegistry.ToAddOrUpdate.ContainsKey(
                        runRemoveEntry.ContentItemCanonicalId))
                        canonicalContentItemsRegistry.ToAddOrUpdate.Remove(
                            runRemoveEntry.ContentItemCanonicalId);
                    canonicalContentItemsRegistry.ToRemove[runRemoveEntry.ContentItemCanonicalId] = runRemoveEntry;
                }

                updatedCanonicalContentItemsRegistryContent = JsonSerializer.Serialize(
                    canonicalContentItemsRegistry!);
            }
            else
            {
                updatedCanonicalContentItemsRegistryContent = JsonSerializer.Serialize(
                    runContentItemsRegistry);
            }

            await _storageService.WriteFileAsync(
                dataPipelineRun.InstanceId,
                canonicalContentItemsRegistryPath,
                updatedCanonicalContentItemsRegistryContent,
                "application/json",
                default);

            _logger.LogInformation("The data pipeline run {DataPipelineRunId} updated the canonical content item state with {AddOrUpdateCount} content items to add or update and {RemoveCount} content items to remove.",
                  dataPipelineRun.RunId,
                  runContentItemsRegistry.ToAddOrUpdate.Count,
                  runContentItemsRegistry.ToRemove.Count);

            #endregion

            // Combine dataPipelineRun, contentItems, and workItems into a single array
            var combinedArray = new object[] { dataPipelineRun }
                .Concat(contentItems)
                .ToArray();

            var upsertResultSuccessfull = await _cosmosDBService.UpsertDataPipelineRunBatchAsync(combinedArray);

            return upsertResultSuccessfull;
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> GetDataPipelineRun(
            string runId) =>
            await _cosmosDBService.RetrieveItem<DataPipelineRun>(
                runId,
                runId);

        public async Task<DataPipelineContentItem> GetDataPipelineContentItem(
            DataPipelineRunWorkItem dataPipelineRunWorkItem) =>
            await _cosmosDBService.GetDataPipelineContentItem(
                dataPipelineRunWorkItem.RunId,
                dataPipelineRunWorkItem.ContentItemCanonicalId);

        /// <inheritdoc/>
        public async Task<DataPipelineRunWorkItem?> GetDataPipelineRunWorkItem(
            string workItemId,
            string runId) =>
            await _cosmosDBService.RetrieveItem<DataPipelineRunWorkItem>(
                workItemId,
                runId);

        /// <inheritdoc/>
        public async Task<bool> UpdateDataPipelineRunStatus(
            DataPipelineRun dataPipelineRun)
        {
            try
            {
                // There is a Cosmos DB limit of up to 10 properties that can be patched at once.
                // We will patch the properties in two separate calls to avoid this limit.

                await _cosmosDBService.PatchItemPropertiesAsync<DataPipelineRun>(
                    dataPipelineRun.RunId,
                    dataPipelineRun.Id,
                    new Dictionary<string, object?>
                    {
                        { "/active_stages", dataPipelineRun.ActiveStages },
                        { "/completed_stages", dataPipelineRun.CompletedStages },
                        { "/failed_stages", dataPipelineRun.FailedStages },
                        { "/stages_metrics", dataPipelineRun.StagesMetrics },
                        { "/completed", dataPipelineRun.Completed },
                        { "/successful", dataPipelineRun.Successful },
                        { "/errors", dataPipelineRun.Errors }
                    });

                await _cosmosDBService.PatchItemPropertiesAsync<DataPipelineRun>(
                    dataPipelineRun.RunId,
                    dataPipelineRun.Id,
                    new Dictionary<string, object?>
                    {
                        { "/completed_on", DateTimeOffset.UtcNow },
                        { "/completed_by", ServiceContext.ServiceIdentity!.UPN },
                        { "/updated_on", DateTimeOffset.UtcNow },
                        { "/updated_by", ServiceContext.ServiceIdentity!.UPN }
                    });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update data pipeline run status for {RunId}.", dataPipelineRun.RunId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PersistDataPipelineRunWorkItems(
            List<DataPipelineRunWorkItem> workItems) =>
            await _cosmosDBService.UpsertDataPipelineRunBatchAsync(workItems.ToArray());

        /// <inheritdoc/>
        public async Task<bool> UpdateDataPipelineRunWorkItemsStatus(
            List<DataPipelineRunWorkItem> workItems) =>
            await _cosmosDBService.PatchDataPipelineRunWorkItemsStatusAsync(workItems);

        public async Task<bool> UpdateDataPipelineRunWorkItem(
            DataPipelineRunWorkItem workItem)
        {
            try
            {
                await _cosmosDBService.UpsertItemAsync<DataPipelineRunWorkItem>(
                    workItem.RunId,
                    workItem);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update data pipeline run work item {WorkItemId}.", workItem.Id);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DataPipelineRun>> GetActiveDataPipelineRuns()
        {
            var query = new QueryDefinition(
                $"SELECT DISTINCT * FROM c WHERE c.type = @type AND c.completed = false")
                .WithParameter("@type", DataPipelineTypes.DataPipelineRun);

            var dataPipelineRuns = await _cosmosDBService.RetrieveItemsAsync<DataPipelineRun>(query);

            return dataPipelineRuns;
        }

        /// <inheritdoc/>
        public async Task<List<DataPipelineRunWorkItem>> GetDataPipelineRunStageWorkItems(
            string runId,
            string stage)
        {
            var query = new QueryDefinition(
                $"SELECT * FROM c WHERE c.type = @type AND c.run_id = @runId and c.stage = @stage")
                .WithParameter("@type", DataPipelineTypes.DataPipelineRunWorkItem)
                .WithParameter("@runId", runId)
                .WithParameter("@stage", stage);
            var dataPipelineRunWorkItems = await _cosmosDBService.RetrieveItemsAsync<DataPipelineRunWorkItem>(query);
            return dataPipelineRunWorkItems;
        }

        /// <inheritdoc/>
        public async Task<(bool Success, List<DataPipelineStateArtifact> Artifacts)> TryLoadDataPipelineRunArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string artifactsNameFilter)
        {
            try
            {
                var artifactsFilter = string.Join('/',
                    [
                        GetDataPipelineCanonicalRootPath(
                            dataPipelineDefinition,
                            dataPipelineRun),
                        artifactsNameFilter
                    ]);

                var artifactsPaths = await _storageService.GetMatchingFilePathsAsync(
                    dataPipelineRun.InstanceId,
                    artifactsFilter);

                var result = await artifactsPaths
                    .ToAsyncEnumerable()
                    .SelectAwait(async path =>
                    {
                        var fileContent = await _storageService.ReadFileAsync(
                            dataPipelineRun.InstanceId,
                            path,
                            default);
                        return new DataPipelineStateArtifact
                        {
                            FileName = Path.GetFileName(path),
                            Content = fileContent
                        };
                    })
                    .ToListAsync();

                return (true, result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load data pipeline run artifacts for {RunId} and filter {ArtifactsNameFilter}.",
                    dataPipelineRun.RunId, artifactsNameFilter);
                return (false, []);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DataPipelineRunWorkItemArtifactChanged(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string artifactName)
        {
            var artifactPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    "content-items",
                    dataPipelineRunWorkItem.ContentItemCanonicalId.Trim('/').Replace('/', '-'),
                    artifactName
                ]);

            _logger.LogDebug("Checking if data pipeline run {DataPipelineRunId} has changed artifact {ArtifactPath}.",
                dataPipelineRun.Id,
                artifactPath);

            var changeLog = await ReadChangeLog(
                dataPipelineRun.InstanceId,
                artifactPath);
            return
                changeLog.LastChangedBy.Equals(dataPipelineRun.Id);
        }

        /// <inheritdoc/>
        public async Task<List<DataPipelineStateArtifact>> LoadDataPipelineRunWorkItemArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string artifactsNameFilter)
        {
            _logger.LogDebug("Loading data pipeline run work item artifacts for {RunId} and content item {ContentItemCanonicalId} with the following name filter: {ArtifactsNameFilter}.",
                dataPipelineRun.RunId,
                dataPipelineRunWorkItem.ContentItemCanonicalId,
                artifactsNameFilter);

            var artifactsFilter = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    "content-items",
                    dataPipelineRunWorkItem.ContentItemCanonicalId.Trim('/').Replace('/', '-'),
                    artifactsNameFilter
                ]);

            _logger.LogDebug("Artifacts filter path: {ArtifactsFilter}", artifactsFilter);

            var artifactsPaths = await _storageService.GetMatchingFilePathsAsync(
                dataPipelineRun.InstanceId,
                artifactsFilter);

            _logger.LogDebug("Found {ArtifactsCount} artifacts for the filter {ArtifactsFilter}.", artifactsPaths.Count, artifactsFilter);

            var result = await artifactsPaths
                .ToAsyncEnumerable()
                .SelectAwait(async path =>
                {
                    var fileContent = await _storageService.ReadFileAsync(
                        dataPipelineRun.InstanceId,
                        path,
                        default);

                    _logger.LogDebug("Loaded artifact {ArtifactFileName}for run {RunId} and content item {ContentItemCanonicalId}.",
                        path,
                        dataPipelineRun.RunId,
                        dataPipelineRunWorkItem.ContentItemCanonicalId);

                    return new DataPipelineStateArtifact
                    {
                        FileName = Path.GetFileName(path),
                        Content = fileContent
                    };
                })
                .ToListAsync();

            _logger.LogDebug("Returning {ArtifactsCount} artifacts.", result.Count);

            return result;
        }

        /// <inheritdoc/>
        public async Task SaveDataPipelineRunWorkItemArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<DataPipelineStateArtifact> artifacts)
        {
            _logger.LogDebug("Saving {ArtifactsCount} data pipeline run work item artifacts for {RunId} and content item {ContentItemCanonicalId}.",
                artifacts.Count,
                dataPipelineRun.RunId,
                dataPipelineRunWorkItem.ContentItemCanonicalId);

            var artifactsPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    "content-items",
                    dataPipelineRunWorkItem.ContentItemCanonicalId.Trim('/').Replace('/', '-')
                ]);

            _logger.LogDebug("Artifacts path: {ArtifactsPath}", artifactsPath);

            var artifactsWithError = new List<string>();

            await Parallel.ForEachAsync<DataPipelineStateArtifact>(
                artifacts,
                new ParallelOptions
                {
                    CancellationToken = default,
                    MaxDegreeOfParallelism = 10
                },
                async (artifact, token) =>
                {
                    _logger.LogDebug("Saving artifact {ArtifactFileName} for run {RunId} and content item {ContentItemCanonicalId}.",
                        artifact.FileName,
                        dataPipelineRun.RunId,
                        dataPipelineRunWorkItem.ContentItemCanonicalId);

                    var artifactFilePath = $"{artifactsPath}/{artifact.FileName}";
                    var changeLog = await ReadChangeLog(
                        dataPipelineRun.InstanceId,
                        artifactFilePath);

                    bool artifactChanged = false;

                    if (await _storageService.FileExistsAsync(
                        dataPipelineRun.InstanceId,
                        artifactFilePath,
                        token))
                    {
                        var existingArtifactContent = await _storageService.ReadFileAsync(
                            dataPipelineRun.InstanceId,
                            artifactFilePath,
                            token);
                        if (!existingArtifactContent.ToArray().SequenceEqual(
                            artifact.Content.ToArray()))
                        {
                            artifactChanged = true;
                            changeLog.AddChange(dataPipelineRun.Id);
                        }
                    }
                    else
                    {
                        artifactChanged = true;
                        changeLog.AddChange(dataPipelineRun.Id);
                    }

                    if (artifactChanged)
                    {
                        await _storageService.WriteFileAsync(
                            dataPipelineRun.InstanceId,
                            artifactFilePath,
                            artifact.Content.ToStream(),
                            artifact.ContentType,
                            token);
                        await WriteChangeLog(
                            dataPipelineRun.InstanceId,
                            artifactFilePath,
                            changeLog);
                    }
                    else if (changeLog.Changes.Count == 0)
                    {
                        // The change log was just created so we need to persist it
                        // regardless of whether the artifact changed or not.
                        changeLog.AddChange(dataPipelineRun.Id);
                        await WriteChangeLog(
                            dataPipelineRun.InstanceId,
                            artifactFilePath,
                            changeLog);
                    }

                    _logger.LogDebug("Successfully saved artifact {ArtifactFileName} for run {RunId} and content item {ContentItemCanonicalId}.",
                        artifact.FileName,
                        dataPipelineRun.RunId,
                        dataPipelineRunWorkItem.ContentItemCanonicalId);
                });

            _logger.LogDebug("Successfully saved data pipeline run work item artifacts for {RunId} and content item {ContentItemCanonicalId}.",
                dataPipelineRun.RunId,
                dataPipelineRunWorkItem.ContentItemCanonicalId);
        } 

        /// <inheritdoc/>
        public async Task SaveDataPipelineRunArtifacts(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            List<DataPipelineStateArtifact> artifacts)
        {
            var artifactsPath = GetDataPipelineCanonicalRootPath(
                dataPipelineDefinition,
                dataPipelineRun);

            var artifactsWithError = new List<string>();

            await Parallel.ForEachAsync<DataPipelineStateArtifact>(
                artifacts,
                new ParallelOptions
                {
                    CancellationToken = default,
                    MaxDegreeOfParallelism = 10
                },
                async (artifact, token) =>
                {
                    await _storageService.WriteFileAsync(
                        dataPipelineRun.InstanceId,
                        $"{artifactsPath}/{artifact.FileName}",
                        artifact.Content.ToStream(),
                        artifact.ContentType,
                        token);
                });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> LoadDataPipelineRunWorkItemParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string fileName,
            string contentSection = "content-items")
            where T : class, new()
        {
            var contentItemPartsPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    contentSection,
                    dataPipelineRunWorkItem.ContentItemCanonicalId.Trim('/').Replace('/', '-'),
                    fileName
                ]);

            if (! await _storageService.FileExistsAsync(
                dataPipelineRun.InstanceId,
                contentItemPartsPath,
                default))
                return [];

            var binaryContent = await _storageService.ReadFileAsync(
                dataPipelineRun.InstanceId,
                contentItemPartsPath,
                default);

            var result = await ParquetSerializer.DeserializeAsync<T>(
                binaryContent.ToStream());

            return [.. result];
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> LoadDataPipelineRunWorkItemParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string contentItemCanonicalId,
            string fileName,
            string contentSection = "content-items")
            where T : class, new()
        {
            var contentItemPartsPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    contentSection,
                    contentItemCanonicalId.Trim('/').Replace('/', '-'),
                    fileName
                ]);

            if (!await _storageService.FileExistsAsync(
                dataPipelineRun.InstanceId,
                contentItemPartsPath,
                default))
                return [];

            var binaryContent = await _storageService.ReadFileAsync(
                dataPipelineRun.InstanceId,
                contentItemPartsPath,
                default);

            var result = await ParquetSerializer.DeserializeAsync<T>(
                binaryContent.ToStream());

            return [.. result];
        }

        /// <inheritdoc/>
        public async Task SaveDataPipelineRunWorkItemParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            IEnumerable<T> contentItemParts,
            string fileName,
            string contentSection = "content-items")
            where T : class, new()
        {
            var contentItemPartsPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    contentSection,
                    dataPipelineRunWorkItem.ContentItemCanonicalId.Trim('/').Replace('/', '-'),
                    fileName
                ]);

            using var parquetStream = new MemoryStream();
            await ParquetSerializer.SerializeAsync<T>(
                contentItemParts,
                parquetStream);

            await _storageService.WriteFileAsync(
                dataPipelineRun.InstanceId,
                contentItemPartsPath,
                parquetStream,
                "application/vnd.apache.parquet",
                default);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> LoadDataPipelineRunParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            string filePath)
            where T : class, new()
        {
            var dataPipelineRunPartsPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    filePath
                ]);

            if (!await _storageService.FileExistsAsync(
                dataPipelineRun.InstanceId,
                dataPipelineRunPartsPath,
                default))
                return [];

            var binaryContent = await _storageService.ReadFileAsync(
                dataPipelineRun.InstanceId,
                dataPipelineRunPartsPath,
                default);

            var result = await ParquetSerializer.DeserializeAsync<T>(
                binaryContent.ToStream());

            return [.. result];
        }

        /// <inheritdoc/>
        public async Task SaveDataPipelineRunParts<T>(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            IEnumerable<T> dataPipelineRunParts,
            string filePath)
            where T : class, new()
        {
            var dataPipelineRunPartsPath = string.Join('/',
                [
                    GetDataPipelineCanonicalRootPath(
                        dataPipelineDefinition,
                        dataPipelineRun),
                    filePath
                ]);

            using var parquetStream = new MemoryStream();
            await ParquetSerializer.SerializeAsync<T>(
                dataPipelineRunParts,
                parquetStream);

            await _storageService.WriteFileAsync(
                dataPipelineRun.InstanceId,
                dataPipelineRunPartsPath,
                parquetStream,
                "application/vnd.apache.parquet",
                default);
        }

        public string GetDataPipelineCanonicalRootPath(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun) =>
            string.Join('/',
                [
                    $"/data-pipeline-state",
                    dataPipelineDefinition.Name,
                    "canonical-data",
                    dataPipelineRun.CanonicalRunId
                ]);

        /// <inheritdoc/>
        public string GetDataPipelineRunRootPath(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun) =>
            string.Join('/',
                [
                    $"/data-pipeline-state",
                    dataPipelineDefinition.Name,
                    "runs",
                    dataPipelineRun.UPN.NormalizeUserPrincipalName(),
                    $"{dataPipelineRun.CreatedOn:yyyy-MM-dd}",
                    dataPipelineRun.RunId
                ]);

        /// <inheritdoc/>
        public async Task<bool> StartDataPipelineRunWorkItemProcessing(
            Func<DataPipelineRunWorkItem, Task> processWorkItem) =>
            await _cosmosDBService.StartChangeFeedProcessorAsync(processWorkItem);

        /// <inheritdoc/>
        public async Task StopDataPipelineRunWorkItemProcessing() =>
            await _cosmosDBService.StopChangeFeedProcessorAsync();

        /// <inheritdoc/>
        public async Task<DataPipelineRunFilterResponse> GetDataPipelineRuns(DataPipelineRunFilter dataPipelineRunFilter) =>
            await _cosmosDBService.GetDataPipelineRuns(dataPipelineRunFilter);

        #region Changelog utils

        private async Task<DataPipelineStateArtifactChangeLog> ReadChangeLog(
            string instanceId,
            string filePath)
        {
            var changeLogFilePath = $"{filePath}.changelog.json";
            if (await _storageService.FileExistsAsync(
                instanceId,
                changeLogFilePath,
                default))
            {
                var changeLogContent = await _storageService.ReadFileAsync(
                    instanceId,
                    changeLogFilePath,
                    default);
                return JsonSerializer.Deserialize<DataPipelineStateArtifactChangeLog>(
                    changeLogContent.ToStream())!;
            }

            return new();
        }

        private async Task WriteChangeLog(
            string instanceId,
            string filePath,
            DataPipelineStateArtifactChangeLog changeLog)
        {
            var changeLogFilePath = $"{filePath}.changelog.json";
            var changeLogContent = JsonSerializer.Serialize(changeLog);
            await _storageService.WriteFileAsync(
                instanceId,
                changeLogFilePath,
                changeLogContent,
                "application/json",
                default);
        }

        #endregion
    }
}
