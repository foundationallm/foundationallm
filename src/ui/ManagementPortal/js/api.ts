/* eslint-disable prettier/prettier */

import type {
	Agent,
	AgentDataSource,
	AgentIndex,
	AgentGatekeeper,
	CreateAgentRequest,
	AgentCheckNameResponse,
	Prompt,
	TextPartitioningProfile,
	TextEmbeddingProfile,
	CreatePromptRequest,
	CreateTextPartitioningProfileRequest
} from './types';
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

	async checkAgentName(name: string, agentType: string): Promise<AgentCheckNameResponse> {
		const payload = {
			name: name,
			type: agentType,
		};
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/checkname?api-version=${this.apiVersion}`, {
			method: 'POST',	
			body: payload,
		});
	},

	async getAgentDataSources(): Promise<AgentDataSource[]> {
		const data = await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/contentsourceprofiles?api-version=${this.apiVersion}`);
		return data.map((source) => ({ ...source, Formats: ['pdf', 'txt'] }));
	},

	async getAgentIndexes(): Promise<AgentIndex[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/indexingprofiles?api-version=${this.apiVersion}`);
	},

	async getTextEmbeddingProfiles(): Promise<TextEmbeddingProfile[]> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textembeddingprofiles?api-version=${this.apiVersion}`);
	},

	async getAgentGatekeepers(): Promise<AgentGatekeeper[]> {
		await wait(this.mockLoadTime);
		return [];
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
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/${agentId}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},
}
