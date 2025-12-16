import { defineStore } from 'pinia';
import type {
	AccountInfo,
	AuthenticationResult
} from '@azure/msal-browser';
import { 
	PublicClientApplication,
	InteractionRequiredAuthError
} from '@azure/msal-browser';
import { useAppStore } from './appStore';

const SHOW_LOGS = false;

export const useAuthStore = defineStore('auth', {
	state: () => ({
		msalInstance: null,
		tokenExpirationTimerId: null as number | null,
		isExpired: false,
		apiToken: null,
		// Reactive trigger to force updates when account changes
		accountUpdateTrigger: 0,

		profilePhoto: null as string | null,
		profilePhotoLoadingPromise: null as Promise<string | null> | null,
		isProfilePhotoLoaded: false,
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

		oneDriveWorkSchoolScopes() {
			const appStore = useAppStore();
			return appStore.coreConfiguration?.fileStoreConnectors?.find(
				(connector) => connector.subcategory === 'OneDriveWorkSchool',
			)?.authentication_parameters.scope;
		},

		apiScopes() {
			return this.authConfig?.scopes || [];
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

			// Set active account if we have accounts
			const accounts = msalInstance.getAllAccounts();
			if (accounts.length > 0) {
				msalInstance.setActiveAccount(accounts[0]);
			}

			return this;
		},

		createTokenRefreshTimer() {
			const tokenExpirationTimeMS = this.apiToken.expiresOn.getTime();
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

		async getApiToken(forceInteractive: boolean = false): Promise<AuthenticationResult> {
			if (!forceInteractive) {
				try {
					const result = await this.msalInstance.acquireTokenSilent({
						account: this.currentAccount,
						scopes: this.apiScopes,
					});

					this.apiToken = result;
					this.isExpired = false;
					this.createTokenRefreshTimer();

					return result;
				} catch (error: any) {
					if (this.isInteractionRequiredError(error)) {
						SHOW_LOGS && console.warn('Auth: Silent token acquisition failed, interaction required.');
					} else {
						SHOW_LOGS && console.error('Auth: Silent token acquisition failed:', error);
						this.isExpired = true;
						throw error;
					}
				}
			}

			// Fallback to interactive token acquisition
			try {
				const result = await this.msalInstance.acquireTokenPopup({
					account: this.currentAccount,
					scopes: this.apiScopes,
				});
				this.apiToken = result;
				this.isExpired = false;
				this.createTokenRefreshTimer();
				return result;
			} catch (popupError: any) {
				SHOW_LOGS && console.error('Auth: Interactive token acquisition failed:', popupError);
				this.isExpired = true;
				throw popupError;
			}
		},

		async getOneDriveWorkSchoolGraphAPIToken() {
			let accessToken = '';
			const oneDriveWorkSchoolAPIScopes: any = {
				account: this.currentAccount,
				scopes: [this.oneDriveWorkSchoolScopes],
			};

			try {
				const resp = await this.msalInstance.acquireTokenSilent(oneDriveWorkSchoolAPIScopes);
				accessToken = resp.accessToken;
			} catch (error) {
				// Redirect to get token or login
				localStorage.setItem('oneDriveWorkSchoolConsentRedirect', JSON.stringify(true));

				oneDriveWorkSchoolAPIScopes.state = 'Core API redirect';
				await this.msalInstance.loginRedirect(oneDriveWorkSchoolAPIScopes);
			}
			return accessToken;
		},

		async getOneDriveWorkSchoolSPOToken(): Promise<string | null> {
			const appStore = useAppStore();
			const connector = appStore.coreConfiguration?.fileStoreConnectors?.find(
				(c) => c.subcategory === 'OneDriveWorkSchool',
			);
			
			if (!connector?.url || !this.oneDriveWorkSchoolScopes) {
				throw new Error('OneDrive Work/School configuration is not available');
			}

			// The scope should be the base URL + scope suffix (e.g., "https://contoso-my.sharepoint.com/.default")
			const fullScope = `${connector.url}${this.oneDriveWorkSchoolScopes}`;
			
			const oneDriveToken = await this.msalInstance.acquireTokenSilent({
				account: this.currentAccount,
				scopes: [fullScope],
			});

			return oneDriveToken.accessToken;
		},

		async getProfilePhoto(): Promise<string | null> {
			if (this.isProfilePhotoLoaded) {
				return this.profilePhoto;
			}
			if (this.profilePhotoLoadingPromise) {
				return this.profilePhotoLoadingPromise;
			}

			this.profilePhotoLoadingPromise = (async () => {
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

					this.profilePhoto = URL.createObjectURL(profilePhotoBlob);
            		this.isProfilePhotoLoaded = true;
					return this.profilePhoto;
				} catch (error) {
					this.isProfilePhotoLoaded = true; // Mark as loaded even on error to prevent retries
					this.profilePhoto = null;
					return null;
				} finally {
					this.profilePhotoLoadingPromise = null;
				}
			})();

			return this.profilePhotoLoadingPromise;
		},

		async login() {
			await this.msalInstance.loginRedirect({
				scopes: this.apiScopes,
			});
		},

		async clearLocalSession() {
			await this.msalInstance.controller.browserStorage.clear();
		},

		async logoutSilent() {
			const logoutHint = this.currentAccount.idTokenClaims.login_hint;
			await this.msalInstance.logoutRedirect({
				...(logoutHint && { logoutHint }),
			});
		},

		async logout() {
			await this.msalInstance.logoutRedirect({
				account: this.currentAccount,
			});
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
