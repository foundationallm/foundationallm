using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Services;
using Microsoft.Extensions.Options;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using SemanticKernel.Tests.Models;
using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Common.Authentication;

namespace FoundationaLLM.SemanticKernel.Tests.Services
{
    public class AzureAISearchIndexingServiceTests
    {
        private readonly SearchIndexClient _searchIndexClient;
        private readonly IIndexingService _indexingService;
        private readonly string _indexName = Environment.GetEnvironmentVariable("AzureAISearchIndexingServiceTestsCollectionName") ?? "semantickernel-integration-tests";

        public AzureAISearchIndexingServiceTests()
        {
            var endpoint = Environment.GetEnvironmentVariable("AzureAISearchIndexingServiceTestsSearchEndpoint") ?? "";
            _searchIndexClient = new SearchIndexClient(
                new Uri(endpoint),
                DefaultAuthentication.GetAzureCredential()
            );
            _indexingService = new AzureAISearchIndexingService(
                Options.Create(
                    new AzureAISearchIndexingServiceSettings
                    {
                        Endpoint = endpoint,
                        AuthenticationType = AzureAISearchAuthenticationTypes.AzureIdentity
                    }
                ),
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AzureAISearchIndexingService>()
            );
        }

        private async Task CreateIndex()
        {
            var searchIndex = new SearchIndex(
                _indexName,
                new FieldBuilder().Build(typeof(TestIndexSchema))
            )
            {
                VectorSearch = new VectorSearch()
            };

            searchIndex.VectorSearch.Algorithms.Add(
                new HnswAlgorithmConfiguration("algorithm-config")
            );

            searchIndex.VectorSearch.Profiles.Add(
                new VectorSearchProfile("vector-config", "algorithm-config")
            );

            await _searchIndexClient.CreateIndexAsync(
                searchIndex
            );
        }

        [Fact]
        public async void TestIndexEmbeddingsAsync()
        {
            await CreateIndex();

            EmbeddedContent embeddedContent = new EmbeddedContent
            {
                ContentId = new ContentIdentifier
                {
                    MultipartId = new List<string> {
                        "https://somesa.blob.core.windows.net",
                        "vectorization-input",
                        "somedata.pdf"
                    },
                    ContentSourceProfileName = "SomePDFData",
                    CanonicalId = "SomeBusinessUnit/SomePDFData"
                },
                ContentParts = new List<EmbeddedContentPart>
                {
                    new EmbeddedContentPart {
                        Id = "1",
                        Content = "This is Phrase #1",
                        Embedding = new Embedding
                        {
                            Vector = new ReadOnlyMemory<float>(Enumerable.Repeat<float>(0, 1536).ToArray())
                        }
                    },
                    new EmbeddedContentPart {
                        Id = "2",
                        Content = "This is Phrase #2",
                        Embedding = new Embedding
                        {
                            Vector = new ReadOnlyMemory<float>(Enumerable.Repeat<float>(0, 1536).ToArray())
                        }
                    },
                    new EmbeddedContentPart {
                        Id = "3",
                        Content = "This is Phrase #3",
                        Embedding = new Embedding
                        {
                            Vector = new ReadOnlyMemory<float>(Enumerable.Repeat<float>(0, 1536).ToArray())
                        }
                    }
                }
            };

            Assert.Equal(
                3,
                (await _indexingService.IndexEmbeddingsAsync(embeddedContent, _indexName)).Count
            );

            await _searchIndexClient.DeleteIndexAsync(_indexName);
        }
    }
}