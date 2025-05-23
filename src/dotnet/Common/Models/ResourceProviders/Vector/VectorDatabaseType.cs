namespace FoundationaLLM.Common.Models.ResourceProviders.Vector
{
    /// <summary>
    /// Defines the types of vector databases available to store embeddings.
    /// </summary>
    public enum VectorDatabaseType
    {
        /// <summary>
        /// Azure AI Search vector database.
        /// </summary>
        AzureAISearch,

        /// <summary>
        /// Azure Cosmos DB NoSQL vector database.
        /// </summary>
        AzureCosmosDBNoSQL,

        /// <summary>
        /// Azure PostgreSQL vector database.
        /// </summary>
        AzurePostgreSQL
    }
}
