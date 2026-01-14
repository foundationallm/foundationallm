import api from '@/js/api';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	if (to.name === 'status') return;

	const nuxtApp = useNuxtApp();
	const authStore = nuxtApp.$authStore;
	const appConfigStore = nuxtApp.$appConfigStore;

	const msal = authStore?.msalInstance;
	if (!msal) {
		console.error('MSAL instance not initialized in authStore middleware.');
		if (to.name !== 'auth/login') {
			return navigateTo({
				name: 'auth/login',
				query: { message: 'Authentication service unavailable. Please refresh the page.' }
			});
		}
		return;
	}

	// Handle MSAL redirect promise
	let redirectResult = null;
	try {
		redirectResult = await msal.handleRedirectPromise();
	} catch (ex: any) {
		console.error('MSAL redirect error:', ex);
		if (to.name !== 'auth/login') {
			return navigateTo({
				name: 'auth/login',
				query: { message: 'Authentication error. Please sign in again.' }
			});
		}
	}

	// If we have a redirect result, process it
	if (redirectResult) {
		// If redirect result has an account, ensure it's properly set
		if (redirectResult.account) {
			try {
				// Set the active account directly from redirect result
				msal.setActiveAccount(redirectResult.account);
				authStore.isExpired = false;

				// Force trigger reactive updates
				const activeAccount = msal.getActiveAccount();
				if (activeAccount) {
					authStore.forceAccountUpdate();
				}
			} catch (error) {
				console.error('Error setting active account from redirect:', error);
			}
		}
	}

	// Check authentication after potential redirect
	// If we have a redirect result with an account, consider user authenticated even if accounts array is empty
	const hasRedirectAccount = redirectResult && redirectResult.account;
	const isAuthenticated = authStore.isAuthenticated || hasRedirectAccount;

	if (isAuthenticated) {
		// Load full configuration after successful authentication
		// Only load if we haven't loaded it yet.
		if (!appConfigStore.isAppConfigurationSetLoaded) {
			try {
				await appConfigStore.loadConfigurationAfterAuth();

				// Update API settings after loading configuration
				const config = useRuntimeConfig();
				const localApiUrl = config.public.LOCAL_API_URL;
				const finalApiUrl = localApiUrl || appConfigStore.apiUrl;

				if (finalApiUrl) {
					api.setApiUrl(finalApiUrl);
				}

				if (appConfigStore.instanceId) {
					api.setInstanceId(appConfigStore.instanceId);
				}
			} catch (error) {
				console.warn('Failed to load configuration after authentication in middleware:', error);
				// Don't block navigation if config loading fails
			}
		}

		// If user is authenticated and trying to access login page, redirect to home
		if (to.name === 'auth/login') {
			return navigateTo({ path: '/' });
		}
	}

	// If user is not authenticated and trying to access protected pages, redirect to login
	if (!isAuthenticated && to.name !== 'auth/login') {
		return navigateTo({ name: 'auth/login', query: from.query });
	}
});
