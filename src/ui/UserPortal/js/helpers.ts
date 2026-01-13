import type { AgentBase, ResourceProviderGetResult } from '@/js/types';

export const isAgentExpired = (agent: ResourceProviderGetResult<AgentBase> | null | undefined): boolean => {
	if (!agent || !agent.resource) return false;
	return agent.resource.expiration_date !== null && new Date() > new Date(agent.resource.expiration_date);
}

export const isAgentFileUploadEnabled = (agent: ResourceProviderGetResult<AgentBase>): boolean => {
	if (!agent || !agent.resource) return false;
	return !!agent.resource.show_file_upload;
}

// Debounce utility
export function debounce<T extends (...args: any[]) => any>(func: T, wait: number) {
	let timeout: ReturnType<typeof setTimeout> | null;
	return function(this: any, ...args: Parameters<T>) {
		if (timeout) clearTimeout(timeout);
		timeout = setTimeout(() => func.apply(this, args), wait);
	} as T;
}

// Returns true if the roles array means the agent is readonly (has Reader but not Owner or Contributor)
export function isAgentReadonly(roles: string[] = []): boolean {
	return roles.includes('Reader') && !roles.includes('Owner') && !roles.includes('Contributor');
}

// Returns true if the user has Owner or Contributor role (can edit the agent)
export function canEditAgent(roles: string[] = []): boolean {
	return roles.includes('Owner') || roles.includes('Contributor');
}