using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

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
        /// <returns>A dictionary that specifies for each index id value whether it was successfully uploaded or not.</returns>
        /// <remarks>
        /// The code in this method assumes that the first field in <paramref name="fieldNames"/> is the key field for the index.
        /// </remarks>
        Task<Dictionary<string,bool>> UploadDocuments(
            string indexName,
            List<string> fieldNames,
            List<object[]> fieldValues);

        /// <summary>
        /// Searches for documents in the specified index that match the given filter and are similar to the provided
        /// user prompt embedding.
        /// </summary>
        /// <param name="indexName">The name of the index to search within.</param>
        /// <param name="select">A list of fields to select in the search results. If null or empty, all fields will be selected.</param>
        /// <param name="filter">A filter expression to narrow down the search results.</param>
        /// <param name="userPrompt">The original user prompt.</param>
        /// <param name="userPromptEmbedding">A read-only memory segment representing the embedding of the user prompt.</param>
        /// <param name="embeddingPropertyName">The name of the index property that contains embeddings.</param>
        /// <param name="similarityThreshold">The minimum similarity score required for a document to be included in the results. Must be a value between
        /// 0 and 1.</param>
        /// <param name="topN">The maximum number of documents to return. Must be a positive integer.</param>
        /// <param name="useSemanticRanking">A flag that indicates whether semantic ranking should be used or not.</param>
        /// <returns>An enumerable collection of <see cref="SearchDocument"/> objects that match the filter and meet the similarity threshold, ordered by
        /// relevance. The collection will be empty if no matching documents are found.</returns>
        Task<IEnumerable<SearchDocument>> SearchDocuments(
            string indexName,
            IEnumerable<string> select,
            string filter,
            string? userPrompt,
            ReadOnlyMemory<float>? userPromptEmbedding,
            string? embeddingPropertyName,
            float? similarityThreshold,
            int topN,
            bool useSemanticRanking);

        /// <summary>
        /// Deletes documents from the specified index in the Azure AI Search service based on a filter.
        /// </summary>
        /// <param name="indexName">The name of the index from which the documents should be deleted.</param>
        /// <param name="keyFieldName">The name of the key field for the index.</param>
        /// <param name="filter">The filter used to identify the documents to be deleted.</param>
        /// <returns>The number of documents that were deleted.</returns>
        Task<int> DeleteDocuments(
            string indexName,
            string keyFieldName,
            string filter);
    }
}
