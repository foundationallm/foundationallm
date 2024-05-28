import { defineStore } from 'pinia';
// import type { AuthConfigOptions } from '@js/auth';
import api from '@/js/api';

export const useAppConfigStore = defineStore('appConfig', {
	state: () => ({
		// API: Defines API-specific settings such as the base URL for application requests.
		apiUrl: null,
		authorizationApiUrl: null,
		coreApiUrl: null,
		gatekeeperApiUrl: null,
		gatekeeperIntegrationApiUrl: null,
		gatewayApiUrl: null,
		langChainApiUrl: null,
		orchestrationApiUrl: null,
		semanticKernelApiUrl: null,
		vectorizationApiUrl: null,
		vectorizationWorkerApiUrl: null,

		instanceId: null,

		// Style: These settings impact the visual style of the chat interface.
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

		// Auth: These settings configure the MSAL authentication.
		auth: {
			clientId: null,
			instance: null,
			tenantId: null,
			scopes: [],
			callbackPath: null,
		}, // as AuthConfigOptions,
	}),
	getters: {},
	actions: {
		async getConfigVariables() {
			const [
				apiUrl,
				authorizationApiUrl,
				coreApiUrl,
				gatekeeperApiUrl,
				gatekeeperIntegrationApiUrl,
				gatewayApiUrl,
				langChainApiUrl,
				orchestrationApiUrl,
				semanticKernelApiUrl,
				vectorizationApiUrl,
				vectorizationWorkerApiUrl,
				instanceId,
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
				authClientId,
				authInstance,
				authTenantId,
				authScopes,
				authCallbackPath,
			] = await Promise.all([
				api.getConfigValue('FoundationaLLM:APIs:ManagementAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:AuthorizationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:CoreAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:GatekeeperAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:GatewayAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:LangChainAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:OrchestrationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:SemanticKernelAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:VectorizationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:VectorizationWorker:APIUrl'),

				api.getConfigValue('FoundationaLLM:Instance:Id'),

				api.getConfigValue('FoundationaLLM:Branding:LogoUrl'),
				api.getConfigValue('FoundationaLLM:Branding:LogoText'),
				api.getConfigValue('FoundationaLLM:Branding:BackgroundColor'),
				api.getConfigValue('FoundationaLLM:Branding:PrimaryColor'),
				api.getConfigValue('FoundationaLLM:Branding:SecondaryColor'),
				api.getConfigValue('FoundationaLLM:Branding:AccentColor'),
				api.getConfigValue('FoundationaLLM:Branding:PrimaryTextColor'),
				api.getConfigValue('FoundationaLLM:Branding:SecondaryTextColor'),
				api.getConfigValue('FoundationaLLM:Branding:AccentTextColor'),
				api.getConfigValue('FoundationaLLM:Branding:PrimaryButtonBackgroundColor'),
				api.getConfigValue('FoundationaLLM:Branding:PrimaryButtonTextColor'),
				api.getConfigValue('FoundationaLLM:Branding:SecondaryButtonBackgroundColor'),
				api.getConfigValue('FoundationaLLM:Branding:SecondaryButtonTextColor'),

				api.getConfigValue('FoundationaLLM:Management:Entra:ClientId'),
				api.getConfigValue('FoundationaLLM:Management:Entra:Instance'),
				api.getConfigValue('FoundationaLLM:Management:Entra:TenantId'),
				api.getConfigValue('FoundationaLLM:Management:Entra:Scopes'),
				api.getConfigValue('FoundationaLLM:Management:Entra:CallbackPath'),
			]);

			this.apiUrl = apiUrl;
			this.authorizationApiUrl = authorizationApiUrl;
			this.coreApiUrl = coreApiUrl;
			this.gatekeeperApiUrl = gatekeeperApiUrl;
			this.gatekeeperIntegrationApiUrl = gatekeeperIntegrationApiUrl;
			this.gatewayApiUrl = gatewayApiUrl;
			this.langChainApiUrl = langChainApiUrl;
			this.orchestrationApiUrl = orchestrationApiUrl;
			this.semanticKernelApiUrl = semanticKernelApiUrl;
			this.vectorizationApiUrl = vectorizationApiUrl;
			this.vectorizationWorkerApiUrl = vectorizationWorkerApiUrl;

			this.instanceId = instanceId;

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

			this.auth.clientId = authClientId;
			this.auth.instance = authInstance;
			this.auth.tenantId = authTenantId;
			this.auth.scopes = authScopes;
			this.auth.callbackPath = authCallbackPath;
		},
	},
});
