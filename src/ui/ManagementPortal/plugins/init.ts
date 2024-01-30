import { defineNuxtPlugin } from '#app';
import { useAppConfigStore } from '@/stores/appConfigStore';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables into the app config store
	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	await appConfigStore.getConfigVariables();

	// Provide global properties to the app
	nuxtApp.provide('appConfigStore', appConfigStore);
});
