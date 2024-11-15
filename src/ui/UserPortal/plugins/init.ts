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

	const appStore = useAppStore(nuxtApp.$pinia);
	nuxtApp.provide('appStore', appStore);
});
