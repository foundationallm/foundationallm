namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Contains Azure Cosmos DB change feed methods.
    /// </summary>
    public interface ICosmosDbChangeFeedService
    {
        /// <summary>
        /// Indicates whether the Azure Cosmos DB change feed is initialized.
        /// </summary>
        bool IsInitialized { get; }
        /// <summary>
        /// Initializes the change feed processors and starts listening for changes.
        /// </summary>
        /// <returns></returns>
        Task StartChangeFeedProcessorsAsync();
        /// <summary>
        /// Stops the change feed processors.
        /// </summary>
        /// <returns></returns>
        Task StopChangeFeedProcessorAsync();
    }
}
