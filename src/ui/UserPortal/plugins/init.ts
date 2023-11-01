import { defineNuxtPlugin } from '#app';
import { appConfig } from '@/stores/appConfig';
import api from '@/js/api';
import { setAuthConfig } from '@/js/auth';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables server-side to ensure they are passed to the client via the store.
	if (process.server) {
		const appConfigStore = appConfig(nuxtApp.$pinia);
		await appConfigStore.getConfigVariables();
	}

	// Set the api url to use from the dynamic azure config.
	const appConfigStore = appConfig(nuxtApp.$pinia);
	api.setApiUrl(appConfigStore.apiUrl);

	// Set the auth configuration for MSAL from the dynamic azure config.
	setAuthConfig(appConfigStore.auth);
});
