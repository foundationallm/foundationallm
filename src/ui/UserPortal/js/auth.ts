import { LogLevel, PublicClientApplication } from '@azure/msal-browser';
import getAppConfigSetting from './config';

let isConfigLoaded = false;
let AUTH_CLIENT_ID: string;
let AUTH_INSTANCE: string;
let AUTH_TENANT_ID: string;
let AUTH_SCOPES: string;
let AUTH_CLIENT_SECRET: string;
let AUTH_CALLBACK_PATH: string;

async function loadConfig() {
	if (!isConfigLoaded) {
		AUTH_CLIENT_ID = await getAppConfigSetting("FoundationaLLM:Chat:Entra:ClientId") as string;
		AUTH_INSTANCE = await getAppConfigSetting("FoundationaLLM:Chat:Entra:Instance") as string;
		AUTH_TENANT_ID = await getAppConfigSetting("FoundationaLLM:Chat:Entra:TenantId") as string;
		AUTH_SCOPES = await getAppConfigSetting("FoundationaLLM:Chat:Entra:Scopes") as string;
		AUTH_CLIENT_SECRET = await getAppConfigSetting("FoundationaLLM:Chat:Entra:ClientSecret") as string;
		AUTH_CALLBACK_PATH = await getAppConfigSetting("FoundationaLLM:Chat:Entra:CallbackPath") as string;
		isConfigLoaded = true;
  	}
}

function getMsalConfig() {
	if (!isConfigLoaded) {
	  throw new Error('Config not loaded. Ensure loadConfig() is called and awaited on before accessing msalConfig.');
	}
  
	// Now that we have ensured the config is loaded, we return the msalConfig object.
	return {
	  auth: {
		clientId: AUTH_CLIENT_ID,
		authority: `${AUTH_INSTANCE}${AUTH_TENANT_ID}`,
		clientSecret: AUTH_CLIENT_SECRET,
		redirectUri: AUTH_CALLBACK_PATH,
		scopes: [AUTH_SCOPES],
		postLogoutRedirectUri: '/', // Must be registered as a SPA redirectURI on your app registration
	  },
	  cache: {
		cacheLocation: 'sessionStorage',
	  },
	  system: {
		loggerOptions: {
		  loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
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
	await loadConfig();
	let msalInstance = new PublicClientApplication(getMsalConfig());
	await msalInstance.initialize();
	return msalInstance;
}

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export async function getLoginRequest() {
	await loadConfig();
	const msalConfig = getMsalConfig(); // This will throw an error if the config isn't loaded yet
	return {
	  scopes: msalConfig.auth.scopes,
	};
  }

// Add here the endpoints for MS Graph API services you would like to use.
export const graphConfig = {
	graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
	graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};
