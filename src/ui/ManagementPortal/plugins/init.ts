import { defineNuxtPlugin } from '#app';
import api from '@/js/api';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAuthStore } from '@/stores/authStore';
import { useAppStore } from '@/stores/appStore';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	
	// Create a promise that resolves when authentication initialization is complete
	let resolveAuthReady: () => void;
	const authReadyPromise = new Promise<void>((resolve) =>
		(resolveAuthReady = resolve));
	nuxtApp.provide('authReady', authReadyPromise);

	// Initialize and provide the appConfigStore and authStore
	// to avoid potential race conditions in middleware.

	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	nuxtApp.provide('appConfigStore', appConfigStore);
	
	// Load config variables into the app config store
	await appConfigStore.getConfigVariables();

	const config = useRuntimeConfig();

	// Use LOCAL_API_URL from the .env file if it's set, otherwise use the Azure App Configuration value.
	const localApiUrl = config.public.LOCAL_API_URL;
	const apiUrl = localApiUrl || appConfigStore.apiUrl;

	api.setApiUrl(apiUrl);
	api.setInstanceId(appConfigStore.instanceId);

	const authStore = await useAuthStore(nuxtApp.$pinia).init();
	nuxtApp.provide('authStore', authStore);
	resolveAuthReady();

	const appStore = useAppStore(nuxtApp.$pinia);
	nuxtApp.provide('appStore', appStore);
});
