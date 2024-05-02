<template>
	<div class="wrapper">
		<Sidebar />
		<div class="page">
			<!-- Page to render -->
			<slot />

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
		</div>
	</div>
</template>

<script>
export default {
	methods: {
		async handleRefreshLogin() {
			await this.$authStore.logoutSilent();
			this.$router.push({ name: 'auth/login' });
		},
	},
};
</script>
