using FoundationaLLM.Common.Models.DataPipelines;
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
        Task PatchDataPipelineRunWorkItemsStatusAsync(
            List<DataPipelineRunWorkItem> workItems);
    }
}
