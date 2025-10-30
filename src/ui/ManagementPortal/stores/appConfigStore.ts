import { defineStore } from 'pinia';
import api from '@/js/api';

export const useAppConfigStore = defineStore('appConfig', {
	state: () => ({
		// API: Defines API-specific settings such as the base URL for application requests.
		apiUrl: null,
		authorizationApiUrl: null,
		coreApiUrl: null,
		stateApiUrl: null,
		// gatekeeperApiUrl: null,
		// gatekeeperIntegrationApiUrl: null,
		// gatewayApiUrl: null,
		// langChainApiUrl: null,
		// orchestrationApiUrl: null,
		// semanticKernelApiUrl: null,
		// vectorizationApiUrl: null,
		// vectorizationWorkerApiUrl: null,

		instanceId: null,

		// Style: These settings impact the visual style of the chat interface.
		favIconUrl: null,
		logoUrl: null,
		logoText: null,
		primaryBg: null,
		primaryColor: null,
		secondaryColor: null,
		accentColor: null,
		primaryText: null,
		secondaryText: null,
		accentText: null,
		primaryButtonBg: null,
		primaryButtonText: null,
		secondaryButtonBg: null,
		secondaryButtonText: null,
		footerText: null,
		allowedUploadFileExtensions: null,
		noAgentsMessage: null,
		defaultAgentWelcomeMessage: null,

		agentPrivateStoreFeatureFlag: false,

		// Auth: These settings configure the MSAL authentication.
		auth: {
			clientId: null,
			instance: null,
			tenantId: null,
			scopes: [],
			callbackPath: null,
		},
	}),
	getters: {},
	actions: {
		async getConfigVariables() {
			const getConfigValueSafe = async (key: string, defaultValue: any = null) => {
				try {
					return await api.getConfigValue(key);
				} catch (error) {
					console.error(`Failed to get config value for key ${key}:`, error);
					return defaultValue;
				}
			};

			const [
				apiUrl,
				authorizationApiUrl,
				// coreApiUrl,
				// stateApiUrl,
				// gatekeeperApiUrl,
				// gatekeeperIntegrationApiUrl,
				// gatewayApiUrl,
				// langChainApiUrl,
				// orchestrationApiUrl,
				// semanticKernelApiUrl,
				// vectorizationApiUrl,
				// vectorizationWorkerApiUrl,
				favIconUrl,
				logoUrl,
				logoText,
				primaryBg,
				primaryColor,
				secondaryColor,
				accentColor,
				primaryText,
				secondaryText,
				accentText,
				primaryButtonBg,
				primaryButtonText,
				secondaryButtonBg,
				secondaryButtonText,
				footerText,
				noAgentsMessage,
				defaultAgentWelcomeMessage,
				agentPrivateStoreFeatureFlag,
				instanceId,
				allowedUploadFileExtensions,
				authClientId,
				authInstance,
				authTenantId,
				authScopes,
				authCallbackPath,
			] = await Promise.all([
				api.getConfigValue('FoundationaLLM:APIEndpoints:ManagementAPI:Essentials:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:CoreAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:GatewayAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials:APIUrl'),
				// api.getConfigValue('FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials:APIUrl'),

				getConfigValueSafe('FoundationaLLM:Branding:FavIconUrl'),
				getConfigValueSafe('FoundationaLLM:Branding:LogoUrl', 'foundationallm-logo-white.svg'),
				getConfigValueSafe('FoundationaLLM:Branding:LogoText'),
				getConfigValueSafe('FoundationaLLM:Branding:BackgroundColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryColor', '#131833'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryColor', '#334581'),
				getConfigValueSafe('FoundationaLLM:Branding:AccentColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:AccentTextColor', '#131833'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryButtonBackgroundColor', '#5472d4'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryButtonTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryButtonBackgroundColor', '#70829a'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryButtonTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:FooterText'),
				getConfigValueSafe(
					'FoundationaLLM:Branding:NoAgentsMessage',
					'No agents available. Please check with your system administrator for assistance.',
				),
				getConfigValueSafe(
					'FoundationaLLM:Branding:DefaultAgentWelcomeMessage',
					'Start the conversation using the text box below.',
				),
				getConfigValueSafe(
					'.appconfig.featureflag/FoundationaLLM.Agent.PrivateStore',
					'{"enabled": false}',
				),
				getConfigValueSafe('FoundationaLLM:Instance:Id', '00000000-0000-0000-0000-000000000000'),
				getConfigValueSafe(
					'FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:AllowedUploadFileExtensions',
					'pdf',
				),

				api.getConfigValue('FoundationaLLM:ManagementPortal:Authentication:Entra:ClientId'),
				api.getConfigValue('FoundationaLLM:ManagementPortal:Authentication:Entra:Instance'),
				api.getConfigValue('FoundationaLLM:ManagementPortal:Authentication:Entra:TenantId'),
				api.getConfigValue('FoundationaLLM:ManagementPortal:Authentication:Entra:Scopes'),
				api.getConfigValue('FoundationaLLM:ManagementPortal:Authentication:Entra:CallbackPath'),
			]);

			this.apiUrl = apiUrl;
			this.authorizationApiUrl = authorizationApiUrl;
			// this.coreApiUrl = coreApiUrl;
			// this.stateApiUrl = stateApiUrl;
			// this.gatekeeperApiUrl = gatekeeperApiUrl;
			// this.gatekeeperIntegrationApiUrl = gatekeeperIntegrationApiUrl;
			// this.gatewayApiUrl = gatewayApiUrl;
			// this.langChainApiUrl = langChainApiUrl;
			// this.orchestrationApiUrl = orchestrationApiUrl;
			// this.semanticKernelApiUrl = semanticKernelApiUrl;
			// this.vectorizationApiUrl = vectorizationApiUrl;
			// this.vectorizationWorkerApiUrl = vectorizationWorkerApiUrl;

			this.instanceId = instanceId;

			this.favIconUrl = favIconUrl;
			this.logoUrl = logoUrl;
			this.logoText = logoText;
			this.primaryBg = primaryBg;
			this.primaryColor = primaryColor;
			this.secondaryColor = secondaryColor;
			this.accentColor = accentColor;
			this.primaryText = primaryText;
			this.secondaryText = secondaryText;
			this.accentText = accentText;
			this.primaryButtonBg = primaryButtonBg;
			this.primaryButtonText = primaryButtonText;
			this.secondaryButtonBg = secondaryButtonBg;
			this.secondaryButtonText = secondaryButtonText;
			this.footerText = footerText;
			this.allowedUploadFileExtensions = allowedUploadFileExtensions;
			this.noAgentsMessage = noAgentsMessage;
			this.defaultAgentWelcomeMessage = defaultAgentWelcomeMessage;
			this.agentPrivateStoreFeatureFlag = JSON.parse(agentPrivateStoreFeatureFlag)?.enabled;

			this.auth.clientId = authClientId;
			this.auth.instance = authInstance;
			this.auth.tenantId = authTenantId;
			this.auth.scopes = authScopes;
			this.auth.callbackPath = authCallbackPath;
		},
	},
});
