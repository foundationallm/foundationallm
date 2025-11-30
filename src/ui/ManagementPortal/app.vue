<template>
	<Head>
		<Title>{{ pageTitle }}</Title>
		<Meta name="description" :content="pageTitle" />
		<Link rel="icon" type="image/x-icon" :href="iconLinkComputed" />
	</Head>

	<div class="main-content">
		<NuxtLayout>
			<NuxtPage />
		</NuxtLayout>
		<Toast position="top-center" />
		<ConfirmationDialog />
	</div>
</template>

<script lang="ts">
export default {
	data() {
		return {
			pageTitle: 'FoundationaLLM Management',
		};
	},

	computed: {
		iconLinkComputed() {
			const appConfigStore = this.$appConfigStore;
			return (appConfigStore && appConfigStore.favIconUrl) ? appConfigStore.favIconUrl : '/favicon.ico';
		},
		style() {
			const appConfigStore = this.$appConfigStore;
			if (!appConfigStore) {
				return {};
			}
			return {
				'--primary-bg': appConfigStore.primaryBg,
				'--primary-color': appConfigStore.primaryColor,
				'--secondary-color': appConfigStore.secondaryColor,
				'--accent-color': appConfigStore.accentColor,
				'--primary-text': appConfigStore.primaryText,
				'--secondary-text': appConfigStore.secondaryText,
				'--accent-text': appConfigStore.accentText,
				'--primary-button-bg': appConfigStore.primaryButtonBg,
				'--primary-button-text': appConfigStore.primaryButtonText,
				'--secondary-button-bg': appConfigStore.secondaryButtonBg,
				'--secondary-button-text': appConfigStore.secondaryButtonText,
			};
		},
	},

	watch: {
		style: {
			immediate: true,
			handler() {
				for (const cssVar in this.style) {
					document.documentElement.style.setProperty(cssVar, this.style[cssVar]);
				}
			},
		},
	},
};
</script>

<style lang="scss">
html,
body,
#__nuxt,
#__layout,
.main-content {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}

.wrapper {
	display: flex;
	flex-direction: row;
	height: 100vh;
	flex-wrap: wrap;
}

.page {
	padding: 32px;
	padding-top: 24px;
	padding-bottom: 24px;
	overflow-y: auto;
	max-height: 100%;
	flex-grow: 1;
}

.page-header {
	margin-top: 0px;
	margin-bottom: 8px;
}

.page-subheader {
	margin-bottom: 24px;
}

.p-component {
	border-radius: 0px;
}

.p-button-text {
	color: var(--primary-button-bg) !important;
}

.p-button:not(.p-button-text):not(.p-togglebutton) {
	background-color: var(--primary-button-bg);
	border-color: var(--primary-button-bg);
	color: var(--primary-button-text);

	& > * {
		color: var(--primary-button-text);
	}
}

.p-button-secondary:not(.p-button-text):not(.p-togglebutton) {
	background-color: var(--secondary-button-bg);
	border-color: var(--secondary-button-bg);
	color: var(--secondary-button-text);

	& > * {
		color: var(--secondary-button-text);
	}
}

.p-togglebutton:not(.p-button-text) {
	background-color: var(--primary-button-bg);
	border-color: var(--primary-button-bg);
	color: var(--primary-button-text);

	& > * {
		color: var(--primary-button-text);
	}
}

.p-datatable-header {
	padding-left: 0;
	padding-right: 0;
}

:root {
	--jse-theme-color: var(--primary-button-bg, #6366f1); /* Default theme color */
}

a.table__button[aria-disabled="true"],
.table__button[aria-disabled="true"] {
	pointer-events: none;
	opacity: 0.6;
	cursor: default;
}

.input-error {
	border: 1.5px solid #e53935 !important;
	background: #fff0f0;
}
.error-message {
	color: #e53935;
	font-size: 0.95em;
	margin-top: 2px;
}
</style>
