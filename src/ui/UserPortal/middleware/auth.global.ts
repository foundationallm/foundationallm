export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return false;

	if (to.name === 'status') return false;

	const authStore = useNuxtApp().$authStore;
	await authStore.msalInstance.handleRedirectPromise();

	if (authStore.isAuthenticated) {
		if (to.name === 'auth/login') {
			return navigateTo({ path: '/' });
		}
	}

	if (!authStore.isAuthenticated && to.name !== 'auth/login') {
		return navigateTo({ name: 'auth/login', query: from.query });
	}
});
