<template>
	<div class="login-page">
		<img :src="logoUrl" class="logo" />
		<Button icon="pi pi-sign-in" label="Sign In" size="large" @click="signIn"></Button>
	</div>
</template>

<script lang="ts">
import { mapStores } from 'pinia';
import { appConfig } from '@/stores/appConfig';
import { getMsalInstance, getLoginRequest } from '@/js/auth';

export default {
	name: 'Login',

	computed: {
		...mapStores(appConfig),

		logoUrl() {
			return this.appConfigStore.logoUrl;
		},
	},

	methods: {
		async signIn() {
			const loginRequest = await getLoginRequest();
			const msalInstance = await getMsalInstance();
			const response = await msalInstance.loginPopup(loginRequest);
			if (response.account) {
				this.$router.push({ path: '/', query: this.$nuxt._route.query });
			}
		},
	},
};
</script>

<style lang="scss" scoped>
.login-page {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	height: 100%;
	background-color: var(--primary-bg);
	background-image: url('/assets/splash.png');
	background-size: cover;
}

.logo {
	width: 300px;
	height: auto;
	margin-bottom: 24px;
}
</style>
