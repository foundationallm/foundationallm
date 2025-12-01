import api from '@/js/api';
import { nextTick } from 'vue';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return false;

	if (to.name === 'status') return false;

	// Wait for authentication initialization to complete
	const nuxtApp = useNuxtApp();
  	await nuxtApp.$authReady;

	const authStore = nuxtApp.$authStore;
	const appConfigStore = nuxtApp.$appConfigStore;

	const msal = authStore.msalInstance;
	if (!msal) {
		console.error(
			'MSAL instance not initialized in authStore middleware. ' +
			'This is unexpected since the authReady promise was awaited.'
		);
		return false;
	}

	// Handle MSAL redirect promise
	let redirectResult = null;
	try {
		redirectResult = await msal.handleRedirectPromise();
	} catch (ex) {
		if (ex.name === 'InteractionRequiredAuthError' && ex.message.includes('AADSTS65004')) {
			localStorage.setItem('oneDriveWorkSchoolConsentRedirect', JSON.stringify(false));
		} else {
			console.error('MSAL redirect error:', ex);
			throw ex;
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

				// Small delay to ensure MSAL state is fully updated
				await new Promise(resolve => setTimeout(resolve, 500));

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
		// Only load if we haven't loaded it yet (check if logoUrl and logoText are still null)
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

				// Force Vue reactivity update
				await nextTick();

				// Emit a global event to notify components
				if (process.client && window) {
					const configDetail = {
						logoUrl: appConfigStore.logoUrl,
						logoText: appConfigStore.logoText
					};
					window.dispatchEvent(new CustomEvent('config-loaded', { detail: configDetail }));

					// Also emit auth-updated event
					const authDetail = {
						isAuthenticated: isAuthenticated,
						currentAccount: authStore.currentAccount,
						hasCurrentAccount: !!authStore.currentAccount,
						msalActiveAccount: msal.getActiveAccount(),
						hasMsalActiveAccount: !!msal.getActiveAccount()
					};
					window.dispatchEvent(new CustomEvent('auth-updated', { detail: authDetail }));
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
