using Microsoft.Azure.Cosmos;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the interface for the Azure Cosmos DB service that provides core database services.
    /// </summary>
    public interface IAzureCosmosDBServiceBase
    {
        /// <summary>
        /// Gets the context container for the Azure Cosmos DB service.
        /// </summary>
        Container ContextContainer { get; }

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
        /// Retrieves items from Azure Cosmos DB.
        /// </summary>
        /// <typeparam name="T">The type of item to retrieve.</typeparam>
        /// <param name="query">The query definition used to retrieve the items.</param>
        /// <returns>The list of retrieved items.</returns>
        Task<List<T>> RetrieveItems<T>(
            QueryDefinition query);

        /// <summary>
        /// Determines whether an item with the specified partition key and identifier exists in the data store.
        /// </summary>
        /// <typeparam name="T">The type of the item to check for existence.</typeparam>
        /// <param name="partitionKey">The partition key that identifies the logical partition containing the item. Cannot be null or empty.</param>
        /// <param name="id">The unique identifier of the item within the specified partition. Cannot be null or empty.</param>
        /// <param name="existencePredicate">An optional predicate function to further validate the existence of the item based on its properties.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the item
        /// exists; otherwise, <see langword="false"/>.</returns>
        Task<bool> ItemExists<T>(
            string partitionKey,
            string id,
            Func<T, bool>? existencePredicate);
    }
}
