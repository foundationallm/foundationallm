namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Provides the interface for the Azure Cosmos DB file service.
    /// </summary>
    public interface IAzureCosmosDBFileService
    {
        /// <summary>
        /// Creates or updates an item in the default Context container
        /// </summary>
        /// <typeparam name="T">The type of the item to create or update.</typeparam>
        /// <param name="item">The item to be created or updated.</param>
        /// <param name="partitionKey">The partition key of the item.</param>
        /// <param name="cancellationToken">Cancellation token for async calls.</param>
        /// <returns>The created or updated object of type <typeparamref name="T"/>.</returns>
        Task<T> UpsertItemAsync<T>(
            string partitionKey,
            T item,
            CancellationToken cancellationToken = default);
    }
}
