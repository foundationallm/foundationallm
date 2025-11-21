import { defineStore } from 'pinia';
import api from '@/js/api';

export const useAppConfigStore = defineStore('appConfig', {
	state: () => ({
		// Loading states
		isConfigurationLoaded: false,
		isFeaturedAgentNamesLoaded: false,
		isAppConfigurationSetLoaded: false,
		hasConfigurationAccessError: false,
		configurationAccessErrorMessage: null as string | null,

		// API: Defines API-specific settings such as the base URL for application requests.
		apiUrl: null as string | null,

		// Layout: These settings impact the structural layout of the chat interface.
		isKioskMode: false,

		// Style: These settings impact the visual style of the chat interface.
		pageTitle: null as string | null,
		favIconUrl: null as string | null,
		logoUrl: null as string | null,
		logoText: null as string | null,
		primaryBg: null as string | null,
		primaryColor: null as string | null,
		secondaryColor: null as string | null,
		accentColor: null as string | null,
		primaryText: null as string | null,
		secondaryText: null as string | null,
		accentText: null as string | null,
		primaryButtonBg: null as string | null,
		primaryButtonText: null as string | null,
		secondaryButtonBg: null as string | null,
		secondaryButtonText: null as string | null,
		footerText: null as string | null,
		noAgentsMessage: null as string | null,
		defaultAgentWelcomeMessage: null as string | null,
		agentIconUrl: null as string | null,

		instanceId: null as string | null,
		allowedUploadFileExtensions: null as string | null,

		showMessageRating: null as boolean | null,
		showLastConversionOnStartup: null as boolean | null,
		showMessageTokens: null as boolean | null,
		showViewPrompt: null as boolean | null,
		showFileUpload: null as boolean | null,
		featuredAgentNames: [] as string[] | null,
		pinnedFeaturedAgentNames: [] as string[] | null,
		agentManagementPermissionRequestUrl: null as string | null,

		// Auth: These settings configure the MSAL authentication.
		auth: {
			clientId: null as string | null,
			instance: null as string | null,
			tenantId: null as string | null,
			scopes: [] as string[],
			callbackPath: null as string | null,
			timeoutInMinutes: 60,
		},

		// Feature flags
		agentSelfServiceFeatureEnabled: false,
	}),
	getters: {},
	actions: {
		/**
		 * Loads basic branding configuration values that are required before authentication.
		 * This method loads only the essential branding values needed for the signin page.
		 */
		async loadBasicBrandingConfiguration() {
			try {
				const branding = await api.filterConfigValues('FoundationaLLM:Branding:*');

				// Branding configuration - treat missing keys as explicit null
				// `branding` from the API may be an object mapping keys->values; normalize and provide a safe getter.
				const configValues = (branding && typeof branding === 'object') ? branding : {};
				const getBrand = (key: string) => Object.prototype.hasOwnProperty.call(configValues, key) ? configValues[key] : null;

				// KioskMode: keep previous boolean parsing but return null if key not present
				const kioskRaw = getBrand('FoundationaLLM:Branding:KioskMode');
				if (kioskRaw !== null && kioskRaw !== undefined) {
						this.isKioskMode = typeof kioskRaw === 'boolean'
								? (kioskRaw as boolean)
								: JSON.parse((kioskRaw as string).toLowerCase());
				} else {
						this.isKioskMode = false;
				}

				// Simple string-valued branding properties: set to the value or null when absent
				this.pageTitle = getBrand('FoundationaLLM:Branding:PageTitle') as string ?? null;
				this.favIconUrl = getBrand('FoundationaLLM:Branding:FavIconUrl') as string ?? null;
				this.logoUrl = getBrand('FoundationaLLM:Branding:LogoUrl') as string ?? null;
				this.logoText = getBrand('FoundationaLLM:Branding:LogoText') as string ?? null;
				this.primaryBg = getBrand('FoundationaLLM:Branding:BackgroundColor') as string ?? null;
				this.primaryColor = getBrand('FoundationaLLM:Branding:PrimaryColor') as string ?? null;
				this.secondaryColor = getBrand('FoundationaLLM:Branding:SecondaryColor') as string ?? null;
				this.accentColor = getBrand('FoundationaLLM:Branding:AccentColor') as string ?? null;
				this.primaryText = getBrand('FoundationaLLM:Branding:PrimaryTextColor') as string ?? null;
				this.secondaryText = getBrand('FoundationaLLM:Branding:SecondaryTextColor') as string ?? null;
				this.accentText = getBrand('FoundationaLLM:Branding:AccentTextColor') as string ?? null;
				this.primaryButtonBg = getBrand('FoundationaLLM:Branding:PrimaryButtonBackgroundColor') as string ?? null;
				this.primaryButtonText = getBrand('FoundationaLLM:Branding:PrimaryButtonTextColor') as string ?? null;
				this.secondaryButtonBg = getBrand('FoundationaLLM:Branding:SecondaryButtonBackgroundColor') as string ?? null;
				this.secondaryButtonText = getBrand('FoundationaLLM:Branding:SecondaryButtonTextColor') as string ?? null;
				this.footerText = getBrand('FoundationaLLM:Branding:FooterText') as string ?? null;
				this.noAgentsMessage = getBrand('FoundationaLLM:Branding:NoAgentsMessage') as string ?? null;
				this.defaultAgentWelcomeMessage = getBrand('FoundationaLLM:Branding:DefaultAgentWelcomeMessage') as string ?? null;
				this.agentIconUrl = getBrand('FoundationaLLM:Branding:AgentIconUrl') as string ?? null;

			} catch (error: any) {
				console.error('Failed to load basic branding configuration:', error);
				// Don't throw error as this is not critical for app functionality
			}
		},

		/**
		 * Loads configuration values from the UserPortal app configuration set.
		 * This is the new approach that replaces individual API calls for marked configuration values.
		 */
		async loadAppConfigurationSet() {
			try {
				// Reset error state before attempting to load
				this.hasConfigurationAccessError = false;
				this.configurationAccessErrorMessage = null;

				const appConfigSetResults = await api.getUserPortalAppConfigurationSet();

				if (appConfigSetResults && appConfigSetResults.length > 0) {
					const configSet = appConfigSetResults[0].resource;

					if (!configSet || !configSet.configuration_values) {
						throw new Error('Invalid app configuration set structure');
					}

					const configValues = configSet.configuration_values;

					// Map configuration values from the app configuration set to store properties
					if (configValues) {

						if (configValues['FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions']) {
							this.allowedUploadFileExtensions = configValues['FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions'] as string;
						}

						// UserPortal configuration
						if (configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageRating'] !== undefined) {
							this.showMessageRating = typeof configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageRating'] === 'boolean'
								? configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageRating'] as boolean
								: JSON.parse((configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageRating'] as string).toLowerCase());
						}
						if (configValues['FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup'] !== undefined) {
							this.showLastConversionOnStartup = typeof configValues['FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup'] === 'boolean'
								? configValues['FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup'] as boolean
								: JSON.parse((configValues['FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup'] as string).toLowerCase());
						}
						if (configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageTokens'] !== undefined) {
							this.showMessageTokens = typeof configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageTokens'] === 'boolean'
								? configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageTokens'] as boolean
								: JSON.parse((configValues['FoundationaLLM:UserPortal:Configuration:ShowMessageTokens'] as string).toLowerCase());
						}
						if (configValues['FoundationaLLM:UserPortal:Configuration:ShowViewPrompt'] !== undefined) {
							this.showViewPrompt = typeof configValues['FoundationaLLM:UserPortal:Configuration:ShowViewPrompt'] === 'boolean'
								? configValues['FoundationaLLM:UserPortal:Configuration:ShowViewPrompt'] as boolean
								: JSON.parse((configValues['FoundationaLLM:UserPortal:Configuration:ShowViewPrompt'] as string).toLowerCase());
						}
						if (configValues['FoundationaLLM:UserPortal:Configuration:ShowFileUpload'] !== undefined) {
							this.showFileUpload = typeof configValues['FoundationaLLM:UserPortal:Configuration:ShowFileUpload'] === 'boolean'
								? configValues['FoundationaLLM:UserPortal:Configuration:ShowFileUpload'] as boolean
								: JSON.parse((configValues['FoundationaLLM:UserPortal:Configuration:ShowFileUpload'] as string).toLowerCase());
						}
						if (configValues['FoundationaLLM:UserPortal:Configuration:FeaturedAgentNames']) {
							const featuredAgentNamesString = configValues['FoundationaLLM:UserPortal:Configuration:FeaturedAgentNames'] as string;

							this.featuredAgentNames = featuredAgentNamesString.split(',').map((name: string) => name.trim()).filter((name: string) => name.length > 0);
							this.pinnedFeaturedAgentNames = this.featuredAgentNames.filter(name => name.endsWith('|*')).map(name => name.replace(/\|\*$/, ''));
							this.featuredAgentNames = this.featuredAgentNames.map(name => name.replace(/\|\*$/, ''));

							this.isFeaturedAgentNamesLoaded = true;
						}
						if (configValues['FoundationaLLM:UserPortal:Configuration:AgentManagementPermissionRequestUrl']) {
							this.agentManagementPermissionRequestUrl = configValues['FoundationaLLM:UserPortal:Configuration:AgentManagementPermissionRequestUrl'] as string;
						}
					}
				}
				this.isAppConfigurationSetLoaded = true;
			} catch (error: any) {
				console.error('Failed to load app configuration set:', error);

				// Check if this is a 403 Forbidden error
				if (error?.status === 403 || error?.statusCode === 403 ||
					(error?.message && error.message.includes('403')) ||
					(error?.response?.status === 403)) {
					this.hasConfigurationAccessError = true;
					this.configurationAccessErrorMessage = 'Please contact your system administrator to request access.';
					console.error('Access to UserPortal app configuration set is forbidden (403)');
				}

				throw error;
			}
		},
		/**
		 * Loads only the authentication-related configuration values using individual API calls.
		 * These are the configuration values that are not marked in red in the image.
		 * Also loads basic API URL and Instance ID needed for API calls.
		 */
		async getAuthConfigVariables() {
			const [
				apiUrl,
				instanceId,
				authClientId,
				authInstance,
				authTenantId,
				authScopes,
				authCallbackPath,
				authTimeoutInMinutes,
			] = await Promise.all([
				api.getConfigValue('FoundationaLLM:APIEndpoints:CoreAPI:Essentials:APIUrl'),
				this.getConfigValueSafe('FoundationaLLM:Instance:Id', '00000000-0000-0000-0000-000000000000'),
				api.getConfigValue('FoundationaLLM:UserPortal:Authentication:Entra:ClientId'),
				api.getConfigValue('FoundationaLLM:UserPortal:Authentication:Entra:Instance'),
				api.getConfigValue('FoundationaLLM:UserPortal:Authentication:Entra:TenantId'),
				api.getConfigValue('FoundationaLLM:UserPortal:Authentication:Entra:Scopes'),
				api.getConfigValue('FoundationaLLM:UserPortal:Authentication:Entra:CallbackPath'),
				this.getConfigValueSafe('FoundationaLLM:UserPortal:Authentication:Entra:TimeoutInMinutes', 60),
			]);

			// Set basic API configuration first
			this.apiUrl = apiUrl || null;
			this.instanceId = instanceId || null;

			this.auth.clientId = authClientId || null;
			this.auth.instance = authInstance || null;
			this.auth.tenantId = authTenantId || null;
			// Handle scopes - it can be a string or array, and we need to split if it's a string
			if (Array.isArray(authScopes)) {
				this.auth.scopes = authScopes;
			} else if (typeof authScopes === 'string') {
				// Split by comma and trim whitespace, filter out empty strings
				this.auth.scopes = authScopes.split(',').map(s => s.trim()).filter(s => s.length > 0);
			} else {
				this.auth.scopes = [];
			}
			this.auth.callbackPath = authCallbackPath || null;
			this.auth.timeoutInMinutes = authTimeoutInMinutes;
		},

		async loadFeatureFlags() {
			const agentSelfServiceFlag = await this.getConfigValueSafe(
				'.appconfig.featureflag/FoundationaLLM.Agent.SelfService',
				'{"enabled": true}',
			);
			try {
				const parsedFlag = JSON.parse(agentSelfServiceFlag);
				this.agentSelfServiceFeatureEnabled = parsedFlag.enabled === true;
			} catch (error) {
				console.error('Failed to parse agent self-service feature flag:', error);
				this.agentSelfServiceFeatureEnabled = false; // Default to enabled if parsing fails
			}
		},

		/**
		 * Helper method to safely get configuration values with default fallback.
		 */
		async getConfigValueSafe(key: string, defaultValue: any = null) {
			try {
				const value = await api.getConfigValue(key);
				if (!value) {
					return defaultValue;
				}
				return value;
			} catch (error) {
				console.error(`Failed to get config value for key ${key}:`, error);
				return defaultValue;
			}
		},

		/**
		 * Main configuration loading method that orchestrates the loading process.
		 * First loads auth config, then after authentication, loads the app configuration set.
		 */
		async getConfigVariables() {
			// Load authentication configuration first
			await this.getAuthConfigVariables();
			// Mark basic config as loaded (auth config includes logoUrl, logoText, etc.)
			this.isConfigurationLoaded = true;
		},

		/**
		 * Loads the full configuration after authentication is complete.
		 * Uses app configuration sets as the only method to load configuration.
		 */
		async loadFullConfiguration() {
			// Load the app configuration set (this is the only supported approach)
			await this.loadAppConfigurationSet();
			await this.loadFeatureFlags();
		},

		/**
		 * Loads configuration after successful authentication.
		 * This should be called from the auth store after login is complete.
		 */
		async loadConfigurationAfterAuth() {
			try {
				await this.loadFullConfiguration();
				this.isConfigurationLoaded = true;
			} catch (error: any) {
				console.error('Failed to load configuration after authentication:', error);

				// If it's a 403 error, we still mark as loaded but keep the error state
				// This allows the UI to render the access denied message
				if (this.hasConfigurationAccessError) {
					this.isConfigurationLoaded = true;
				} else {
					// For other errors, still mark as loaded so UI doesn't stay in loading state
					this.isConfigurationLoaded = true;
				}
			}
		},

	},
});
