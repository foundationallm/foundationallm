import { $fetch } from 'ofetch';
import type {
	ResourceProviderGetResult,
	Agent,
	DataSource,
	AppConfigUnion,
	// AppConfigKeyVault,
	AgentIndex,
	// AgentGatekeeper,
	AgentAccessToken,
	ResourceProviderUpsertResult,
	ResourceProviderActionResult,
	AIModel,
	FilterRequest,
	CreateAgentRequest,
	CheckNameResponse,
	Prompt,
	TextPartitioningProfile,
	TextEmbeddingProfile,
	CreatePromptRequest,
	CreateTextPartitioningProfileRequest,
	ExternalOrchestrationService,
	// Role,
	RoleAssignment,
	APIEndpointConfiguration,
	FileToolAssociation,
	UpdateAgentFileToolAssociationRequest,
	Workflow,
	AgentTool,
} from './types';
import { convertToDataSource, convertToAppConfigKeyVault, convertToAppConfig } from '@/js/types';
// import { isEmpty, upperFirst, camelCase } from 'lodash';
// async function wait(milliseconds: number = 1000): Promise<void> {
// 	return await new Promise<void>((resolve) => setTimeout(() => resolve(), milliseconds));
// }

export default {
	apiVersion: '2024-02-16',
	apiUrl: null as string | null,
	setApiUrl(apiUrl: string) {
		this.apiUrl = apiUrl;
	},

	instanceId: null as string | null,
	setInstanceId(instanceId: string) {
		this.instanceId = instanceId;
	},

	/**
	 * Retrieves the bearer token for authentication.
	 * If the bearer token is already available, it will be returned immediately.
	 * Otherwise, it will acquire a new bearer token using the MSAL instance.
	 * @returns The bearer token.
	 */
	async getBearerToken() {
		// When the scope is specific on aquireTokenSilent this seems to be instant
		// otherwise we would have to store the token and check if it has expired here
		// to determine if we need to fetch it again
		const token = await useNuxtApp().$authStore.getApiToken();
		return token.accessToken;
	},

	async fetch(url: string, opts: any = {}) {
		const options = opts;
		options.headers = opts.headers || {};

		// if (options?.query) {
		// 	url += '?' + (new URLSearchParams(options.query)).toString();
		// }

		const bearerToken = await this.getBearerToken();
		options.headers.Authorization = `Bearer ${bearerToken}`;

		return await $fetch(`${this.apiUrl}${url}`, options);
	},

	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key,
			},
		});
	},

	/**
	 * Retrieves the ManagementPortal app configuration set from the management endpoint.
	 * @returns A promise that resolves to the ManagementPortal app configuration set.
	 */
	async getManagementPortalAppConfigurationSet() {
		return await this.fetch<ResourceProviderGetResult<any>[]>(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurationSets/ManagementPortal`
		);
	},

	/*
		Data Sources
	 */
	async checkDataSourceName(name: string, type: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	async getDefaultDataSource(): Promise<DataSource | null> {
		const payload: FilterRequest = {
			default: true,
		};

		const data = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);

		if (data && data.length > 0) {
			return data[0] as DataSource;
		} else {
			return null;
		}
	},

	async getAgentDataSources(
		addDefaultOption: boolean = false,
	): Promise<ResourceProviderGetResult<DataSource>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<DataSource>[];
		if (addDefaultOption) {
			const defaultDataSource: DataSource = {
				name: 'Select default data source',
				type: 'DEFAULT',
				object_id: '',
				resolved_configuration_references: {},
				configuration_references: {},
			};
			const defaultDataSourceResult: ResourceProviderGetResult<DataSource> = {
				resource: defaultDataSource,
				actions: [],
				roles: [],
			};
			data.unshift(defaultDataSourceResult);
		}
		return data;
	},

	async getDataSource(dataSourceId: string): Promise<ResourceProviderGetResult<DataSource>> {
		const [data] = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSourceId}?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<DataSource>[];
		let dataSource = data.resource as DataSource;
		dataSource.resolved_configuration_references = {};
		// Retrieve all the app config values for the data source.
		const appConfigFilter = `FoundationaLLM:DataSources:${dataSource.name}:*`;
		const appConfigResults = await this.getAppConfigs(appConfigFilter);

		// If set the resolved_configuration_references property on the data source with the app config values.
		if (appConfigResults) {
			for (const appConfigResult of appConfigResults) {
				const appConfig = appConfigResult.resource;
				const propertyName = appConfig.name.split(':').pop();
				dataSource.resolved_configuration_references[propertyName as string] = String(
					appConfig.value,
				);
			}
		} else {
			for (const [configName /* configValue */] of Object.entries(
				dataSource.configuration_references,
			)) {
				const resolvedValue = await this.getAppConfig(
					dataSource.configuration_references[
						configName as keyof typeof dataSource.configuration_references
					],
				);
				if (resolvedValue) {
					dataSource.resolved_configuration_references[configName] = String(resolvedValue.value);
				} else {
					dataSource.resolved_configuration_references[configName] = '';
				}
			}
		}
		dataSource = convertToDataSource(dataSource);
		data.resource = dataSource;
		return data;
	},

	async upsertDataSource(request): Promise<any> {
		const dataSource = convertToDataSource(request);
		for (const [propertyName, propertyValue] of Object.entries(
			dataSource.resolved_configuration_references || {},
		)) {
			if (!propertyValue) {
				continue;
			}

			const appConfigKey = `FoundationaLLM:DataSources:${dataSource.name}:${propertyName}`;
			const keyVaultSecretName =
				`foundationallm-datasources-${dataSource.name}-${propertyName}`.toLowerCase();
			const metadata = dataSource.configuration_reference_metadata?.[propertyName];

			const appConfigResult = await this.getAppConfig('FoundationaLLM:Configuration:KeyVaultURI');
			const keyVaultUri = appConfigResult.resource;

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
		for (const [propertyName, propertyValue] of Object.entries(
			dataSource.configuration_references,
		)) {
			if (!propertyValue) {
				delete dataSource.configuration_references[propertyName];
			}
		}

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSource.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(dataSource),
				headers: {
					'Content-Type': 'application/json',
				},
			},
		);
	},

	async deleteDataSource(dataSourceId: string): Promise<void> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSourceId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	/*
		App Configuration
	 */
	async getAppConfig(key: string): Promise<ResourceProviderGetResult<AppConfigUnion>> {
		const data = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${key}?api-version=${this.apiVersion}`,
		);
		return data[0] as ResourceProviderGetResult<AppConfigUnion>;
	},

	async getAppConfigs(filter?: string): Promise<ResourceProviderGetResult<AppConfigUnion>[]> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${filter}?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AppConfigUnion>[];
	},

	async upsertAppConfig(request: AppConfigUnion): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${request.key}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	// async deleteAppConfig(appConfigKey: string): Promise<void> {
	// 	return await this.fetch(
	// 		`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${appConfigKey}?api-version=${this.apiVersion}`,
	// 		{
	// 			method: 'DELETE',
	// 		},
	// 	);
	// },

	/*
		Agents
	 */
	async checkAgentName(name: string, agentType: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type: agentType,
		};

		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		)) as CheckNameResponse;
	},

	async getAgents(): Promise<ResourceProviderGetResult<Agent>[]> {
		const agents = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<Agent>[];
		// Sort the agents by name.
		agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));
		return agents;
	},

	async getAgent(agentId: string): Promise<ResourceProviderGetResult<Agent>> {
		const [agentGetResult]: ResourceProviderGetResult<Agent>[] = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`,
		);

		return agentGetResult;
	},

	async upsertAgent(agentId: string, agentData: CreateAgentRequest): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: agentData,
			},
		);
	},

	async createAgent(request: CreateAgentRequest): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${request.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	async deleteAgent(agentId: string): Promise<void> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	async setDefaultAgent(agentId: string): Promise<ResourceProviderActionResult> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}/set-default?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: {},
			},
		)) as ResourceProviderActionResult;
	},

	/*
		Prompts
	 */
	async checkPromptName(name: string, promptType: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type: promptType,
		};

		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		)) as CheckNameResponse;
	},

	async getPrompts(): Promise<ResourceProviderGetResult<Prompt>[] | null> {
		try {
			const data = await this.fetch(
				`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts?api-version=${this.apiVersion}`,
			);
			return data as ResourceProviderGetResult<Prompt>[];
		} catch (error) {
			return null;
		}
	},

	async getPromptByName(promptName: string): Promise<ResourceProviderGetResult<Prompt> | null> {
		// Attempt to retrieve the prompt. If it doesn't exist, return an empty object.
		try {
			const data = await this.fetch(
				`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${promptName}?api-version=${this.apiVersion}`,
			);
			return data[0];
		} catch (error) {
			return null;
		}
	},

	async getPrompt(promptId: string): Promise<ResourceProviderGetResult<Prompt> | null> {
		// Attempt to retrieve the prompt. If it doesn't exist, return an empty object.
		try {
			const data = await this.fetch(`${promptId}?api-version=${this.apiVersion}`);
			return data[0];
		} catch (error) {
			return null;
		}
	},

	async createOrUpdatePrompt(promptName: string, request: CreatePromptRequest): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${promptName}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	async getTextPartitioningProfile(
		profileId: string,
	): Promise<ResourceProviderGetResult<TextPartitioningProfile>> {
		const data = await this.fetch(`${profileId}?api-version=${this.apiVersion}`);
		return data[0];
	},

	async createOrUpdateTextPartitioningProfile(
		agentId: string,
		request: CreateTextPartitioningProfileRequest,
	): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	async getBranding(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/FoundationaLLM:Branding:*`,
		);
	},

	async saveBranding(key: String, params: any): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${key}`,
			{
				method: 'POST',
				body: params,
			},
		);
	},

	/*
		API Endpoints Configurations
	 */
	async getAPIEndpointConfigurations(): Promise<
		ResourceProviderGetResult<APIEndpointConfiguration>[]
	> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<APIEndpointConfiguration>[];

		return data;
	},

	async getAPIEndpointConfiguration(
		apiEndpointName: string,
	): Promise<ResourceProviderGetResult<APIEndpointConfiguration>> {
		const [data] = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/${apiEndpointName}?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<APIEndpointConfiguration>[];

		// data.resource.resolved_authentication_parameters = {};

		// // Attempt to load the real authentication parameter values if they are stored in app config
		// for (const authenticationParameterKey in data.resource.authentication_parameters) {
		// 	const authenticationParameterValue =
		// 		data.resource.authentication_parameters[authenticationParameterKey];
		// 	const appConfigValue = await this.getAppConfig(authenticationParameterValue);

		// 	if (appConfigValue) {
		// 		data.resource.resolved_authentication_parameters[authenticationParameterKey] = {
		// 			secret: true,
		// 			value: appConfigValue.resource.value,
		// 		};
		// 	} else {
		// 		data.resource.resolved_authentication_parameters[authenticationParameterKey] = {
		// 			secret: false,
		// 			value: authenticationParameterValue,
		// 		};
		// 	}
		// }

		return data;
	},

	async createAPIEndpointConfiguration(apiEndpoint: any): Promise<ResourceProviderUpsertResult> {
		// const authenticationParameters = {};

		// if (!isEmpty(apiEndpoint.resolved_authentication_parameters)) {
		// 	// Convert secret authentication values into app config values and store app config key as value instead
		// 	for (const authenticationParameterKey in apiEndpoint.resolved_authentication_parameters) {
		// 		const authenticationParameterValue =
		// 			apiEndpoint.resolved_authentication_parameters[authenticationParameterKey];

		// 		if (authenticationParameterValue.secret) {
		// 			const parameterKeyPascalCase = upperFirst(camelCase(authenticationParameterKey));
		// 			var appConfigKey = `FoundationaLLM:APIEndpoints:${apiEndpoint.name}:Essentials:${parameterKeyPascalCase}`;
		// 			let appConfig: AppConfigKeyVault = {
		// 				name: appConfigKey,
		// 				display_name: appConfigKey,
		// 				description: '',
		// 				key: appConfigKey,
		// 				value: authenticationParameterValue.value,
		// 				content_type: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8',
		// 			};

		// 			const appConfigResult = await this.getAppConfig(
		// 				'FoundationaLLM:Configuration:KeyVaultURI',
		// 			);
		// 			const keyVaultUri = appConfigResult.resource;
		// 			const keyVaultSecretName =
		// 				`foundationallm-apiendpoints-${apiEndpoint.name}-${parameterKeyPascalCase}`.toLowerCase();

		// 			appConfig = {
		// 				...appConfig,
		// 				key_vault_uri: keyVaultUri.value,
		// 				key_vault_secret_name: keyVaultSecretName,
		// 			};

		// 			await this.upsertAppConfig(appConfig);

		// 			authenticationParameters[authenticationParameterKey] = appConfigKey;
		// 		} else {
		// 			authenticationParameters[authenticationParameterKey] = authenticationParameterValue.value;
		// 		}
		// 	}
		// }

		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/${apiEndpoint.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: {
					type: 'api-endpoint',
					...apiEndpoint,
					// resolved_authentication_parameters: undefined,
					// authentication_parameters: authenticationParameters,
				},
			},
		)) as ResourceProviderUpsertResult;

		return data;
	},

	async deleteAPIEndpointConfiguration(apiEndpointName: string): Promise<void> {
		const response = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/${apiEndpointName}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);

		// Try to delete the APIKey associated with the endpoint as well
		// try {
		// 	const appConfigKey = `FoundationaLLM:APIEndpoints:${apiEndpointName}:Essentials:APIKey`;
		// 	await this.deleteAppConfig(appConfigKey);
		// } catch (error) {
		// 	console.error('Failed to delete app config APIKey associated to endpoint.');
		// }

		return response;
	},

	async checkAPIEndpointConfigurationName(name: string): Promise<CheckNameResponse> {
		const payload = {
			name,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	/*
		AI Models
	 */
	async getAIModels(): Promise<ResourceProviderGetResult<AIModel>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AIModel>[];

		return data;
	},

	async getAIModel(aiModelName: string): Promise<ResourceProviderGetResult<AIModel>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels/${aiModelName}?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AIModel>[];

		return data;
	},

	async createAIModel(aiModel: CreateAgentRequest): Promise<ResourceProviderUpsertResult[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels/${aiModel.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: aiModel,
			},
		)) as ResourceProviderUpsertResult;

		return data;
	},

	async upsertAIModel(
		aiModelOriginalName: string,
		aiModel: CreateAgentRequest,
	): Promise<ResourceProviderUpsertResult> {
		const aiModelNoType = { ...aiModel };
		delete aiModelNoType.type;

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels/${aiModelOriginalName}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: {
					// Type must be the first property sent in the payload in order to succeed
					type: aiModel.type,
					...aiModelNoType,
				},
			},
		);
	},

	async deleteAIModel(aiModelName: string): Promise<void> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels/${aiModelName}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	async checkAIModelName(name: string): Promise<CheckNameResponse> {
		const payload = {
			name,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	/*
		Role Assignments
	 */
	async getRoleAssignments(scope: string): Promise<ResourceProviderGetResult<RoleAssignment>[]> {
		const assignments = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					scope: `/instances/${this.instanceId}${scope ? `/${scope}` : ''}`,
				}),
			},
		)) as ResourceProviderGetResult<RoleAssignment>[];

		assignments.forEach((assignment) => {
			if (assignment.resource.scope === `/instances/${this.instanceId}`) {
				assignment.resource.scope_name = scope ? 'Instance (Inherited)' : 'Instance';
			} else if (assignment.resource.scope === `/instances/${this.instanceId}/${scope}`) {
				assignment.resource.scope_name = 'This resource';
			}
		});

		return assignments;
	},

	async getRoleAssignment(roleAssignmentId: string): Promise<RoleAssignment[]> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${roleAssignmentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					scope: `/instances/${this.instanceId}`,
				}),
			},
		)) as RoleAssignment[];
	},

	async createRoleAssignment(request: any): Promise<ResourceProviderUpsertResult> {
		if (!request.scope) {
			request.scope = `/instances/${this.instanceId}`;
		}

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${request.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(request),
			},
		);
	},

	async deleteRoleAssignment(roleAssignmentId: string): Promise<void> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${roleAssignmentId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	/*
		Role Definitions
	 */
	async getRoleDefinitions(): RoleAssignment[] {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions?api-version=${this.apiVersion}`,
		)) as Object[];
	},

	async getRoleDefinition(roleAssignmentId): RoleAssignment {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions/${roleAssignmentId}?api-version=${this.apiVersion}`,
		)) as RoleAssignment[];
	},

	/*
		Users
	 */
	async getUsers(params) {
		const defaults = {
			name: '',
			ids: [],
			page_number: 1,
			page_size: null,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/identity/users/retrieve?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					...defaults,
					...params,
				}),
			},
		);
	},

	async getUser(userId) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/users/${userId}?api-version=${this.apiVersion}`,
		);
	},

	/*
		Groups
	 */
	async getGroups(params) {
		const defaults = {
			name: '',
			ids: [],
			page_number: 1,
			page_size: null,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/identity/groups/retrieve?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					...defaults,
					...params,
				}),
			},
		);
	},

	async getGroup(groupId) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/groups/${groupId}?api-version=${this.apiVersion}`,
		);
	},

	/*
		Service Principals
	 */
	async getServicePrincipals(params) {
		const defaults = {
			name: '',
			ids: [],
			page_number: 1,
			page_size: null,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/identity/serviceprincipals/retrieve?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					...defaults,
					...params,
				}),
			},
		);
	},

	async getServicePrincipal(servicePrincipalId) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/serviceprincipals/${servicePrincipalId}?api-version=${this.apiVersion}`,
		);
	},

	/*
		Combined User + Groups + Service Principals
	 */
	async getObjects(params = { ids: [] }) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/objects/retrievebyids?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(params),
			},
		);
	},

	/*
		Private Storage
	 */
	async getPrivateStorageFiles(agentName: string) {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles?api-version=${this.apiVersion}`,
		)) as Object[];
	},

	async uploadToPrivateStorage(agentName: string, fileName: string, file: FormData): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles/${fileName}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: file,
			},
		);
	},

	async deleteFileFromPrivateStorage(agentName: string, fileName: string): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles/${fileName}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	async getPrivateStorageFileToolAssociations(agentName: string) {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFileToolAssociations?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<FileToolAssociation>[];
	},

	async updateFileToolAssociations(
		agentName: string,
		payload: UpdateAgentFileToolAssociationRequest,
	): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFileToolAssociations/__all__?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(payload),
			},
		);
	},

	/*
		Agent Workflows
	 */
	async getAgentWorkflows(): Promise<ResourceProviderGetResult<Workflow>[]> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/workflows?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<Workflow>[];
	},

	/*
		Agent Tools
	 */
	async getAgentTools(): Promise<ResourceProviderGetResult<AgentTool>[]> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/tools?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AgentTool>[];
	},

	/*
		Agent Access Tokens
	 */
	async getAgentAccessTokens(agentName: string) {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentAccessTokens?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AgentAccessToken>[];
	},

	async createAgentAccessToken(
		agentName: string,
		body: AgentAccessToken,
	): Promise<ResourceProviderUpsertResult> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentAccessTokens/${body.id}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body,
			},
		)) as ResourceProviderUpsertResult;
	},

	async deleteAgentAccessToken(agentName: string, accessTokenId: string): Promise<void> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentAccessTokens/${accessTokenId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	async getOrchestrationServices(): Promise<APIEndpointConfiguration> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations?api-version=${this.apiVersion}`,
		)) as APIEndpointConfiguration;
	},

	async getExternalOrchestrationServices(
		resolveApiKey: boolean = false,
	): Promise<ResourceProviderGetResult<ExternalOrchestrationService>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<ExternalOrchestrationService>[];

		// Return the updated external orchestration services.
		return data;
	},

	/*
		Pipelines
	*/
	// async getPipelines(): Promise<any> {
	// 	return await this.fetch(
	// 		`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/vectorizationPipelines?api-version=${this.apiVersion}`,
	// 	);
	// },

	// async getPipeline(pipeline: string): Promise<any> {
	// 	return await this.fetch(
	// 		`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/vectorizationPipelines/${pipeline}?api-version=${this.apiVersion}`,
	// 	);
	// },

	// async getPipelineRuns(pipeline): Promise<any> {
	// 	return await this.fetch(
	// 		`/pipeline-state/${pipeline}/${pipeline}-{GUID}.json?api-version=${this.apiVersion}`,
	// 	);
	// },

	/*
		Data Pipelines
	*/
	async getPipelines(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines?api-version=${this.apiVersion}`,
		);
	},

	async getPipeline(pipeline: string): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/${pipeline}?api-version=${this.apiVersion}`,
		);
	},

	async checkPipelineName(name: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type: 'data-pipeline',
		};

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	async createPipeline(pipeline: any): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/${pipeline.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: pipeline,
			},
		);
	},

	async triggerPipeline(name: string, payload: any): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/${name}/trigger?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	async getPipelineRuns(name: string, payload: any): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/${name}/dataPipelineRuns/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	/*
		Plugins
	*/
	async getPlugins(): Promise<ResourceProviderGetResult<Plugin>[]> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Plugin/plugins?api-version=${this.apiVersion}`,
		);
	},

	async getPlugin(pluginId: string): Promise<ResourceProviderGetResult<Plugin>> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Plugin/plugins/${pluginId}?api-version=${this.apiVersion}`,
		);
	},

	async filterPlugins(categories: string[]): Promise<ResourceProviderGetResult<Plugin>[]> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Plugin/plugins/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					categories,
				}),
			},
		);
	},

	async filterResources(resourcePath: string, filterActionPayload: any): Promise<any> {
		if (filterActionPayload === null) {
			filterActionPayload = {};
		}
		const data = await this.fetch(
			`/instances/${this.instanceId}/${resourcePath}/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(filterActionPayload),
			},
		);
		return data;
	},

	/*
		Vector Databases
	*/
	async getVectorDatabases(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Vector/vectorDatabases?api-version=${this.apiVersion}`,
		);
	},

	/*
		Projects
	*/
	async getProjects(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AzureAI/projects?api-version=${this.apiVersion}`,
		);
	},

	/*
		Knowledge Sources
	*/
	async getKnowledgeSources(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Context/knowledgeSources?api-version=${this.apiVersion}`,
		);
	},

	async getKnowledgeUnit(
		knowledgeUnitName: string
	): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Context/knowledgeUnits/${knowledgeUnitName}?api-version=${this.apiVersion}`,
		);
	},

	async queryKnowledgeSource(
		knowledgeSourceName: string,
		queryRequest: any
	): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Context/knowledgeSources/${knowledgeSourceName}/query?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: queryRequest,
			},
		);
	},

	async renderKnowledgeUnitGraph(
		knowledgeUnitName: string,
		queryRequest: any
	): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Context/knowledgeUnits/${knowledgeUnitName}/render-graph?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: queryRequest,
			},
		);
	}
};
