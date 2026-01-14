<template>
	<!-- Access Denied Message for 403 Errors -->
	<div v-if="$appConfigStore.hasConfigurationAccessError" class="access-denied-overlay">
		<div class="access-denied-container">
			<div class="access-denied-icon">
				<i class="pi pi-ban" style="font-size: 3rem; color: #e74c3c;"></i>
			</div>
			<p class="access-denied-title">Access Denied</p>
			<p class="access-denied-message">
				{{ $appConfigStore.configurationAccessErrorMessage }}
			</p>
		</div>
	</div>

	<!-- Normal App Content (hidden when access denied) -->
	<div v-else :class="$appStore.sidebarCollapsed ? 'wrapper collapsed-sidebar' : 'wrapper'">
		<Button
			class="skip-to-main-content-button"
			role="link"
			label="Skip to main content"
			aria-label="Skip to main content"
			@click="focusMainContent"
		/>

		<Sidebar />
		<div class="page">
			<!-- Page to render -->
			<div class="page-content">
				<div
					v-show="!$appStore.sidebarCollapsed"
					class="sidebar-blur"
					@click="$appStore.sidebarCollapsed = true"
				/>
				<slot />
			</div>

			<!-- Session expired dialog -->
			<Dialog
				modal
				:visible="$authStore.isExpired"
				:closable="false"
				header="Your session has expired."
			>
				Please log in again to continue using the app.
				<template #footer>
					<Button label="Log in" primary @click="handleRefreshLogin" />
				</template>
			</Dialog>

			<footer v-if="$appConfigStore.footerText">
				<!-- eslint-disable-next-line vue/no-v-html -->
				<div class="footer-item" v-html="$appConfigStore.footerText"></div>
			</footer>
		</div>
	</div>
</template>

<script>
import '@/styles/access-denied.scss';

export default {
	methods: {
		async handleRefreshLogin() {
			await this.$authStore.logoutSilent();
			this.$router.push({ name: 'auth/login' });
		},

		focusMainContent() {
			const mainContent = document.getElementById('main-content');
			const focusableElements = mainContent.querySelectorAll(
				'a, button, input, textarea, select, details, [tabindex]:not([tabindex="-1"])',
			);
			if (focusableElements.length > 0) {
				focusableElements[0].focus();
			}
		},
	},
};
</script>

<style scoped>
.page {
	max-width: 100%;
	display: flex;
	flex-direction: column;
	height: auto;
	overflow-x: auto;
	flex: 1;
}

.collapsed-sidebar {
	flex-direction: column !important;
}

.collapsed-sidebar .page {
	padding-top: 94px;
}

.sidebar-blur {
	display: none;
}

.page-content {
	flex: 1;
}

footer {
	text-align: right;
	font-size: 0.85rem;
	margin-top: 24px;
	padding-right: 24px;
}

.skip-to-main-content-button {
	position: absolute;
	width: 1px;
	height: 1px;
	padding: 0;
	margin: -1px;
	overflow: hidden;
	clip: rect(0, 0, 0, 0);
	border: 0;
}

.skip-to-main-content-button:focus {
	position: fixed !important; /* Override sr-only */
	top: 10px !important; /* Ensure visibility */
	left: 10px !important;
	width: auto !important;
	height: auto !important;
	z-index: 100 !important;
	padding: 10px !important;
	border: 2px solid #fff !important;
	outline: none !important;
	box-shadow: 0 0 5px rgba(0, 0, 0, 0.3) !important;
	clip: auto !important; /* Remove clip hiding */
	width: auto !important;
	height: auto !important;
}

@media only screen and (max-width: 960px) {
	.page {
		padding-top: 94px;
	}

	.sidebar-blur {
		display: block;
		position: absolute;
		width: 100%;
		height: 100%;
		z-index: 2;
		top: 0px;
		left: 0px;
		backdrop-filter: blur(3px);
	}
}
</style>
