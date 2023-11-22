<template>
	<div class="chat-app">
		<NavBar @close-sidebar="closeSidebar" />
		<div class="chat-content">
			<ChatSidebar v-show="!isSidebarClosed" />
			<ChatThread :sidebar-closed="isSidebarClosed" />
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'Index',

	data() {
		return {
			isSidebarClosed: false,
		};
	},

	methods: {
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
