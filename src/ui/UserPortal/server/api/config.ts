// Node may try dns resolution with IPv6 first which breaks the azure app
// configuration service requests, so we need to force it use IPv4 instead.
import dns from 'node:dns';
dns.setDefaultResultOrder('ipv4first');

import { AppConfigurationClient } from '@azure/app-configuration';

const validKeys = [
	'FoundationaLLM:APIs:CoreAPI:APIUrl',
	'.appconfig.featureflag/FoundationaLLM-AllowAgentHint',
	'FoundationaLLM:Branding:AllowAgentSelection',
	'FoundationaLLM:Branding:KioskMode',
	'FoundationaLLM:Branding:PageTitle',
	'FoundationaLLM:Branding:LogoUrl',
	'FoundationaLLM:Branding:LogoText',
	'FoundationaLLM:Branding:BackgroundColor',
	'FoundationaLLM:Branding:PrimaryColor',
	'FoundationaLLM:Branding:SecondaryColor',
	'FoundationaLLM:Branding:AccentColor',
	'FoundationaLLM:Branding:PrimaryTextColor',
	'FoundationaLLM:Branding:SecondaryTextColor',
	'FoundationaLLM:Chat:Entra:ClientId',
	'FoundationaLLM:Chat:Entra:Instance',
	'FoundationaLLM:Chat:Entra:TenantId',
	'FoundationaLLM:Chat:Entra:Scopes',
	'FoundationaLLM:Chat:Entra:CallbackPath',
];

export default defineEventHandler(async (event) => {
	const query = getQuery(event);
	const key = query.key;

	if (!validKeys.includes(key)) {
		setResponseStatus(event, 404, `Config value ${key} not found.`);
		return '404';
	}

	const config = useRuntimeConfig();
	const appConfigClient = new AppConfigurationClient(config.APP_CONFIG_ENDPOINT);

	try {
		const setting = await appConfigClient.getConfigurationSetting({ key });
		return setting.value;
	} catch (error) {
		setResponseStatus(event, 404, `Config value ${key} not found.`);
		return '404';
	}
});
