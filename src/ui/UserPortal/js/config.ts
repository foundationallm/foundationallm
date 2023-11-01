// Node may try dns resolution with IPv6 first which breaks the azure app
// configuration service requests, so we need to force it use IPv4 instead.
if (process.server) {
	const dns = require('node:dns');
	dns.setDefaultResultOrder('ipv4first');
}

import { AppConfigurationClient } from '@azure/app-configuration';

let appConfigClient = null;

function getConfigClient() {
	if (!appConfigClient) {
		appConfigClient = new AppConfigurationClient(process.env.APP_CONFIG_ENDPOINT);
	}

	return appConfigClient;
}

export async function getAppConfigSetting(key: string) {
	const setting = await getConfigClient().getConfigurationSetting({ key });
	return setting.value ? setting.value : null;
}
