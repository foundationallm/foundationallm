/* eslint-disable prettier/prettier */

import type {
	Agent,
	DataSource,
	AppConfigUnion,
	AgentIndex,
	AgentGatekeeper,
	CreateAgentRequest,
	CheckNameResponse,
	Prompt,
	TextPartitioningProfile,
	TextEmbeddingProfile,
	CreatePromptRequest,
	CreateTextPartitioningProfileRequest
} from './types';
import {
	isAzureDataLakeDataSource,
	isSharePointOnlineSiteDataSource,
	isAzureSQLDatabaseDataSource,
	convertDataSourceToAzureDataLake,
	convertDataSourceToSharePointOnlineSite,
	convertDataSourceToAzureSQLDatabase,
	convertToDataSource,
	convertToAppConfigKeyVault,
	convertToAppConfig
} from '@/js/types';
// import { mockAzureDataLakeDataSource1 } from './mock';
import { getMsalInstance } from '@/js/auth';


async function wait(milliseconds: number = 1000): Promise<void> {
	return await new Promise<void>((resolve) => setTimeout(() => resolve(), milliseconds));
}

export default {
	mockLoadTime: 1000,

	apiVersion: '2024-02-16',
	apiUrl: null as string | null,
	setApiUrl(apiUrl: string) {
		this.apiUrl = apiUrl;
	},

	instanceId: null as string | null,
	setInstanceId(instanceId: string) {
		this.instanceId = instanceId;
	},

  bearerToken: null,
	async getBearerToken() {
		if (this.bearerToken) return this.bearerToken;

		const msalInstance = await getMsalInstance();
		const accounts = msalInstance.getAllAccounts();
		const account = accounts[0];
		const bearerToken = await msalInstance.acquireTokenSilent({ account });

		this.bearerToken = bearerToken.accessToken;
		return this.bearerToken;
	},

	async fetch(url: string, opts: any = {}) {
		const options = opts;
		options.headers = opts.headers || {};

		// if (options?.query) {
		// 	url += '?' + (new URLSearchParams(options.query)).toString();
		// }

		const bearerToken = await this.getBearerToken();
		options.headers['Authorization'] = `Bearer ${bearerToken}`;

		return await $fetch(`${this.apiUrl}${url}`, options);
	},

	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key
			}
		});
	},

	// Data sources
	async checkDataSourceName(name: string, type: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type: type,
		};

		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/checkname?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: payload,
		});
	},
	
	async getAgentDataSources(): Promise<DataSource[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources?api-version=${this.apiVersion}`) as DataSource[];
	},

	async getDataSource(dataSourceId: string): Promise<DataSource> {
		const data = await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSourceId}?api-version=${this.apiVersion}`);
		let dataSource = data[0] as DataSource;
		dataSource.resolved_configuration_references = {};
		// Retrieve all the app config values for the data source.
		const appConfigFilter = `FoundationaLLM:DataSources:${dataSource.name}:*`;
		const appConfigs = await this.getAppConfigs(appConfigFilter);

		// If set the resolved_configuration_references property on the data source with the app config values.
		if (appConfigs) {
			for (const appConfig of appConfigs) {
				const propertyName = appConfig.name.split(':').pop();
				dataSource.resolved_configuration_references[propertyName as string] = String(appConfig.value);
			}
		}
		else {
			for (const [configName, configValue] of Object.entries(dataSource.configuration_references)) {
				const resolvedValue = await this.getAppConfig(dataSource.configuration_references[configName as keyof typeof dataSource.configuration_references]);
				if (resolvedValue) {
					dataSource.resolved_configuration_references[configName] = String(resolvedValue.value);
				}
				else {
					dataSource.resolved_configuration_references[configName] = '';
				}
			}
		}
		dataSource = convertToDataSource(dataSource);
		return dataSource;
	},

	async upsertDataSource(request): Promise<any> {
		const dataSource = convertToDataSource(request);
		for (const [propertyName, propertyValue] of Object.entries(dataSource.resolved_configuration_references || {})) {
			if (!propertyValue) {
				continue;
			}

			const appConfigKey = `FoundationaLLM:DataSources:${dataSource.name}:${propertyName}`;
			const keyVaultSecretName = `foundationallm-datasources-${dataSource.name}-${propertyName}`.toLowerCase();
			const metadata = dataSource.configuration_reference_metadata?.[propertyName];

			let keyVaultUri = await this.getAppConfig('FoundationaLLM:Configuration:KeyVaultURI');

			let appConfig: AppConfigUnion = {
				name: appConfigKey,
				display_name: appConfigKey,
				description: '',
				key: appConfigKey,
				value: propertyValue,
			};

			if (metadata && metadata.isKeyVaultBacked) {
				appConfig = convertToAppConfigKeyVault({
					...appConfig,
					key_vault_uri: keyVaultUri.value,
					key_vault_secret_name: keyVaultSecretName,
				});
			} else {
				appConfig = convertToAppConfig(appConfig);
			}
	
			await this.upsertAppConfig(appConfig);
	
			dataSource.configuration_references[propertyName] = appConfigKey;
		}

		// Remove any any configuration_references whose values are null or empty strings.
		for (const [propertyName, propertyValue] of Object.entries(dataSource.configuration_references)) {
			if (!propertyValue) {
				delete dataSource.configuration_references[propertyName];
			}
		}
	
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSource.name}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: JSON.stringify(dataSource),
			headers: {
				'Content-Type': 'application/json',
			},
		});
	},
	

	async deleteDataSource(dataSourceId: string): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSourceId}?api-version=${this.apiVersion}`, {
			method: 'DELETE',
		});
	},

	// App Configuration
	async getAppConfig(key: string): Promise<AppConfigUnion> {
		// await wait(this.mockLoadTime);
    	// return mockAzureDataLakeDataSource1;
		const data = await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${key}?api-version=${this.apiVersion}`);
		return data[0] as AppConfigUnion;
	},

	async getAppConfigs(filter?: string): Promise<AppConfigUnion[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${filter}?api-version=${this.apiVersion}`);
	},

	async upsertAppConfig(request): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${request.key}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},

	// Indexes
	async getAgentIndexes(): Promise<AgentIndex[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/indexingProfiles?api-version=${this.apiVersion}`);
	},

	// Text embedding profiles
	async getTextEmbeddingProfiles(): Promise<TextEmbeddingProfile[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles?api-version=${this.apiVersion}`);
	},

	// Agents
	async checkAgentName(name: string, agentType: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type: agentType,
		};

		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/checkname?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: payload,
		});
	},

	async getAgents(): Promise<Agent[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents?api-version=${this.apiVersion}`);
	},

	async getAgent(agentId: string): Promise<any> {
		const data = await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`);
		return data[0];
	},

	async updateAgent(agentId: string, request: CreateAgentRequest): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},

	async createAgent(request: CreateAgentRequest): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${request.name}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},

	async deleteAgent(agentId: string): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`, {
			method: 'DELETE',
		});
	},

	async getAgentGatekeepers(): Promise<AgentGatekeeper[]> {
		await wait(this.mockLoadTime);
		return [];
	},

	// Prompts
	async getPrompt(promptId: string): Promise<Prompt> {
		const data = await this.fetch(`${promptId}?api-version=${this.apiVersion}`);
		return data[0];
	},

	async createOrUpdatePrompt(agentId: string, request: CreatePromptRequest): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${agentId}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},

	async getTextPartitioningProfile(profileId: string): Promise<TextPartitioningProfile> {
		const data = await this.fetch(`${profileId}?api-version=${this.apiVersion}`);
		return data[0];
	},

	async createOrUpdateTextPartitioningProfile(agentId: string, request: CreateTextPartitioningProfileRequest): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/${agentId}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},
}
