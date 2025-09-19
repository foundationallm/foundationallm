import { defineNuxtPlugin } from '#app';
import api from '@/js/api';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAuthStore } from '@/stores/authStore';
import { useAppStore } from '@/stores/appStore';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load authentication config variables first (required for auth initialization)
	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	await appConfigStore.getConfigVariables();

	const config = useRuntimeConfig();

	// Make stores globally accessible on the nuxt app instance
	nuxtApp.provide('appConfigStore', appConfigStore);

	// Set API URL and Instance ID after loading auth config
	const localApiUrl = config.public.LOCAL_API_URL;
	const apiUrl = localApiUrl || appConfigStore.apiUrl;
	
	// Set API URL and Instance ID
	if (apiUrl) {
		api.setApiUrl(apiUrl);
	}
	
	if (appConfigStore.instanceId) {
		api.setInstanceId(appConfigStore.instanceId);
	}

	// Initialize authentication store
	const authStore = await useAuthStore(nuxtApp.$pinia).init();
	nuxtApp.provide('authStore', authStore);

	// Only load full configuration if user is authenticated
	// This prevents API calls before authentication is complete
	if (authStore.isAuthenticated) {
		try {
			// After authentication is complete, load the full configuration
			// This includes the app configuration set that replaces individual API calls
			await appConfigStore.loadFullConfiguration();
			
			// Update API URL after loading full configuration if needed
			const finalApiUrl = localApiUrl || appConfigStore.apiUrl;
			if (finalApiUrl && finalApiUrl !== api.getApiUrl()) {
				api.setApiUrl(finalApiUrl);
			}
			
			// Update instance ID after loading full configuration if needed
			if (appConfigStore.instanceId && appConfigStore.instanceId !== api.instanceId) {
				api.setInstanceId(appConfigStore.instanceId);
			}
		} catch (error: any) {
			console.warn('Failed to load app configuration set during init, will be loaded later:', error);
			// Don't throw error here, let the app continue loading
			// Configuration will be loaded when user authenticates
			// If it's a 403 error, the error state will be preserved in the store
		}
	}

	const appStore = useAppStore(nuxtApp.$pinia);
	nuxtApp.provide('appStore', appStore);
});
