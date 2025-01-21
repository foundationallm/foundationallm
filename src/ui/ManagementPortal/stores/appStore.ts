import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', {
	state: () => ({
		openCreateAgentItemId: null,
		sidebarCollapsed: false,
	}),

	getters: {},

	actions: {
		initializeSidebarState() {
			// Set sidebarCollapsed based on the screen width
			this.sidebarCollapsed = window.innerWidth < 950;
		},
	},
});
