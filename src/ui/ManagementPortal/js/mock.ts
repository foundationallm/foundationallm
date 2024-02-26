import type { AgentIndex } from './types';

export const mockGetAgentIndexesResponse: AgentIndex[] = [
	{
		name: 'AzureAISearch_Test_001',
		object_id: '47893247',
		description: 'Azure AI Search index for vectorization testing.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'fllm-test-001',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		name: 'sotu-index',
		object_id: '25637942',
		description: 'Azure AI Search index for the State of the Union agent.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'sotu',
			TopN: '3',
			Filters: '[]',
			EmbeddingFieldName: 'Embedding',
			TextFieldName: 'Text',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
];

export const mockGetAgentDataSourcesResponse: AgentIndex[] = [
	{
		name: 'AzureBlob_DataSource_1',
		object_id: '90871234981',
		Type: 'AzureDataLake',
		Container: {
			Name: 'documents',
			Formats: [
				'pdf',
			],
		},
		description: 'Azure AI Search index for vectorization testing.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'fllm-test-001',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		name: 'AzureBlob_DataSource_2',
		object_id: '8931729038',
		Type: 'SharePointOnline',
		Container: {
			Name: 'census_data',
			Formats: [
				'pdf',
				'txt',
				'doc',
			],
		},
		description: 'Azure AI Search index for the State of the Union agent.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'sotu',
			TopN: '3',
			Filters: '[]',
			EmbeddingFieldName: 'Embedding',
			TextFieldName: 'Text',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		name: 'AzureBlob_DataSource_3',
		object_id: '12873989',
		Type: 'AzureDataLake',
		Container: {
			Name: 'data',
			Formats: [
				'txt',
			],
		},
	}
];
