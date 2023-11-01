import { AppConfigurationClient } from '@azure/app-configuration';

let appConfigClient = null;

async function getConfigClient() {
	if (!appConfigClient) {
		// Node may try dns resolution with IPv6 first which breaks the azure app
		// configuration service requests, so we need to force it use IPv4 instead.
		if (process.server) {
			const dns = await import('node:dns');
			dns.setDefaultResultOrder('ipv4first');
		}

		appConfigClient = new AppConfigurationClient(process.env.APP_CONFIG_ENDPOINT);
	}

	return appConfigClient;
}

export async function getAppConfigSetting(key: string) {
	const client = await getConfigClient();
	const setting = await client.getConfigurationSetting({ key });
	return setting.value ? setting.value : null;
}
