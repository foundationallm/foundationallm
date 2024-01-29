/* eslint-disable prettier/prettier */

import type {
	AgentDataSource,
	AgentIndex,
	AgentGatekeeper,
	MockCreateAgentRequest
} from './types';
import { mockGetAgentIndexesResponse } from './mock';

async function wait(milliseconds: number = 1000): Promise<void> {
	return await new Promise<void>((resolve) => setTimeout(() => resolve(), milliseconds));
}

export default {
	mockLoadTime: 2000,

	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key
			}
		});
	},

	async getAgentDataSources(): Promise<AgentDataSource[]> {
		await wait(this.mockLoadTime);
		return [];
	},

	async getAgentIndexes(): Promise<AgentIndex[]> {
		await wait(this.mockLoadTime);
		return mockGetAgentIndexesResponse;
	},

	async getAgentGatekeepers(): Promise<AgentGatekeeper[]> {
		await wait(this.mockLoadTime);
		return [];
	},

	async createAgent(request: MockCreateAgentRequest): Promise<void> {
		console.log('Mock create agent started:', request);
		const waitPromise = await wait(this.mockLoadTime);
		console.log('Mock create agent completed.');
		return waitPromise;
	},
}
