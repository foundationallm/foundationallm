// Node may try dns resolution with IPv6 first which breaks the azure app
// configuration service requests, so we need to force it use IPv4 instead.
import dns from 'node:dns';
dns.setDefaultResultOrder('ipv4first');

import { AppConfigurationClient } from '@azure/app-configuration';

export default defineEventHandler(async (event) => {
	const key = getRouterParam(event, 'key');
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
