import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', {
	state: () => ({
		openCreateAgentItemId: null,
		sidebarCollapsed: false,
	}),

	getters: {},

	actions: {},
});
