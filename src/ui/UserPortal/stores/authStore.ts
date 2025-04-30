import { defineStore } from 'pinia';
import type { AccountInfo } from '@azure/msal-browser';
import { PublicClientApplication } from '@azure/msal-browser';
import { useAppStore } from './appStore';

const SHOW_LOGS = false;

export const useAuthStore = defineStore('auth', {
	state: () => ({
		msalInstance: null,
		tokenExpirationTimerId: null as number | null,
		isExpired: false,
		apiToken: null,
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

		oneDriveWorkSchoolScopes() {
			const appStore = useAppStore();
			return appStore.coreConfiguration?.fileStoreConnectors?.find(
				(connector) => connector.subcategory === 'OneDriveWorkSchool',
			)?.authentication_parameters.scope;
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
					scopes: this.apiScopes,
					// Must be registered as a SPA redirectURI on your app registration.
					postLogoutRedirectUri: '/',
				},
				cache: {
					cacheLocation: 'sessionStorage',
				},
			});

			await msalInstance.initialize();
			this.msalInstance = msalInstance;

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

			SHOW_LOGS && console.log(`Auth: Cleared previous access token timer.`);
			clearTimeout(this.tokenExpirationTimerId);

			this.tokenExpirationTimerId = setTimeout(() => {
				this.tryTokenRefresh();
			}, timeUntilExpirationMS);

			const refreshDate = new Date(tokenExpirationTimeMS);
			SHOW_LOGS &&
				console.log(
					`Auth: Set access token timer refresh for ${refreshDate} (in ${timeUntilExpirationMS / 1000} seconds).`,
				);
		},

		async tryTokenRefresh() {
			try {
				await this.getApiToken();
				SHOW_LOGS && console.log('Auth: Successfully refreshed access token.');
			} catch (error) {
				SHOW_LOGS && console.error('Auth: Failed to refresh access token:', error);
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

		async requestOneDriveWorkSchoolConsent() {
			let accessToken = '';
			const oneDriveWorkSchoolAPIScopes: any = {
				account: this.currentAccount,
				scopes: [this.oneDriveWorkSchoolScopes],
			};

			try {
				const resp = await this.msalInstance.acquireTokenSilent(oneDriveWorkSchoolAPIScopes);
				accessToken = resp.accessToken;
			} catch (error) {
				try {
					// Try popup first
					const response = await this.msalInstance.acquireTokenPopup(oneDriveWorkSchoolAPIScopes);
					accessToken = response.accessToken;
				} catch (popupError) {
					if (popupError.name === 'BrowserAuthError' && popupError.errorCode === 'popup_window_error') {
						// Fallback to redirect if popup is blocked
						localStorage.setItem('oneDriveWorkSchoolConsentRedirect', JSON.stringify(true));
						oneDriveWorkSchoolAPIScopes.state = 'Core API redirect';
						await this.msalInstance.acquireTokenRedirect(oneDriveWorkSchoolAPIScopes);
					} else {
						throw popupError;
					}
				}
			}
			return accessToken;
		},

		async getOneDriveWorkSchoolToken(): Promise<string | null> {
			try {
				const appStore = useAppStore();
				const oneDriveBaseURL = appStore.coreConfiguration?.fileStoreConnectors?.find(
					(connector) => connector.subcategory === 'OneDriveWorkSchool',
				)?.url;

				if (!oneDriveBaseURL || !this.oneDriveWorkSchoolScopes) {
					console.warn('OneDrive configuration not found');
					return null;
				}

				const scopes = [`${oneDriveBaseURL}${this.oneDriveWorkSchoolScopes}`];
				const oneDriveToken = await this.msalInstance?.acquireTokenSilent({
					account: this.currentAccount,
					scopes,
				});

				if (!oneDriveToken) {
					console.warn('Failed to acquire OneDrive token');
					return null;
				}

				return oneDriveToken;
			} catch (error) {
				console.warn('Error acquiring OneDrive token:', error);
				return null;
			}
		},

		async getProfilePhoto() {
			try {
				const token = await this.getOneDriveWorkSchoolToken();
				if (!token) {
					return null;
				}

				const response = await fetch('https://graph.microsoft.com/v1.0/me/photo/$value', {
					headers: {
						Authorization: `Bearer ${token.accessToken}`,
					},
				});

				if (!response.ok) {
					if (response.status === 404) {
						// User doesn't have a profile photo, return default avatar
						return null;
					}
					throw new Error(`Failed to get profile photo: ${response.statusText}`);
				}

				const blob = await response.blob();
				return URL.createObjectURL(blob);
			} catch (error) {
				console.warn('Failed to get profile photo:', error);
				return null;
			}
		},

		async login() {
			try {
				const response = await this.msalInstance.loginPopup({
					scopes: this.apiScopes,
				});
				
				// Set the active account after successful login
				if (response.account) {
					this.msalInstance.setActiveAccount(response.account);
				}
				
				// After setting the active account, get the token
				await this.getApiToken();
				
				// Force a refresh of the current window
				if (window.self !== window.top) {
					// We're in an iframe
					window.location.reload();
				}
				
				return response;
			} catch (error) {
				if (error.name === 'BrowserAuthError' && error.errorCode === 'popup_window_error') {
					// Fallback to redirect if popup is blocked
					return await this.msalInstance.loginRedirect({
						scopes: this.apiScopes,
					});
				}
				throw error;
			}
		},

		async clearLocalSession() {
			await this.msalInstance.controller.browserStorage.clear();
		},

		async logoutSilent() {
			const logoutHint = this.currentAccount.idTokenClaims.login_hint;
			try {
				await this.msalInstance.logoutPopup({
					logoutHint,
				});
				
				// Force a refresh of the current window if in iframe
				if (window.self !== window.top) {
					window.location.reload();
				}
			} catch (error) {
				if (error.name === 'BrowserAuthError' && error.errorCode === 'popup_window_error') {
					// Fallback to redirect if popup is blocked
					await this.msalInstance.logoutRedirect({
						logoutHint,
					});
				} else {
					throw error;
				}
			}
		},

		async logout() {
			try {
				await this.msalInstance.logoutPopup({
					account: this.currentAccount,
				});
				
				// Force a refresh of the current window if in iframe
				if (window.self !== window.top) {
					window.location.reload();
				}
			} catch (error) {
				if (error.name === 'BrowserAuthError' && error.errorCode === 'popup_window_error') {
					// Fallback to redirect if popup is blocked
					await this.msalInstance.logoutRedirect({
						account: this.currentAccount,
					});
				} else {
					throw error;
				}
			}

			useNuxtApp().$router.push({ name: 'auth/login' });
		},
	},
});
