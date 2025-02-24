﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class IndexingProfilesCatalog
    {
        public static readonly List<IndexingProfile> Items =
        [
            new IndexingProfile { Name = "indexing_profile_really_big", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "index_name", "reallybig" }, { "top_n", "3" }, { "filters", "" }, { "embedding_field_name", "Embedding" }, { "text_field_name", "Text" }, { VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, TestAPIEndpointConfigurationNames.DefaultAzureAISearch } } },
            new IndexingProfile { Name = "indexing_profile_pdf", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "index_name", "pdf" }, { "top_n", "3" }, { "filters", "" }, { "embedding_field_name", "Embedding" }, { "text_field_name", "Text" }, { VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, TestAPIEndpointConfigurationNames.DefaultAzureAISearch} } },
            new IndexingProfile { Name = "indexing_profile_sdzwa", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "index_name", "fllm-pdf" }, { "top_n", "3" }, { "filters", "" }, { "embedding_field_name", "Embedding" }, { "text_field_name", "Text" }, { VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, TestAPIEndpointConfigurationNames.DefaultAzureAISearch} } },
            new IndexingProfile { Name = "indexing_profile_dune", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string> { { "index_name", "fllm-dune" }, { "top_n", "3" }, { "filters", "" }, { "embedding_field_name", "Embedding" }, { "text_field_name", "Text" }, { VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, TestAPIEndpointConfigurationNames.DefaultAzureAISearch} } }
        ];

        public static List<IndexingProfile> GetIndexingProfiles()
        {
            var items = new List<IndexingProfile>();
            items.AddRange(Items);
            return items;
        }
    }
}
