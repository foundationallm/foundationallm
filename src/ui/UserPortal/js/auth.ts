import { LogLevel, PublicClientApplication } from '@azure/msal-browser';

const ENABLE_LOGS = false;

export interface AuthConfigOptions {
	clientId: string;
	instance: string;
	tenantId: string;
	scopes: string;
	callbackPath: string;
}

let configOptions: AuthConfigOptions = {
	clientId: '',
	instance: '',
	tenantId: '',
	scopes: '',
	callbackPath: '',
};

export function setAuthConfig(config: AuthConfigOptions) {
	configOptions = config;
}

function getMsalConfig() {
	return {
		auth: {
			clientId: configOptions.clientId,
			authority: `${configOptions.instance}${configOptions.tenantId}`,
			redirectUri: configOptions.callbackPath,
			scopes: [configOptions.scopes],
			// Must be registered as a SPA redirectURI on your app registration.
			postLogoutRedirectUri: '/',
		},
		cache: {
			cacheLocation: 'sessionStorage',
		},
		system: {
			loggerOptions: {
				loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
					if (!ENABLE_LOGS) return;

					if (containsPii) {
						return;
					}

					switch (level) {
						case LogLevel.Error:
							console.error(message);
							return;
						case LogLevel.Info:
							console.info(message);
							return;
						case LogLevel.Verbose:
							console.debug(message);
							return;
						case LogLevel.Warning:
							console.warn(message);
							return;
					}
				},
				logLevel: LogLevel.Verbose,
			},
		},
	};
}

export async function getMsalInstance() {
	const msalInstance = new PublicClientApplication(getMsalConfig());
	await msalInstance.initialize();
	return msalInstance;
}

// Add scopes here for the id token to be used at MS Identity Platform endpoints.
export function getLoginRequest() {
	const msalConfig = getMsalConfig();
	return {
		scopes: msalConfig.auth.scopes,
	};
}

// Add the endpoints here for MS Graph API services you would like to use.
export const graphConfig = {
	graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
	graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};
