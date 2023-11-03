<template>
	<div class="chat-app">
		<NavBar :currentSession="currentSession" @close-sidebar="closeSidebar" />
		<div class="chat-content">
			<ChatSidebar v-show="!isSidebarClosed" ref="sidebar" :currentSession="currentSession" @change-session="handleChangeSession" @session-updated="handleSessionUpdated" />
			<ChatThread :session="currentSession" :sidebar-closed="isSidebarClosed" @session-updated="handleThreadSessionUpdated" />
		</div>
	</div>
</template>

<script lang="ts">
import { mapStores } from 'pinia';
import { appConfig } from '@/stores/appConfig';
import type { Session } from '@/js/types';

export default {
	name: 'Index',

	data() {
		return {
			currentSession: {} as Session,
			isSidebarClosed: true,
		};
	},

	computed: {
		...mapStores(appConfig),
	},

	methods: {
		handleSessionUpdated(session: Session) {
			this.currentSession = session;
		},

		handleChangeSession(session: Session) {
			if (this.appConfigStore.isKioskMode) {
				this.$router.push({ query: {} });
			} else {
				const query = { chat: session.id };
				this.$router.push({ query });
			}
			this.currentSession = session;
		},

		handleThreadSessionUpdated(session: Session) {
			this.currentSession = session;
			this.$refs.sidebar.getSessions();
		},

		closeSidebar(closed: boolean) {
			this.isSidebarClosed = closed;
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-app {
	display: flex;
	flex-direction: column;
	height: 100vh;
	background-color: var(--primary-bg);
}
.chat-content {
	display: flex;
	flex-direction: row;
	height: calc(100% - 70px);
	background-color: var(--primary-bg);
}
</style>
