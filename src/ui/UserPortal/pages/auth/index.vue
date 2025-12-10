<template>
	<div class="login-page">
		<div class="login-container">
			<img 
				:src="appConfigStore.logoUrl || '/foundationallm-logo-white.svg'" 
				class="login__logo" 
				:alt="appConfigStore.logoText || 'FoundationaLLM'" 
				@error="handleImageError"
			/>
			<Button icon="pi pi-microsoft" label="Sign in" size="large" @click="signIn"></Button>
			<div v-if="route.query.message" class="login__message">{{ route.query.message }}</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAuthStore } from '@/stores/authStore';

definePageMeta({
	name: 'auth/login',
	path: '/signin-oidc',
});

const appConfigStore = useAppConfigStore();
const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();

onMounted(async () => {
	// If user is already authenticated, redirect to home
	if (authStore.isAuthenticated) {
		await router.push({ path: '/' });
	}
});

async function signIn() {
	await authStore.login();
}

function handleImageError(event: Event) {
	console.error('Logo image failed to load:', event);
	const img = event.target as HTMLImageElement;
	// Fallback to local asset if remote URL fails
	img.src = '/foundationallm-logo-white.svg';
}
</script>

<style lang="scss" scoped>
.login-page {
	display: flex;
	align-items: center;
	justify-content: center;
	height: 100%;
	background-color: var(--primary-color, #131833);
	background: linear-gradient(45deg, var(--primary-color, #131833) 0%, var(--secondary-color, #334581) 50%);
}

.login-container {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	width: 500px;
	height: auto;
	padding: 48px;
	background-color: rgba(255, 255, 255, 0.05);
	backdrop-filter: blur(300px);
}

.login__logo {
	width: 300px;
	height: auto;
	margin-bottom: 48px;
}

.login__message {
	margin-top: 16px;
	padding: 16px;
	color: var(--primary-text);
	font-size: 1rem;
}
</style>
