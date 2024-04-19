export type Agent = {
	name: string;
	object_id: string;
	description: string;
	type: 'knowledge-management' | 'analytics';

	vectorization: {
		dedicated_pipeline: boolean;
		indexing_profile_object_id: string;
		text_embedding_profile_object_id: string;
		text_partitioning_profile_object_id: string;
		data_source_object_id: string;
		vectorization_data_pipeline_object_id: string;
		trigger_type: string;
		trigger_cron_schedule: string;
	};

	sessions_enabled: boolean;
	orchestration_settings: {
		orchestrator: string;
		endpoint_configuration: {
			endpoint: string;
			api_key: string;
			api_version: string;
			operation_type: string;
		};
		model_parameters: {
			temperature: number;
			deployment_name: string;
		};
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
	};
	prompt_object_id: string;
};

export type Prompt = {
	type: string;
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

export interface ConfigurationReferenceMetadata {
	isKeyVaultBacked: boolean;
}

// Data sources
interface BaseDataSource {
	type: string;
	name: string;
	object_id: string;
	description: string;
	configuration_references: { [key: string]: string };
	// The resolved configuration references are used to store the resolved values for displaying in the UI and updating the configuration.
	resolved_configuration_references: { [key: string]: string | null };
	// The metadata objects that specify the nature of each configuration reference.
	configuration_reference_metadata: { [key: string]: ConfigurationReferenceMetadata };
}

export interface AzureDataLakeDataSource extends BaseDataSource {
	type: 'azure-data-lake';
	folders?: string[];
	configuration_references: {
		AuthenticationType: string;
		ConnectionString: string;
		APIKey: string;
		Endpoint: string;
	};
}

export interface AzureSQLDatabaseDataSource extends BaseDataSource {
	type: 'azure-sql-database';
	tables?: string[];
	configuration_references: {
		ConnectionString: string;
	};
}

export interface SharePointOnlineSiteDataSource extends BaseDataSource {
	type: 'sharepoint-online-site';
	site_url?: string;
	document_libraries?: string[];
	configuration_references: {
		ClientId: string;
		TenantId: string;
		CertificateName: string;
		KeyVaultURL: string;
	};
}

export type DataSource =
	| AzureDataLakeDataSource
	| SharePointOnlineSiteDataSource
	| AzureSQLDatabaseDataSource;
// End data sources

// App Configuration
export interface AppConfigBase {
	type: string;
	name: string;
	object_id: string;
	display_name: string;
	description: string | null;
	key: string;
	value: string;
	content_type: string | null;
}

export interface AppConfig extends AppConfigBase {
	type: 'appconfiguration-key-value';
	content_type: '';
}

export interface AppConfigKeyVault extends AppConfigBase {
	type: 'appconfiguration-key-vault-reference';
	key_vault_uri: string;
	key_vault_secret_name: string;
	content_type: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8';
}

export type AppConfigUnion = AppConfig | AppConfigKeyVault;
// End App Configuration

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

export type CheckNameResponse = {
	type: string;
	name: string;
	status: string;
	message: string;
};

export type FilterRequest = {
	default?: boolean;
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
	};

	vectorization: {
		dedicated_pipeline: boolean;
		indexing_profile_object_id: string;
		text_embedding_profile_object_id: string;
		text_partitioning_profile_object_id: string;
		data_source_object_id: string;
		vectorization_data_pipeline_object_id: string;
		trigger_type: string;
		trigger_cron_schedule: string;
	};

	sessions_enabled: boolean;
	orchestration_settings: {
		orchestrator: string;
		endpoint_configuration: {
			endpoint: string;
			api_key: string;
			api_version: string;
			operation_type: string;
		};
		model_parameters: {
			temperature: number;
			deployment_name: string;
		};
	};
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

// Type guards
export function isAzureDataLakeDataSource(
	dataSource: DataSource,
): dataSource is AzureDataLakeDataSource {
	return dataSource.type === 'azure-data-lake';
}

export function isSharePointOnlineSiteDataSource(
	dataSource: DataSource,
): dataSource is SharePointOnlineSiteDataSource {
	return dataSource.type === 'sharepoint-online-site';
}

export function isAzureSQLDatabaseDataSource(
	dataSource: DataSource,
): dataSource is AzureSQLDatabaseDataSource {
	return dataSource.type === 'azure-sql-database';
}

export function isAppConfig(config: AppConfigUnion): config is AppConfig {
	return config.type === 'appconfiguration-key-value';
}

export function isAppConfigKeyVault(config: AppConfigUnion): config is AppConfigKeyVault {
	return config.type === 'appconfiguration-key-vault-reference';
}

export function convertDataSourceToAzureDataLake(dataSource: DataSource): AzureDataLakeDataSource {
	return {
		type: 'azure-data-lake',
		name: dataSource.name,
		object_id: dataSource.object_id,
		description: dataSource.description,
		folders: dataSource.folders || [],
		configuration_references: {
			AuthenticationType: dataSource.configuration_references?.AuthenticationType || '',
			ConnectionString: dataSource.configuration_references?.ConnectionString || '',
			APIKey: dataSource.configuration_references?.APIKey || '',
			Endpoint: dataSource.configuration_references?.Endpoint || '',
		},
		configuration_reference_metadata: {
			AuthenticationType: { isKeyVaultBacked: false },
			ConnectionString: { isKeyVaultBacked: true },
			APIKey: { isKeyVaultBacked: true },
			Endpoint: { isKeyVaultBacked: false },
		},
		resolved_configuration_references: dataSource.resolved_configuration_references,
	};
}

export function convertDataSourceToSharePointOnlineSite(
	dataSource: DataSource,
): SharePointOnlineSiteDataSource {
	return {
		type: 'sharepoint-online-site',
		name: dataSource.name,
		object_id: dataSource.object_id,
		description: dataSource.description,
		site_url: dataSource.site_url || '',
		document_libraries: dataSource.document_libraries || [],
		configuration_references: {
			ClientId: dataSource.configuration_references?.ClientId || '',
			TenantId: dataSource.configuration_references?.TenantId || '',
			CertificateName: dataSource.configuration_references?.CertificateName || '',
			KeyVaultURL: dataSource.configuration_references?.KeyVaultURL || '',
		},
		configuration_reference_metadata: {
			ClientId: { isKeyVaultBacked: false },
			TenantId: { isKeyVaultBacked: false },
			CertificateName: { isKeyVaultBacked: false },
			KeyVaultURL: { isKeyVaultBacked: false },
		},
		resolved_configuration_references: dataSource.resolved_configuration_references,
	};
}

export function convertDataSourceToAzureSQLDatabase(
	dataSource: DataSource,
): AzureSQLDatabaseDataSource {
	return {
		type: 'azure-sql-database',
		name: dataSource.name,
		object_id: dataSource.object_id,
		description: dataSource.description,
		tables: dataSource.tables || [],
		configuration_references: {
			ConnectionString: dataSource.configuration_references?.ConnectionString || '',
		},
		configuration_reference_metadata: {
			ConnectionString: { isKeyVaultBacked: true },
		},
		resolved_configuration_references: dataSource.resolved_configuration_references,
	};
}

export function convertToDataSource(dataSource: DataSource): DataSource {
	if (isAzureDataLakeDataSource(dataSource)) {
		return convertDataSourceToAzureDataLake(dataSource);
	} else if (isSharePointOnlineSiteDataSource(dataSource)) {
		return convertDataSourceToSharePointOnlineSite(dataSource);
	} else if (isAzureSQLDatabaseDataSource(dataSource)) {
		return convertDataSourceToAzureSQLDatabase(dataSource);
	}
	return dataSource;
}

export function convertToAppConfig(baseConfig: AppConfigUnion): AppConfig {
	return {
		...baseConfig,
		type: 'appconfiguration-key-value',
		content_type: '',
	};
}

export function convertToAppConfigKeyVault(baseConfig: AppConfigUnion): AppConfigKeyVault {
	if (!('key_vault_uri' in baseConfig) || !('key_vault_secret_name' in baseConfig)) {
		throw new Error('Missing Key Vault properties');
	}

	return {
		...baseConfig,
		type: 'appconfiguration-key-vault-reference',
		key_vault_uri: baseConfig.key_vault_uri,
		key_vault_secret_name: baseConfig.key_vault_secret_name,
		content_type: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8',
	};
}
