import api from '@/js/api';
import { nextTick } from 'vue';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	if (to.name === 'status') return;

	// Wait for authentication initialization to complete
	const nuxtApp = useNuxtApp();
	try {
		await nuxtApp.$authReady;
	} catch (error) {
		console.error('Auth initialization failed:', error);
		// Redirect to login with error message
		if (to.name !== 'auth/login') {
			return navigateTo({
				name: 'auth/login',
				query: { message: 'Authentication service failed to initialize. Please try again.' }
			});
		}
		return;
	}

	const authStore = nuxtApp.$authStore;
	const appConfigStore = nuxtApp.$appConfigStore;

	const msal = authStore.msalInstance;
	if (!msal) {
		console.error(
			'MSAL instance not initialized in authStore middleware. ' +
			'This is unexpected since the authReady promise was awaited.'
		);
		
		// Redirect to login with error message instead of silently failing
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
		// Redirect to login with error details
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
		// Update API settings after authentication
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
		if (process.client) {
			const authDetail = {
				isAuthenticated: isAuthenticated,
				currentAccount: authStore.currentAccount,
				hasCurrentAccount: !!authStore.currentAccount,
				msalActiveAccount: msal.getActiveAccount(),
				hasMsalActiveAccount: !!msal.getActiveAccount()
			};
			window.dispatchEvent(new CustomEvent('auth-updated', { detail: authDetail }));
		}

		// If user is authenticated and trying to access login page, redirect to home
		if (to.name === 'auth/login') {
			return navigateTo({ path: '/' });
		}
	}

	// If user is not authenticated and trying to access protected pages, redirect to login
	if (!isAuthenticated && to.name !== 'auth/login') {
		return navigateTo({ name: 'auth/login', query: { redirect: to.fullPath } });
	}
});
