import { defineNuxtPlugin } from '#app';
import api from '@/js/api';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAuthStore } from '@/stores/authStore';
import { useAppStore } from '@/stores/appStore';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables into the app config store
	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	await appConfigStore.getConfigVariables();

	const config = useRuntimeConfig();

	// Use LOCAL_API_URL from the .env file if it's set, otherwise use the Azure App Configuration value.
	const localApiUrl = config.public.LOCAL_API_URL;
	const apiUrl = localApiUrl || appConfigStore.apiUrl;

	api.setApiUrl(apiUrl);
	api.setInstanceId(appConfigStore.instanceId);

	// Make stores globally accessible on the nuxt app instance
	nuxtApp.provide('appConfigStore', appConfigStore);

	const authStore = await useAuthStore(nuxtApp.$pinia).init();
	nuxtApp.provide('authStore', authStore);

	if (authStore.isAuthenticated) {
		try {
			// After authentication is complete, load the full configuration
			// This includes the app configuration set that replaces individual API calls
			await appConfigStore.loadFullConfiguration();

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
