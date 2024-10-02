export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return false;

	if (to.name === 'status') return false;

	const authStore = useNuxtApp().$authStore;
	try {
		await authStore.msalInstance.handleRedirectPromise();
	} catch (ex) {
		if (ex.name === 'InteractionRequiredAuthError' && ex.message.includes('AADSTS65004')) {
			localStorage.setItem('oneDriveConsentRedirect', JSON.stringify(false));
		} else {
			throw ex;
		}
	}

	if (authStore.isAuthenticated) {
		if (to.name === 'auth/login') {
			return navigateTo({ path: '/' });
		}
	}

	if (!authStore.isAuthenticated && to.name !== 'auth/login') {
		return navigateTo({ name: 'auth/login', query: from.query });
	}
});
