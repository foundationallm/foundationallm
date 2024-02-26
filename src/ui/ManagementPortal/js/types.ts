export type Agent = {
	name: string;
	object_id: string;
	description: string;
	type: 'knowledge-management' | 'analytics';
	indexing_profile_object_id: string;
	text_embedding_profile_object_id: string;
	text_partitioning_profile_object_id: string;
	content_source_profile_object_id: string;

	sessions_enabled: boolean;
	orchestrator: string;
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
	language_model: {
		type: string;
		provider: string;
		temperature: number;
		use_chat: boolean;
		api_endpoint: string;
		api_key: string;
		api_version: string;
		version: string;
		deployment: string;
	}
	prompt_object_id: string;
};

export type Prompt = {
	type: string
	name: string;
	object_id: string;
	description: string;
	prefix: string;
	suffix: string;
};

export type AgentDataSource = {
	name: string;
	content_source: string;
	object_id: string;
};

export type AgentIndex = {
	name: string;
	object_id: string;
	description: string;
	indexer: string;
	settings: {
		IndexName: string;
		TopN?: string;
		Filters?: string;
		EmbeddingFieldName?: string;
		TextFieldName?: string;
	};
	configuration_references: {
		APIKey: string;
		AuthenticationType: string;
		Endpoint: string;
	};
};

export type TextPartitioningProfile = {
	text_splitter: string;
	name: string;
	object_id: string;
	settings: {
		Tokenizer: string;
		TokenizerEncoder: string;
		ChunkSizeTokens: string;
		OverlapSizeTokens: string;
	};
};

export type TextEmbeddingProfile = {
	type: string;
	text_embedding: string;
	name: string;
	object_id: string;
	configuration_references: {
		APIKey: string;
		APIVersion: string;
		AuthenticationType: string;
		DeploymentName: string;
		Endpoint: string;
	};
};

export type AgentCheckNameResponse = {
    type: string;
    name: string;
    status: string;
    message: string;
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

export type CreateAgentRequest = {
	type: 'knowledge-management' | 'analytics';
	name: string;
	description: string;
	object_id: string;
	language_model: {
		type: string;
		provider: string;
		temperature: number;
		use_chat: boolean;
		api_endpoint: string;
		api_key: string;
		api_version: string;
		version: string;
		deployment: string;
	}
	indexing_profile_object_id: string;
	text_embedding_profile_object_id: string;
	content_source_profile_object_id: string;
	text_partitioning_profile_object_id: string;
	sessions_enabled: boolean;
	orchestrator: string;
	conversation_history: {
		enabled: boolean;
		max_history: number;
	};
	gatekeeper: {
		use_system_setting: boolean;
		options: string[];
	};
	prompt_object_id: string;
};

export type CreatePromptRequest = {
	type: 'basic' | 'multipart';
	name: string;
	object_id: string;
	description: string;
	prefix: string;
	suffix: string;
};

export type CreateTextPartitioningProfileRequest = {
	text_splitter: string;
	name: string;
	object_id: string;
	settings: {
		Tokenizer: string;
		TokenizerEncoder: string;
		ChunkSizeTokens: string;
		OverlapSizeTokens: string;
	};
};
