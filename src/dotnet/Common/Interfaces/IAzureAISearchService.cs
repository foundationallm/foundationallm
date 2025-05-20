using Azure.Search.Documents.Indexes.Models;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Interface for Azure AI Search Service.
    /// </summary>
    public interface IAzureAISearchService
    {
        /// <summary>
        /// Creates an index in the Azure AI Search service if it does not already exist.
        /// </summary>
        /// <param name="indexName">The name of the index to create.</param>
        /// <param name="indexFields">The list of fields for the newly created index.</param>
        /// <param name="indexConfiguration">The optionsl vectorization configuration for the newly created index.</param>
        /// <returns></returns>
        Task CreateIndexIfNotExists(
            string indexName,
            IEnumerable<SearchField> indexFields,
            VectorSearch? indexConfiguration = null);
    }
}
