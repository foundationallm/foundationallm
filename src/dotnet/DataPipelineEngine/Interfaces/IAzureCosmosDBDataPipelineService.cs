using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using Microsoft.Azure.Cosmos;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the Azure Cosmos DB data pipeline service.
    /// </summary>
    public interface IAzureCosmosDBDataPipelineService
    {
        /// <summary>
        /// Upserts an item in Azure Cosmos DB.
        /// </summary>
        /// <typeparam name="T">The type of the item to upsert.</typeparam>
        /// <param name="partitionKey">The partition of the item to upsert.</param>
        /// <param name="item">The item to upsert.</param>
        /// <param name="cancellationToken">The cancellation token used to signal a cancellation request.</param>
        /// <returns>The upserted item.</returns>
        Task<T> UpsertItemAsync<T>(
            string partitionKey,
            T item,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Patches the specified properties of an item in Azure Cosmos DB.
        /// </summary>
        /// <typeparam name="T">The type of the item to patch.</typeparam>
        /// <param name="partitionKey">The partition key of the item.</param>
        /// <param name="id">The identifier of the item.</param>
        /// <param name="propertyValues">Dictionary of the property names and values to patch.</param>
        /// <param name="cancellationToken">Cancellation token for async calls.</param>
        /// <returns>The patched object of type <typeparam name="T"></typeparam>.</returns>
        Task<T> PatchItemPropertiesAsync<T>(
            string partitionKey,
            string id,
            Dictionary<string, object?> propertyValues,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch create or update data pipeline run items.
        /// </summary>
        /// <param name="dataPipelineRunItems">Data pipeline run items to create or replace.</param>
        /// <returns><see langword="true"/> if the batch is successfully processed.</returns>
        Task<bool> UpsertDataPipelineRunBatchAsync(params dynamic[] dataPipelineRunItems);

        /// <summary>
        /// Retrieves items from Azure Cosmos DB.
        /// </summary>
        /// <typeparam name="T">The type of the items to retrieve.</typeparam>
        /// <param name="query">The query definition used to retrieve the items.</param>
        /// <returns>The list of retrieved items.</returns>
        Task<List<T>> RetrieveItemsAsync<T>(
            QueryDefinition query);

        /// <summary>
        /// Retrieves an item from Azure Cosmos DB.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="id">The identifier of the item.</param>
        /// <param name="partitionKey">The partition key of the item.</param>
        /// <returns></returns>
        Task<T> RetrieveItem<T>(
            string id,
            string partitionKey);

        /// <summary>
        /// Patches the status of data pipeline run work items in Azure Cosmos DB.
        /// </summary>
        /// <param name="workItems">The list of data pipeline work items that need the status update.</param>
        /// <returns><see langword="true"/> if the batch is successfully processed.</returns>
        Task<bool> PatchDataPipelineRunWorkItemsStatusAsync(
            List<DataPipelineRunWorkItem> workItems);

        /// <summary>
        /// Gets a list of data pipeline runs filtered by the provided filter criteria.
        /// </summary>
        /// <param name="dataPipelineRunFilter">The filter criteria used to filter data pipeline runs.</param>
        /// <returns>The list of requests data pipeline runs.</returns>
        Task<DataPipelineRunFilterResponse> GetDataPipelineRuns(
            DataPipelineRunFilter dataPipelineRunFilter);

        /// <summary>
        /// Get a data pipeline content item by its canonical identifier.
        /// </summary>
        /// <param name="dataPipelineRunId">Thje data pipeine run identifier.</param>
        /// <param name="contentItemCanonicalId">The canonical identifier of the content item run.</param>
        /// <returns></returns>
        Task<DataPipelineContentItem> GetDataPipelineContentItem(
            string dataPipelineRunId,
            string contentItemCanonicalId);

        /// <summary>
        /// Starts the change feed processor for the data pipeline run work items.
        /// </summary>
        /// <param name="dataPipelineRunWorkItemProcessor">The asynchronous delegate that is invoked for each data pipeline run work item.</param>
        /// <returns><see langword="true"/> if the change feed processor is successfully started.</returns>
        Task<bool> StartChangeFeedProcessorAsync(
            Func<DataPipelineRunWorkItem, Task> dataPipelineRunWorkItemProcessor);

        /// <summary>
        /// Stops the change feed processor for the data pipeline run work items.
        /// </summary>
        Task StopChangeFeedProcessorAsync();
    }
}
