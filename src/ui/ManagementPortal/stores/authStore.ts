import { defineStore } from 'pinia';
import type { AccountInfo } from '@azure/msal-browser';
import { 
	PublicClientApplication,
	InteractionRequiredAuthError
} from '@azure/msal-browser';

export const useAuthStore = defineStore('auth', {
	state: () => ({
		msalInstance: null as PublicClientApplication | null,
		tokenExpirationTimerId: null as number | null,
		isExpired: false,
		apiToken: null as any,
		// Reactive trigger to force updates when account changes
		accountUpdateTrigger: 0,
	}),

	getters: {
		accounts(): AccountInfo[] {
			if (!this.msalInstance) return [];
			try {
				return this.msalInstance.getAllAccounts();
			} catch (error) {
				console.error('Error getting accounts from MSAL:', error);
				return [];
			}
		},

		currentAccount(): AccountInfo | null {
			// Force reactivity by accessing trigger
			this.accountUpdateTrigger;
			const accountsArray = this.accounts;
			
			if (this.msalInstance) {
				const activeAccount = this.msalInstance.getActiveAccount();
				if (activeAccount) {
					return activeAccount;
				}
			}
			
			return accountsArray[0] || null;
		},

		isAuthenticated(): boolean {
			const hasAccount = !!this.currentAccount && !this.isExpired;
			this.accounts.length; // Force reactivity
			return hasAccount;
		},

		authConfig() {
			return useNuxtApp().$appConfigStore.auth;
		},

		apiScopes() {
			return [this.authConfig.scopes];
		},
	},

	actions: {
		async init() {
			const msalInstance = new PublicClientApplication({
				auth: {
					clientId: this.authConfig.clientId,
					authority: `${this.authConfig.instance}${this.authConfig.tenantId}`,
					redirectUri: this.authConfig.callbackPath,
					// Must be registered as a SPA redirectURI on your app registration.
					postLogoutRedirectUri: '/',
				},
				cache: {
					cacheLocation: 'sessionStorage',
				},
			});

			await msalInstance.initialize();
			this.msalInstance = msalInstance;

			// Set active account if we have accounts
			const accounts = msalInstance.getAllAccounts();
			if (accounts.length > 0) {
				msalInstance.setActiveAccount(accounts[0]);
			}

			return this;
		},

		createTokenRefreshTimer() {
			const tokenExpirationTimeMS = this.apiToken.expiresOn;
			const currentTime = Date.now();
			const timeUntilExpirationMS = tokenExpirationTimeMS - currentTime;

			// If the token expires within the next minute, try to refresh it
			if (timeUntilExpirationMS <= 60 * 1000) {
				return this.tryTokenRefresh();
			}

			console.log(`Auth: Cleared previous access token timer.`);
			clearTimeout(this.tokenExpirationTimerId);

			this.tokenExpirationTimerId = setTimeout(() => {
				this.tryTokenRefresh();
			}, timeUntilExpirationMS);

			const refreshDate = new Date(tokenExpirationTimeMS);
			console.log(
				`Auth: Set access token timer refresh for ${refreshDate} (in ${timeUntilExpirationMS / 1000} seconds).`,
			);
		},

		async tryTokenRefresh() {
			try {
				await this.getApiToken();
				console.log('Auth: Successfully refreshed access token.');
			} catch (error) {
				console.error('Auth: Failed to refresh access token:', error);
				this.isExpired = true;
			}
		},

		async getApiToken() {
			try {
				this.apiToken = await this.msalInstance.acquireTokenSilent({
					account: this.currentAccount,
					scopes: this.apiScopes,
				});

				this.createTokenRefreshTimer();

				return this.apiToken;
			} catch (error) {
				this.isExpired = true;
				throw error;
			}
		},

		async getProfilePhoto(): string | null {
			try {
				const graphScopes = ['https://graph.microsoft.com/User.Read'];
				const graphToken = await this.msalInstance.acquireTokenSilent({
					account: this.currentAccount,
					scopes: graphScopes,
				});

				const profilePhotoBlob = await $fetch('https://graph.microsoft.com/v1.0/me/photo/$value', {
					method: 'GET',
					headers: {
						Authorization: `Bearer ${graphToken.accessToken}`,
					},
				});

				return URL.createObjectURL(profilePhotoBlob);
			} catch (error) {
				return null;
			}
		},

		async login() {
			return await this.msalInstance.loginRedirect({
				scopes: this.apiScopes,
			});
		},

		async logoutSilent() {
			await this.msalInstance.controller.browserStorage.clear();
		},

		async logout() {
			await this.msalInstance.logoutRedirect({
				account: this.currentAccount,
			});

			useNuxtApp().$router.push({ name: 'auth/login' });
		},

		forceAccountUpdate() {
			this.accountUpdateTrigger++;
		},

		isInteractionRequiredError(error: any): boolean {
			if (!error) return false;

			// If you have access to the error class:
			if (error instanceof InteractionRequiredAuthError) return true;

			const code = error.errorCode || error.code || "";
			const name = error.name || "";

			return (
				name === "InteractionRequiredAuthError" ||
				code === "interaction_required" ||
				code === "login_required" ||
				code === "consent_required" ||
				code === "no_account_in_silent_request" ||
				code === "no_tokens_found" ||
				code === "user_login_error"
			);
		}
	},
});
