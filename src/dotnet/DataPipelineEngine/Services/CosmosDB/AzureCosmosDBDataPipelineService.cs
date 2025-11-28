using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Text;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.DataPipelineEngine.Services.CosmosDB
{
    /// <summary>
    /// Provides the implementation for the Azure Cosmos DB data pipeline service.
    /// </summary>
    public class AzureCosmosDBDataPipelineService : IAzureCosmosDBDataPipelineService
    {
        protected readonly AzureCosmosDBSettings _settings;
        protected readonly ILogger<AzureCosmosDBDataPipelineService> _logger;

        protected readonly CosmosClient _cosmosClient;
        protected readonly Container _dataPipelineContainer;
        protected readonly Container _leasesContainer;

        private ChangeFeedProcessor _changeFeedProcessor = null!;
        private bool _changeFeedProcessorStarted = false;
        private Func<DataPipelineRunWorkItem, Task> _dataPipelineRunWorkItemProcessor = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBDataPipelineService"/> class.
        /// </summary>
        /// <param name="options">The <see cref="IOptions"/> providing the <see cref="AzureCosmosDBSettings"/>) with the Azure Cosmos DB settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public AzureCosmosDBDataPipelineService(
            IOptions<AzureCosmosDBSettings> options,
            ILogger<AzureCosmosDBDataPipelineService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            _logger.LogInformation("Initializing the Azure Cosmos DB data pipeline service...");

            _cosmosClient = GetCosmosDBClient();

            var database = _cosmosClient.GetDatabase(_settings.Database)
                ?? throw new DataPipelineServiceException($"The Azure Cosmos DB database '{_settings.Database}' was not found.");

            // The Data Pipelines container name is the first container in the list.
            var dataPipelinesContainerName = _settings.Containers.Split(',')[0];
            _dataPipelineContainer = database.GetContainer(dataPipelinesContainerName)
                ?? throw new DataPipelineServiceException($"The Azure Cosmos DB container [{dataPipelinesContainerName}] was not found.");

            _leasesContainer = database.GetContainer("leases")
                ?? throw new DataPipelineServiceException($"The Azure Cosmos DB container [leases] was not found.");

            _logger.LogInformation("Successfully initialized the Azure Cosmos DB datra pipeline service.");
        }

        #region Initialization

        private CosmosClient GetCosmosDBClient()
        {
            var opt = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            // Configure CosmosSystemTextJsonSerializer
            var serializer
                = new CosmosSystemTextJsonSerializer(opt);
            var result = new CosmosClientBuilder(
                    _settings.Endpoint,
                    ServiceContext.AzureCredential)
                .WithCustomSerializer(serializer)
                .WithBulkExecution(true)
                .WithConnectionModeGateway()
                .Build();

            if (!_settings.EnableTracing)
            {
                var defaultTrace =
                    Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
                var traceSource = (TraceSource)defaultTrace?.GetProperty("TraceSource")?.GetValue(null)!;
                traceSource.Switch.Level = SourceLevels.All;
                traceSource.Listeners.Clear();
            }

            return result;
        }

        #endregion

        /// <inheritdoc/>
        public async Task<T> UpsertItemAsync<T>(
            string partitionKey,
            T item,
            CancellationToken cancellationToken = default)
        {
            var response = await _dataPipelineContainer.UpsertItemAsync<T>(
                item: item,
                partitionKey: new PartitionKey(partitionKey),
                cancellationToken: cancellationToken
            );

            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task<T> PatchItemPropertiesAsync<T>(
            string partitionKey,
            string id,
            Dictionary<string, object?> propertyValues,
            CancellationToken cancellationToken = default)
        {
            var patchOperations = propertyValues.Keys
                .Select(key => PatchOperation.Set(key, propertyValues[key])).ToArray();
            var response = await _dataPipelineContainer.PatchItemAsync<T>(
                id: id,
                partitionKey: new PartitionKey(partitionKey),
                patchOperations: patchOperations,
                cancellationToken: cancellationToken
            );
            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task<List<T>> RetrieveItemsAsync<T>(
            QueryDefinition query)
        {
            var results = _dataPipelineContainer.GetItemQueryIterator<T>(query);

            List<T> output = [];
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync();
                output.AddRange(response);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task<T> RetrieveItem<T>(string id, string partitionKey)
        {
            var result = await _dataPipelineContainer.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> UpsertDataPipelineRunBatchAsync(params dynamic[] dataPipelineRunItems)
        {
            if (dataPipelineRunItems.Select(m => m.RunId).Distinct().Count() > 1)
            {
                throw new ArgumentException("All items must have the same partition key.");
            }

            PartitionKey partitionKey = new(dataPipelineRunItems.First().RunId);

            foreach (var chunk in dataPipelineRunItems.Chunk(100))
            {
                var batch = _dataPipelineContainer.CreateTransactionalBatch(partitionKey);
                foreach (var message in chunk)
                {
                    batch.UpsertItem(
                        item: message
                    );
                }

                var result = await batch.ExecuteAsync();

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to upsert one or more data pipeline run items. Status code: {StatusCode}, Error message: {ErrorMessage}",
                        result.StatusCode, result.ErrorMessage);
                    return false;
                }
                else
                {
                    _logger.LogInformation("Successfully upserted {Count} data pipeline run items.",
                        chunk.Length);
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> PatchDataPipelineRunWorkItemsStatusAsync(
            List<DataPipelineRunWorkItem> workItems)
        {
            var concurrentTasks = new List<Task<ItemResponse<DataPipelineRunWorkItem>>>();
            foreach (var workItem in workItems)
                concurrentTasks.Add(
                    _dataPipelineContainer.PatchItemAsync<DataPipelineRunWorkItem>(
                        id: workItem.Id,
                        partitionKey: new PartitionKey(workItem.RunId),
                        patchOperations: new[]
                        {
                            PatchOperation.Replace("/completed", workItem.Completed),
                            PatchOperation.Replace("/successful", workItem.Successful),
                            PatchOperation.Replace("/errors", workItem.Errors)
                        }
                    )
                );

            await Task.WhenAll(concurrentTasks);

            return concurrentTasks.All(task =>
                task.Result.StatusCode == System.Net.HttpStatusCode.OK);
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRunFilterResponse> GetDataPipelineRuns(
            DataPipelineRunFilter dataPipelineRunFilter)
        {
            var queryString = "SELECT * FROM c WHERE c.type = @type AND c.instance_id = @instanceId";
            var query = new QueryDefinition(queryString)
                .WithParameter("@type", DataPipelineTypes.DataPipelineRun)
                .WithParameter("@instanceId", dataPipelineRunFilter.InstanceId);

            if (!string.IsNullOrEmpty(dataPipelineRunFilter.DataPipelineName)
                && dataPipelineRunFilter.DataPipelineName != "all")
            {
                queryString += " AND STARTSWITH(c.object_id, @objectIdPrefix)";
                query.WithParameter("@objectIdPrefix",
                    string.Join('/', [
                        $"/instances/{dataPipelineRunFilter.InstanceId}",
                        $"providers/{ResourceProviderNames.FoundationaLLM_DataPipeline}",
                        $"{DataPipelineResourceTypeNames.DataPipelines}/{dataPipelineRunFilter.DataPipelineName}"]));
            }
            if (dataPipelineRunFilter.Completed.HasValue)
            {
                queryString += " AND c.completed = @completed";
                query.WithParameter("@completed", dataPipelineRunFilter.Completed.Value);
            }
            if (dataPipelineRunFilter.Successful.HasValue)
            {
                queryString += " AND c.successful = @successful";
                query.WithParameter("@successful", dataPipelineRunFilter.Successful.Value);
            }
            if (dataPipelineRunFilter.StartTime.HasValue)
            {
                queryString += " AND c._ts >= @startTime";
                query.WithParameter("@startTime", dataPipelineRunFilter.StartTime.Value.ToUnixTimeSeconds());
            }
            if (dataPipelineRunFilter.EndTime.HasValue)
            {
                queryString += " AND c._ts <= @endTime";
                query.WithParameter("@endTime", dataPipelineRunFilter.EndTime.Value.ToUnixTimeSeconds());
            }

            queryString += " ORDER BY c._ts DESC";

            // Re-create QueryDefinition with the final query string and parameters
            var finalQuery = new QueryDefinition(queryString);
            foreach (var param in query.GetQueryParameters())
            {
                finalQuery.WithParameter(param.Name, param.Value);
            }

            var queryRequestOptions = new QueryRequestOptions
            {
                MaxItemCount = dataPipelineRunFilter.PageSize ?? 10
            };

            var iterator = _dataPipelineContainer.GetItemQueryIterator<DataPipelineRun>(
                finalQuery,
                continuationToken: dataPipelineRunFilter.ContinuationToken,
                requestOptions: queryRequestOptions);

            var items = new List<DataPipelineRun>();
            string? continuationToken = null;

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                items.AddRange(response);
                continuationToken = response.ContinuationToken;
            }

            return new DataPipelineRunFilterResponse
            {
                Name = string.Empty,
                Items = items,
                ContinuationToken = continuationToken
            };
        }

        /// <inheritdoc/>
        public async Task<DataPipelineContentItem> GetDataPipelineContentItem(
            string dataPipelineRunId,
            string contentItemCanonicalId)
        {
            var queryString = "SELECT * FROM c WHERE c.type = @type AND c.run_id = @runId AND c.content_identifier.canonical_id = @canonicalId";
            var query = new QueryDefinition(queryString)
                .WithParameter("@type", DataPipelineTypes.DataPipelineContentItem)
                .WithParameter("@runId", dataPipelineRunId)
                .WithParameter("@canonicalId", contentItemCanonicalId);
            var results = await RetrieveItemsAsync<DataPipelineContentItem>(query);
            if (results.Count == 0)
            {
                throw new DataPipelineServiceException($"No content item found for run ID '{dataPipelineRunId}' and canonical ID '{contentItemCanonicalId}'.");
            }
            return results.First();
        }

        #region Change Feed Processor

        /// <inheritdoc/>
        public async Task<bool> StartChangeFeedProcessorAsync(
            Func<DataPipelineRunWorkItem, Task> dataPipelineRunWorkItemProcessor)
        {
            _logger.LogInformation("Starting the data pipeline run work items change feed processor...");

            try
            {
                _changeFeedProcessor = _dataPipelineContainer
                    .GetChangeFeedProcessorBuilder<dynamic>(
                        "ProcessDataPipelineRunWorkItems", ChangeFeedHandler)
                    .WithInstanceName("ProcessDataPipelineRunWorkItems")
                    .WithLeaseContainer(_leasesContainer)
                    .Build();

                await _changeFeedProcessor.StartAsync();

                _dataPipelineRunWorkItemProcessor = dataPipelineRunWorkItemProcessor;

                _changeFeedProcessorStarted = true;
                _logger.LogInformation("The data pipeline run work items change feed processor started.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error starting the data pipeline run work items change feed processor.");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task StopChangeFeedProcessorAsync()
        {
            if (_changeFeedProcessor is not null
                && _changeFeedProcessorStarted)
            {
                try
                {
                    _logger.LogInformation("Stopping the data pipeline run work items change feed processor...");

                    await _changeFeedProcessor.StopAsync();

                    _logger.LogInformation("The data pipeline run work items change feed processor stopped.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error stopping the data pipeline run work items change feed processor.");
                }
            }
        }

        private async Task ChangeFeedHandler(
            IReadOnlyCollection<dynamic> input,
            CancellationToken cancellationToken)
        {
            try
            {
                var completedWorkItems = input
                    .Where(document => ((JsonElement)document).GetProperty("type").ToString() == DataPipelineTypes.DataPipelineRunWorkItem)
                    .Select<dynamic, DataPipelineRunWorkItem>(document =>
                        JsonSerializer.Deserialize<DataPipelineRunWorkItem>((JsonElement)document)!)
                    .Where(workItem => workItem.Completed)
                    .ToList();

                if (completedWorkItems.Count > 0)
                {
                    _logger.LogInformation("Processing {WorkItemsCount} completed data pipeline run work items.",
                        completedWorkItems.Count);

                    await Parallel.ForEachAsync<DataPipelineRunWorkItem>(
                        completedWorkItems,
                        new ParallelOptions
                        {
                            CancellationToken = cancellationToken,
                            MaxDegreeOfParallelism = 10
                        },
                        async (workItem, token) =>
                        {
                            try
                            {
                                await _dataPipelineRunWorkItemProcessor(workItem);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(
                                    ex,
                                    "There was an error processing the data pipeline run work item with id {WorkItemId}",
                                    workItem.Id);
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error processing the data pipeline run work items change feed.");
            }
        }

        #endregion
    }
}
