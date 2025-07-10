export default defineNuxtRouteMiddleware(async (to /*, from */) => {
	if (process.server) return;

	if (to.name === 'status') return;

	const authStore = useNuxtApp().$authStore;
	
	if (authStore?.msalInstance) {
        await authStore.msalInstance.handleRedirectPromise();
    }

	if (authStore?.isAuthenticated) {
		if (to.name === 'auth/login') {
			return navigateTo({ path: '/' });
		}
	}

	if (!authStore?.isAuthenticated && to.name !== 'auth/login') {
		return navigateTo({ name: 'auth/login' });
	}
});
