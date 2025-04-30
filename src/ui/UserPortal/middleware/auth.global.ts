export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return false;

	if (to.name === 'status') return false;

	const authStore = useNuxtApp().$authStore;
	
	// Handle any pending redirect promises
	try {
		const response = await authStore.msalInstance.handleRedirectPromise();
		// Set the active account if we got one from the redirect
		if (response?.account) {
			authStore.msalInstance.setActiveAccount(response.account);
		}
	} catch (ex) {
		if (ex.name === 'InteractionRequiredAuthError' && ex.message.includes('AADSTS65004')) {
			localStorage.setItem('oneDriveWorkSchoolConsentRedirect', JSON.stringify(false));
		} else {
			throw ex;
		}
	}

	// If no active account is set but we have accounts, set the first one as active
	if (!authStore.msalInstance.getActiveAccount() && authStore.accounts.length > 0) {
		authStore.msalInstance.setActiveAccount(authStore.accounts[0]);
	}

	// Check if we're in an iframe and need to refresh
	if (window.self !== window.top && !authStore.isAuthenticated) {
		// We're in an iframe and not authenticated, try to get the token
		try {
			await authStore.getApiToken();
		} catch (error) {
			// If token acquisition fails, redirect to login
			if (to.name !== 'auth/login') {
				return navigateTo({ name: 'auth/login', query: from.query });
			}
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
