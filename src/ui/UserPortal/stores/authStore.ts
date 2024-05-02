import { defineStore } from 'pinia';
import type { AccountInfo } from '@azure/msal-browser';
import { PublicClientApplication, EventType } from '@azure/msal-browser';

export const useAuthStore = defineStore('auth', {
	state: () => ({
		msalInstance: null,
		tokenExpirationTimerId: null as number | null,
		isExpired: false,
	}),

	getters: {
		accounts(): AccountInfo[] {
			return this.msalInstance.getAllAccounts();
		},

		currentAccount(): AccountInfo | null {
			return this.accounts[0] || null;
		},

		isAuthenticated(): boolean {
			return !!this.currentAccount && !this.isExpired;
		},

		authConfig() {
			return useNuxtApp().$appConfigStore.auth;
		},
	},

	actions: {
		async init() {
			const msalInstance = new PublicClientApplication({
				auth: {
					clientId: this.authConfig.clientId,
					authority: `${this.authConfig.instance}${this.authConfig.tenantId}`,
					redirectUri: this.authConfig.callbackPath,
					scopes: this.authConfig.scopes,
					// Must be registered as a SPA redirectURI on your app registration.
					postLogoutRedirectUri: '/',
				},
				cache: {
					cacheLocation: 'sessionStorage',
				},
			});

			msalInstance.addEventCallback((event) => {
				const { eventType } = event;
				if (
					eventType === EventType.ACQUIRE_TOKEN_SUCCESS ||
					eventType === EventType.LOGIN_SUCCESS
				) {
					this.createTokenRefreshTimer();
				}
			});

			await msalInstance.initialize();

			this.msalInstance = msalInstance;

			return this;
		},

		createTokenRefreshTimer() {
			const tokenExpirationTime = this.currentAccount.idTokenClaims.exp * 1000;
			const currentTime = Date.now();
			const timeUntilExpirationMS = tokenExpirationTime - currentTime;

			if (timeUntilExpirationMS <= 0) {
				console.log(`Auth: Access token expired ${timeUntilExpirationMS / 1000} seconds ago.`);
				this.isExpired = true;
				return;
				// return useNuxtApp().$router.push({
				// 	name: 'auth/login',
				// 	query: {
				// 		message: 'Your login has expired. Please sign in again.',
				// 	},
				// });
			}

			clearTimeout(this.tokenExpirationTimerId);

			this.tokenExpirationTimerId = setTimeout(() => {
				this.refreshToken();
			}, timeUntilExpirationMS);

			console.log(
				`Auth: Set access token timer refresh in ${timeUntilExpirationMS / 1000} seconds.`,
			);
		},

		async refreshToken() {
			try {
				await this.msalInstance.acquireTokenSilent({
					account: this.currentAccount,
					scopes: [this.authConfig.scopes],
				});
				console.log('Auth: Refreshed access token.');
				this.createTokenRefreshTimer();
			} catch (error) {
				console.error('Auth: Token refresh error:', error);
				// sessionStorage.clear();
				this.isExpired = true;
				// useNuxtApp().$router.push({ name: 'auth/login' });
			}
		},

		async getToken() {
			try {
				return await this.msalInstance.acquireTokenSilent({
					account: this.currentAccount,
				});
			} catch (error) {
				this.isExpired = true;
				throw error;
			}
		},

		async login() {
			return await this.msalInstance.loginRedirect({
				scopes: [this.authConfig.scopes],
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
	},
});
