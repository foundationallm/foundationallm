using Azure;
using Azure.Core;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
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
            Dictionary<string, object> clientParameters) =>
            (AuthenticationTypes) clientParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] switch
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
        public async Task UploadDocuments(
            string indexName,
            List<string> fieldNames,
            List<object[]> fieldValues)
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
        }
    }
}
