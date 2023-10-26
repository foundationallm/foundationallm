<template>
	<div class="chat-app">
		<NavBar :currentSession="currentSession" @collapse-sidebar="collapseSidebar" />
		<div class="chat-content">
			<ChatSidebar v-show="!closeSidebar" ref="sidebar" :currentSession="currentSession" @change-session="handleChangeSession" @session-updated="handleSessionUpdated" />
			<ChatThread :session="currentSession" :sidebar-closed="closeSidebar" @session-updated="handleThreadSessionUpdated" />
		</div>
	</div>
</template>

<script lang="ts">
import { Session } from '@/js/types';

export default {
	name: 'Index',

	data() {
		return {
			currentSession: {} as Session,
			closeSidebar: false,
		};
	},

	methods: {
		handleSessionUpdated(session: Session) {
			this.currentSession = session;
		},

		handleChangeSession(session: Session) {
			const query = { chat: session.id };
			this.$router.push({ query });
			this.currentSession = session;
		},

		handleThreadSessionUpdated(session: Session) {
			this.currentSession = session;
			this.$refs.sidebar.getSessions();
		},

		collapseSidebar(collapsed: boolean) {
			this.closeSidebar = collapsed;
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
