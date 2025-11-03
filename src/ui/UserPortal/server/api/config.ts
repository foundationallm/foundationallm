// Node may try dns resolution with IPv6 first which breaks the azure app
// configuration service requests, so we need to force it use IPv4 instead.
import dns from 'node:dns';
import { AppConfigurationClient } from '@azure/app-configuration';

dns.setDefaultResultOrder('ipv4first');

const allowedFilters = [
	'FoundationaLLM:Branding:*'
]

const allowedKeys = [
	'FoundationaLLM:APIEndpoints:CoreAPI:Essentials:APIUrl',
	'.appconfig.featureflag/FoundationaLLM-AllowAgentHint',
	'FoundationaLLM:Branding:AllowAgentSelection',
	'FoundationaLLM:Branding:KioskMode',
	'FoundationaLLM:Branding:PageTitle',
	'FoundationaLLM:Branding:FavIconUrl',
	'FoundationaLLM:Branding:AgentIconUrl',
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
	'FoundationaLLM:Branding:NoAgentsMessage',
	'FoundationaLLM:Branding:DefaultAgentWelcomeMessage',
	'FoundationaLLM:Instance:Id',
	'FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup',
	'FoundationaLLM:UserPortal:Authentication:Entra:ClientId',
	'FoundationaLLM:UserPortal:Authentication:Entra:Instance',
	'FoundationaLLM:UserPortal:Authentication:Entra:TenantId',
	'FoundationaLLM:UserPortal:Authentication:Entra:Scopes',
	'FoundationaLLM:UserPortal:Authentication:Entra:CallbackPath',
	'FoundationaLLM:UserPortal:Authentication:Entra:TimeoutInMinutes',
	'FoundationaLLM:UserPortal:Configuration:ShowMessageRating',
	'FoundationaLLM:UserPortal:Configuration:ShowViewPrompt',
	'FoundationaLLM:UserPortal:Configuration:ShowMessageTokens',
	'FoundationaLLM:UserPortal:Configuration:ShowFileUpload',
	'FoundationaLLM:UserPortal:Configuration:FeaturedAgentNames',
	'FoundationaLLM:UserPortal:Configuration:AgentManagementPermissionRequestUrl',
	'FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions',
];

export default defineEventHandler(async (event) => {
	const query = getQuery(event);
	const key = query.key as string;
	const filter = query.filter as string;

	// Respond with a 400 (Bad Request) if the key to access was not provided.
	if (!key && !filter) {
		console.error('None of the valid query items "key" or "filter" were provided.');
		setResponseStatus(event, 400, 'None of the valid query items "key" or "filter" were provided.');
		return '400';
	}

	if (key && filter) {
		console.error('Only one of the query items "key" or "filter" should be provided.');
		setResponseStatus(event, 400, 'Only one of the query items "key" or "filter" should be provided.');
		return '400';
	}

	// Respond with a 403 (Forbidden) if the key is not in the allowed keys list.
	if (key && !allowedKeys.includes(key)) {
		console.error(
			`Config value "${key}" is not allowed to be accessed, please add it to the list of allowed keys if required.`,
		);
		setResponseStatus(event, 403, `Config value "${key}" is not an accessible key.`);
		return '403';
	}

	// Respond with a 403 (Forbidden) if the filter is not in the allowed filters list.
	if (filter && !allowedFilters.includes(filter)) {
		console.error(
			`Config filter "${filter}" is not allowed to be accessed, please add it to the list of allowed filters if required.`,
		);
		setResponseStatus(event, 403, `Config filter "${filter}" is not an accessible filter.`);
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

	if (key) {
		try {
			const setting = await appConfigClient.getConfigurationSetting({ key: key as string });
			return setting.value;
		} catch (error) {
			console.error(
				`Failed to load the configuration value for "${key}", please ensure it exists and has the correct format.`,
			);
			return null;
		}
	} else {
		try {
			const settings: Record<string, string | null> = {};
			for await (const s of appConfigClient.listConfigurationSettings({ keyFilter: filter })) {
					settings[s.key] = s.value;
			}
			return settings;
		} catch (error) {
			console.error(
				`Failed to load the configuration values for "${filter}", please ensure they exist and have the correct format.`,
			);
			return null;
		}
	}
});
