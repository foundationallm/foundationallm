import { getMsalInstance } from '@/js/auth';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	const msalInstance = await getMsalInstance();

	const accounts = await msalInstance.getAllAccounts();
	if (accounts.length === 0 && to.path !== '/login') {
		return navigateTo({ path: '/login', query: from.query });
	}
});
