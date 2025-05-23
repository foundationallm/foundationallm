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

        /// <summary>
        /// Uploads documents to the specified index in the Azure AI Search service.
        /// </summary>
        /// <param name="indexName">The name of the index in which the documents should be uploaded.</param>
        /// <param name="fieldNames">The names of the fields of the documents.</param>
        /// <param name="fieldValues">The values of the fields of the documents.</param>
        Task UploadDocuments(
            string indexName,
            List<string> fieldNames,
            List<object[]> fieldValues);
    }
}
