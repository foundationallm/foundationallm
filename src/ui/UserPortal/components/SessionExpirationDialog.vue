<template>
	<Dialog
		modal
		:visible="showDialog && $route.name !== 'auth/login'"
		:closable="false"
		header="Your session is about to expire"
	>
		For security and protection of your personal data, you will be logged out in
		<h2 class="flex justify-center" :class="{ 'text--danger': minutes === 0 && seconds < 30 }">
			{{ pad(minutes) }}:{{ pad(seconds) }}
		</h2>
		Would you like to stay logged in?
		<template #footer>
			<Button label="No, log out" severity="secondary" @click="handleLogout" />
			<Button label="Yes, extend my session" autofocus @click="handleStayLoggedIn" />
		</template>
	</Dialog>
</template>

<script lang="ts">
export default {
	data() {
		return {
			showDialog: false,
			dialogInterval: null,
			countdownInterval: null,
			minutes: 5,
			seconds: 0,
		};
	},

	mounted() {
		this.startDialogTimer();
	},

	beforeUnmount() {
		clearInterval(this.dialogInterval);
		clearInterval(this.countdownInterval);
	},

	methods: {
		pad(value: number): string {
			return value.toString().padStart(2, '0');
		},

		startDialogTimer() {
			// Show the dialog every configured interval
			const timeoutInMinutes = this.$appConfigStore.auth.timeoutInMinutes;
			const timeoutInMS = timeoutInMinutes * 60 * 1000;

			this.dialogInterval = setInterval(() => {
				this.triggerDialog();
			}, timeoutInMS);
		},

		triggerDialog() {
			this.startCountdown();
			this.showDialog = true;
		},

		startCountdown() {
			// 5 minutes from now
			const sessionExpirationDate = Date.now() + 5 * 60 * 1000;

			this.countdownInterval = setInterval(() => {
				const difference = sessionExpirationDate - Date.now();
				if (difference <= 0) {
					this.handleLogout();
					return 0;
				}

				this.minutes = Math.floor((difference / 1000 / 60) % 60);
				this.seconds = Math.floor((difference / 1000) % 60);
			}, 500);
		},

		async handleLogout() {
			clearInterval(this.dialogInterval);
			clearInterval(this.countdownInterval);
			await this.$authStore.logoutSilent();
			// this.$router.push({ name: 'auth/login' });
		},

		async handleStayLoggedIn() {
			clearInterval(this.dialogInterval);
			clearInterval(this.countdownInterval);
			this.countdownInterval = null;
			
			try {
				await this.$authStore.getApiToken();
				this.showDialog = false;
				this.startDialogTimer();  // Restart the timer for next session check
			} catch (error) {
				console.error('Failed to extend session:', error);
				// Token refresh failed, log user out
				await this.handleLogout();
			}
		},
	},
};
</script>

<style lang="scss"></style>
