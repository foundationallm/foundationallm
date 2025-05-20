using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;

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

        public async Task UploadDocuments(
            string indexName)
        {
            var searchClient = _searchIndexClient.GetSearchClient(indexName);

            //searchClient.
        }
    }
}
