import fs from 'fs';

const buildLoadingTemplate = (() => {
	const path = 'server/buildLoadingTemplate.html';

	try {
		const data = fs.readFileSync(path, 'utf8');
		return data;
	} catch (error) {
		console.error('Error reading build loading template!', error);
		return null;
	}
})();

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	compatibilityDate: '2024-08-27',
	ssr: false,
	devtools: { enabled: true },
	modules: ['@pinia/nuxt', '@nuxtjs/eslint-module', 'floating-vue/nuxt'],
	components: true,
	app: {
		head: {
			title: process.env.BRANDING_PAGE_TITLE ?? 'FoundationaLLM',
			htmlAttrs: {
				lang: 'en',
			},
			link: [
				{
					rel: 'icon',
					type: 'image/x-icon',
					href: process.env.BRANDING_FAV_ICON_URL ?? '/favicon.ico',
				},
			],
		},
	},
	routeRules: {
		'*': { ssr: false },
	},
	css: [
		'primevue/resources/themes/viva-light/theme.css',
		'~/styles/fonts.scss',
		'primeicons/primeicons.css',
	],
	build: {
		transpile: ['primevue'],
	},
	hooks: {
		'vite:extendConfig': (config, { isClient /*, isServer */ }) => {
			if (isClient) {
				config.resolve.alias.vue = 'vue/dist/vue.esm-bundler.js';
			}
		},
	},
	devServer: {
		...(buildLoadingTemplate ? { loadingTemplate: () => buildLoadingTemplate } : {}),
		port: 3000,
	},
	runtimeConfig: {
		APP_CONFIG_ENDPOINT: process.env.APP_CONFIG_ENDPOINT,
		public: {
			LOCAL_API_URL: process.env.LOCAL_API_URL,
		},
	},
});
