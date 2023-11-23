<template>
	<div class="navbar" :class="{ 'navbar-collapsed': closeSidebar }">
		<!-- Sidebar header -->
		<div class="navbar__header">
			<img v-if="logoURL !== ''" :src="logoURL" />
			<span v-else>{{ logoText }}</span>

			<template v-if="!appConfigStore.isKioskMode">
				<Button v-if="isSidebarClosed" icon="pi pi-arrow-right" size="small" severity="secondary" @click="closeSidebar(false)" />
				<Button v-else icon="pi pi-arrow-left" size="small" severity="secondary" @click="closeSidebar(true)" />
			</template>
		</div>

		<!-- Navbar content -->
		<div class="navbar__content">
			<div class="navbar__content__left">
				<div class="navbar__content__left__item">
					<template v-if="currentSession">
						<span>{{ currentSession.name }}</span>
						<Button
							v-if="!appConfigStore.isKioskMode"
							v-tooltip.bottom="'Copy link to chat session'"
							class="button--share"
							icon="pi pi-copy"
							text
							severity="secondary"
							@click="handleCopySession"
						/>
						<Toast position="top-center" />
					</template>
					<template v-else>
						<span>Please select a session</span>
					</template>
				</div>

				<div class="navbar__content__left__item">
					<template v-if="currentSession && allowAgentHint">
						<Dropdown
							v-model="agentSelection"
							:options="agents"
							optionLabel="label"
							placeholder="--Select--"
							@change="handleAgentChange"
						/>
					</template>
				</div>
			</div>

			<!-- Right side content -->
			<div class="navbar__content__right">
				<!-- Auth button -->
				<div v-if="!signedIn" class="navbar__content__right__item">
					<Button class="button--auth" icon="pi pi-sign-in" label="Sign In" @click="signIn()"></Button>
				</div>
				<!-- Logged in user name -->
				<div v-else class="navbar__content__right__item">
					<span>Welcome, {{ accountName }}</span>
					<Button class="button--auth" icon="pi pi-sign-out" label="Sign Out" @click="signOut()"></Button>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import { mapStores } from 'pinia';
import type { Session } from '@/js/types';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAppStore } from '@/stores/appStore';
import { getMsalInstance, getLoginRequest } from '@/js/auth';

export default {
	name: 'NavBar',

	emits: ['close-sidebar'],

	data() {
		return {
			logoText: '',
			logoURL: '',
			isSidebarClosed: true,
			signedIn: false,
			accountName: '',
			userName: '',
			allowAgentHint: false,
			agentSelection: null,
			agents: [],
		};
	},

	computed: {
		...mapStores(useAppConfigStore),
		...mapStores(useAppStore),

		currentSession() {
			return this.appStore.currentSession;
		},
	},

	watch: {
		currentSession(newSession: Session, oldSession: Session) {
			if (newSession.id === oldSession?.id) return;
			this.agentSelection = this.agents.find(agent => agent.value === this.appConfigStore.selectedAgents.get(newSession.id)) || null;
		},
	},

	async created() {
		this.allowAgentHint = this.appConfigStore.allowAgentHint.enabled;
		this.logoText = this.appConfigStore.logoText;
		this.logoURL = this.appConfigStore.logoUrl;
		this.closeSidebar(this.appConfigStore.isKioskMode);

		this.agents.push({ label: '--select--', value: null});
		for (const agent of this.appConfigStore.agents) {
			this.agents.push({ label: agent, value: agent });
		}

		if (process.client) {
			const msalInstance = await getMsalInstance();
			const accounts = await msalInstance.getAllAccounts();
			if (accounts.length > 0) {
				this.signedIn = true;
				this.accountName = accounts[0].name;
				this.userName = accounts[0].username;
			}
		}
	},

	methods: {
		closeSidebar(closed: boolean) {
			this.isSidebarClosed = closed;
			this.$emit('close-sidebar', closed);
		},

		handleCopySession() {
			const chatLink = `${window.location.origin}?chat=${this.currentSession!.id}`;
			navigator.clipboard.writeText(chatLink);

			this.$toast.add({
				severity: 'success',
				detail: 'Chat link copied!',
				life: 2000,
			});
		},

		handleAgentChange() {
			this.appConfigStore.selectedAgents.set(this.currentSession.id, this.agentSelection.value);
			const message = this.agentSelection.value ? `Agent changed to ${this.agentSelection.label}` : `Cleared agent hint selection`;
			this.$toast.add({
				severity: 'success',
				detail: message,
				life: 2000,
			});
		},

		async signIn() {
			const loginRequest = await getLoginRequest();
			const msalInstance = await getMsalInstance();
			const response = await msalInstance.loginPopup(loginRequest);
			if (response.account) {
				this.signedIn = true;
				this.accountName = response.account.name;
				this.userName = response.account.username;
			}
		},

		async signOut() {
			const msalInstance = await getMsalInstance();
			const accountFilter = {
				username: this.userName,
			};
			const logoutRequest = {
				account: msalInstance.getAccount(accountFilter),
			};

			await msalInstance.logoutRedirect(logoutRequest);
			this.signedIn = false;
			this.accountName = '';
			this.userName = '';
			this.$router.push({ path: '/login' });
			// await msalInstance.logout();
		},
	},
};
</script>

<style lang="scss" scoped>
.navbar {
	height: 70px;
	width: 100%;
	display: flex;
	flex-direction: row;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
}

.navbar--collapsed {
	.navbar__content {
		background-color: var(--primary-color);
		justify-content: flex-end;
		border-bottom: none;
	}
}

.navbar__header {
	width: 300px;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	padding-bottom: 12px;
	display: flex;
	align-items: center;
	justify-content: space-between;
	color: var(--primary-text);
	background-color: var(--primary-color);

	img {
		max-height: 100%;
		width: auto;
		max-width: 148px;
		margin-right: 12px;
	}
}

.navbar__content {
	flex: 1;
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 24px;
	border-bottom: 1px solid #EAEAEA;
	background-color: var(--accent-color);
}

.navbar__content__left {
	display: flex;
	align-items: center;
}

.navbar__content__left__item {
	display: flex;
	align-items: center;
}

.navbar__content__right__item {
	display: flex;
	align-items: center;
}

.button--share {
	margin-left: 8px;
}

.button--auth {
	margin-left: 24px;
}
</style>
