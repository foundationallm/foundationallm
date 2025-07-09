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
    }
}
