// Node may try dns resolution with IPv6 first which breaks the azure app
// configuration service requests, so we need to force it use IPv4 instead.
import dns from 'node:dns';
import { AppConfigurationClient } from '@azure/app-configuration';

dns.setDefaultResultOrder('ipv4first');

const allowedKeys = [
	'FoundationaLLM:APIEndpoints:ManagementAPI:Essentials:APIUrl',
	'FoundationaLLM:Instance:Id',

	'FoundationaLLM:ManagementPortal:Authentication:Entra:ClientId',
	'FoundationaLLM:ManagementPortal:Authentication:Entra:Instance',
	'FoundationaLLM:ManagementPortal:Authentication:Entra:TenantId',
	'FoundationaLLM:ManagementPortal:Authentication:Entra:Scopes',
	'FoundationaLLM:ManagementPortal:Authentication:Entra:CallbackPath',

	'FoundationaLLM:Branding:FavIconUrl',
	'FoundationaLLM:Branding:LogoUrl',
	'FoundationaLLM:Branding:LogoText',
	'FoundationaLLM:Branding:BackgroundColor',
	'FoundationaLLM:Branding:PrimaryColor',
	'FoundationaLLM:Branding:SecondaryColor',
	'FoundationaLLM:Branding:AccentColor',
	'FoundationaLLM:Branding:PrimaryTextColor',
	'FoundationaLLM:Branding:SecondaryTextColor',
	'FoundationaLLM:Branding:AccentTextColor',
	'FoundationaLLM:Branding:PrimaryButtonBackgroundColor',
	'FoundationaLLM:Branding:PrimaryButtonTextColor',
	'FoundationaLLM:Branding:SecondaryButtonBackgroundColor',
	'FoundationaLLM:Branding:SecondaryButtonTextColor',
	'FoundationaLLM:Branding:FooterText',
	'FoundationaLLM:Instance:Id',
	// 'FoundationaLLM:APIEndpoints:AgentHubAPI:Essentials:APIUrl',
	'FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:APIUrl',
	'FoundationaLLM:APIEndpoints:CoreAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:DataSourceHubAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:GatekeeperAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:GatewayAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:PromptHubAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials:APIUrl',
	// 'FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials:APIUrl',
	'FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIUrl',
];

export default defineEventHandler(async (event) => {
	const query = getQuery(event);
	const key = query.key;

	// Respond with a 400 (Bad Request) if the key to access was not provided.
	if (!key) {
		console.error('The query item "key" was not provided.');
		setResponseStatus(event, 400, 'The query item "key" was not provided.');
		return '400';
	}

	// Respond with a 403 (Forbidden) if the key is not in the allowed keys list.
	if (!allowedKeys.includes(key)) {
		console.error(
			`Config value "${key}" is not allowed to be accessed, please add it to the list of allowed keys if required.`,
		);
		setResponseStatus(event, 403, `Config value "${key}" is not an accessible key.`);
		return '403';
	}

	// Respond with a 500 (Internal Server Error) if the APP_CONFIG_ENDPOINT env is not set.
	const config = useRuntimeConfig();
	if (!config.APP_CONFIG_ENDPOINT) {
		console.error('APP_CONFIG_ENDPOINT env not found. Please ensure it is set.');
		setResponseStatus(event, 500, `Configuration endpoint missing.`);
		return '500';
	}

	// This will throw a 500 (Internal Server Error) with an error if the connection string is invalid.
	const appConfigClient = new AppConfigurationClient(config.APP_CONFIG_ENDPOINT);

	try {
		const setting = await appConfigClient.getConfigurationSetting({ key });
		return setting.value;
	} catch (error) {
		// Respond with a 404 (Not Found) if the key does not exist in the Azure config service.
		console.error(
			`Failed to load config value for "${key}", please ensure it exists and is the correct format.`,
		);
		setResponseStatus(event, 404, `Config value "${key}" not found.`);
		return '404';
	}
});
