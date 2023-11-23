import { getMsalInstance, createTokenRefreshTimer } from '@/js/auth';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	const msalInstance = await getMsalInstance();
	await msalInstance.handleRedirectPromise();
	const accounts = await msalInstance.getAllAccounts();

	if (accounts.length > 0 && to.path !== '/') {
		return navigateTo({ path: '/', query: from.query });
	}

	if (accounts.length === 0 && to.path !== '/signin-oidc') {
		return navigateTo({ path: '/signin-oidc', query: from.query });
	} else {
		createTokenRefreshTimer();
	}
});
