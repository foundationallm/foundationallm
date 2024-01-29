export type AgentDataSource = {};

export type AgentIndex = {
	Name: string;
	Description: string;
	Indexer: string;
	Settings: {
		IndexName: string;
		TopN?: string;
		Filters?: string;
		EmbeddingFieldName?: string;
		TextFieldName?: string;
	};
	ConfigurationReferences: {
		APIKey: string;
		AuthenticationType: string;
		Endpoint: string;
	};
};

export type AgentGatekeeper = {};

export type MockCreateAgentRequest = {
	type: 'knowledge' | 'analytics';
	storageSource: number;
	indexSource: number;
	processing: {
		chunkSize: number;
		overlapSize: number;
	};
	trigger: {
		frequency: 'auto' | 'manual' | 'scheduled';
	};
	conversation_history: {
		enabled: boolean;
		max_history: number;
	};
	gatekeeper: {
		use_system_setting: boolean;
		options: {
			content_safety: number;
			data_protection: number;
		};
	};
	prompt: string;
};
