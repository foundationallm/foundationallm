import { msalInstance } from '@/js/auth';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	await msalInstance.initialize();
	const accounts = await msalInstance.getAllAccounts();
	if (accounts.length === 0 && to.path !== '/login') {
		return navigateTo({ path: '/login', query: from.query });
	}
});