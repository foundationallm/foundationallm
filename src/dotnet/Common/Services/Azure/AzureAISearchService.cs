using Azure;
using Azure.Core;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients.Http;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Implements the Azure AI Search Service.
    /// </summary>
    /// <param name="searchIndexClient">The Azure SDK client for the Azure AI Search service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureAISearchService(
        SearchIndexClient searchIndexClient,
        ILogger<AzureAISearchService> logger) : IAzureAISearchService
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SearchIndexClient"/> class using a dictionary of parameter values.
        /// </summary>
        /// <param name="clientParameters">The dictionary of parameter values used to create the client instance.</param>
        /// <returns>The requested client instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static SearchIndexClient CreateSearchIndexClient(
            Dictionary<string, object> clientParameters)
        {
            if (clientParameters.TryGetValue(HttpClientFactoryServiceKeyNames.EnableDiagnostics, out var enableDiagnostics)
                && (bool)enableDiagnostics)
            {
                var searchClientOptions = new SearchClientOptions
                {
                    Diagnostics =
                    {
                        IsLoggingEnabled = true,
                        IsLoggingContentEnabled = true
                    }
                };
                searchClientOptions.AddPolicy(
                    new HttpPipelineInterceptPolicy(),
                    HttpPipelinePosition.PerCall);

                return (AuthenticationTypes)clientParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] switch
                {
                    AuthenticationTypes.AzureIdentity => new SearchIndexClient(
                        new Uri(clientParameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString()!),
                        ServiceContext.AzureCredential,
                        searchClientOptions),
                    AuthenticationTypes.APIKey => new SearchIndexClient(
                        new Uri(clientParameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString()!),
                        new AzureKeyCredential(clientParameters[HttpClientFactoryServiceKeyNames.APIKey].ToString()!),
                        searchClientOptions),
                    _ => throw new ConfigurationValueException(
                        $"The {clientParameters[HttpClientFactoryServiceKeyNames.AuthenticationType]} authentication type is not supported by the Azure AI Search service.")
                };
            }

            return (AuthenticationTypes)clientParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] switch
            {
                AuthenticationTypes.AzureIdentity => new SearchIndexClient(
                    new Uri(clientParameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString()!),
                    ServiceContext.AzureCredential),
                AuthenticationTypes.APIKey => new SearchIndexClient(
                    new Uri(clientParameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString()!),
                    new AzureKeyCredential(clientParameters[HttpClientFactoryServiceKeyNames.APIKey].ToString()!)),
                _ => throw new ConfigurationValueException(
                    $"The {clientParameters[HttpClientFactoryServiceKeyNames.AuthenticationType]} authentication type is not supported by the Azure AI Search service.")
            };
        }

        private readonly SearchIndexClient _searchIndexClient = searchIndexClient;
        private readonly ILogger<AzureAISearchService> _logger = logger;

        /// <inheritdoc/>
        public async Task CreateIndexIfNotExists(
            string indexName,
            IEnumerable<SearchField> indexFields,
            VectorSearch? indexConfiguration = null)
        {
            var indexExists = true;

            try
            {
                var index = await _searchIndexClient.GetIndexAsync(indexName).ConfigureAwait(false);
                
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Index does not exist, proceed to create it
                indexExists = false;
            }

            if (!indexExists)
            {
                var indexDefinition = new SearchIndex(indexName)
                {
                    Fields = [.. indexFields]
                };

                if (indexConfiguration is not null)
                    indexDefinition.VectorSearch = indexConfiguration;

                await _searchIndexClient.CreateIndexAsync(indexDefinition);
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, bool>> UploadDocuments(
            string indexName,
            List<string> fieldNames,
            List<object[]> fieldValues)
        {
            int bucketSize = 500;
            int bucketCount = (fieldValues.Count + bucketSize - 1) / bucketSize;

            using var semaphore = new SemaphoreSlim(10); // Limit to 10 concurrent uploads

            var uploadTasks = Enumerable.Range(0, bucketCount)
                .Select(async i =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await UploadDocumentsInternal(
                            indexName,
                            fieldNames,
                            [.. fieldValues.Skip(i * bucketSize).Take(bucketSize)]);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var uploadResults = await Task.WhenAll(uploadTasks);
            return uploadResults
                .SelectMany(x => x)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private async Task<Dictionary<string, bool>> UploadDocumentsInternal(
            string indexName,
            List<string> fieldNames,
            List<object[]> fieldValues)
        {
            try
            {
                // Build the endpoint URI for the documents action
                var endpoint = new Uri($"{_searchIndexClient.Endpoint}/indexes/{indexName}/docs/index?api-version=2024-07-01");

                var request = _searchIndexClient.Pipeline.CreateRequest();
                request.Uri.Reset(endpoint);

                fieldNames = [.. fieldNames.Prepend("@search.action")];
                var payload = new
                {
                    value = fieldValues.Select(
                    x => fieldNames
                        .Zip(
                            x.Prepend("mergeOrUpload"),
                            (name, value) => new KeyValuePair<string, object>(name, value))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                };

                var json = JsonSerializer.Serialize(payload);
                var jsonBytes = Encoding.UTF8.GetBytes(json);

                request.Headers.Add("Content-Type", "application/json");
                request.Method = RequestMethod.Post;
                request.Content = RequestContent.Create(jsonBytes);
                var message = new HttpMessage(request, _searchIndexClient.Pipeline.ResponseClassifier);
                await _searchIndexClient.Pipeline.SendAsync(message, default);

                if (message.Response.IsError)
                    throw new RequestFailedException(message.Response);

                return fieldValues
                    .Select(fv => fv[0].ToString()!)
                    .ToDictionary(id => id, id => true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading documents to index {IndexName}", indexName);

                return fieldValues
                    .Select(fv => fv[0].ToString()!)
                    .ToDictionary(id => id, id => false);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchDocument>> SearchDocuments(
            string indexName,
            IEnumerable<string> select,
            string filter,
            string? userPrompt,
            ReadOnlyMemory<float>? userPromptEmbedding,
            string? embeddingPropertyName,
            float? similarityThreshold,
            int topN,
            bool useSemanticRanking)
        {
            // Create a SearchClient for the index
            var searchClient = _searchIndexClient.GetSearchClient(indexName);

            var vectorQuery = default(VectorizedQuery);
            if (userPromptEmbedding is not null)
            {
                vectorQuery = new VectorizedQuery(userPromptEmbedding.Value);
                vectorQuery.Fields.Add(embeddingPropertyName!);
                vectorQuery.KNearestNeighborsCount = 500;
                vectorQuery.Threshold = new VectorSimilarityThreshold(similarityThreshold!.Value);
            }

            var searchOptions = new SearchOptions
            {
                QueryType = useSemanticRanking
                    ? SearchQueryType.Semantic
                    : SearchQueryType.Simple,
                Filter = filter,
                Size = topN,
                VectorSearch = new VectorSearchOptions()
                {
                    FilterMode = VectorFilterMode.PostFilter
                }
            };
            foreach (var field in select)
                searchOptions.Select.Add(field);

            if (vectorQuery is not null)
                searchOptions.VectorSearch.Queries.Add(vectorQuery);

            if (useSemanticRanking)
            {
                searchOptions.SemanticSearch = new SemanticSearchOptions();
                searchOptions.SemanticSearch.SemanticFields.Add("Text");
            }

            // Execute the search
            var results = new List<SearchDocument>();
            var response = userPrompt is null
                ? await searchClient.SearchAsync<SearchDocument>(
                    searchOptions)
                : await searchClient.SearchAsync<SearchDocument>(
                    userPrompt,
                    searchOptions);

            await foreach (var result in response.Value.GetResultsAsync())
            {
                // Add the score to the document for easy access
                result.Document["Score"] = result.Score;
                result.Document["SemanticRankingScore"] = useSemanticRanking
                    ? result.SemanticSearch.RerankerScore
                    : null;
                results.Add(result.Document);
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<int> DeleteDocuments(
            string indexName,
            string keyFieldName,
            string filter)
        {
            // Create a SearchClient for the index
            var searchClient = _searchIndexClient.GetSearchClient(indexName);

            // Search for documents matching the filter
            var options = new SearchOptions
            {
                Filter = filter,
                Select = { keyFieldName }
            };

            var keysToDelete = new List<string>();
            var searchResponse = await searchClient.SearchAsync<SearchDocument>(options);
            await foreach (var result in searchResponse.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue(keyFieldName, out var keyValue) && keyValue is string key)
                    keysToDelete.Add(key);
            }

            if (keysToDelete.Count == 0)
                return 0;

            var deleteResponse = await searchClient.DeleteDocumentsAsync(
                keyFieldName,
                keysToDelete);

            var deletedKeysCount = deleteResponse.Value.Results
                .Count(r => r.Status == StatusCodes.Status200OK);

            if (deletedKeysCount != keysToDelete.Count)
                throw new RequestFailedException(
                    $"Failed to delete {keysToDelete.Count - deletedKeysCount} of {keysToDelete.Count} index entries.");

            return deletedKeysCount;
        }
    }
}
