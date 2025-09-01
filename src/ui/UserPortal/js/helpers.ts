import type { AgentBase, ResourceProviderGetResult } from '@/js/types';

export const isAgentExpired = (agent: ResourceProviderGetResult<AgentBase>): boolean => {
	return agent.resource.expiration_date !== null && new Date() > new Date(agent.resource.expiration_date)
}

// Debounce utility
export function debounce<T extends (...args: any[]) => any>(func: T, wait: number) {
	let timeout: ReturnType<typeof setTimeout> | null;
	return function(this: any, ...args: Parameters<T>) {
		if (timeout) clearTimeout(timeout);
		timeout = setTimeout(() => func.apply(this, args), wait);
	} as T;
}