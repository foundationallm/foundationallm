<template>
	<div class="chat-app">
		<header role="banner">
			<NavBar />
		</header>
		<div class="chat-content">
			<aside v-show="!$appStore.isSidebarClosed" ref="sidebar" class="chat-sidebar-wrapper" role="navigation">
				<ChatSidebar class="chat-sidebar" :style="{ width: sidebarWidth + 'px' }" />
				<div class="resize-handle" @mousedown="startResizing"></div>
			</aside>
			<div
				v-show="!$appStore.isSidebarClosed"
				class="sidebar-blur"
				@click="$appStore.toggleSidebar"
			/>
			<main role="main" class="chat-main">
				<ChatThread />
			</main>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'Index',

	data() {
		return {
			sidebarWidth: 305,
		};
	},

	mounted() {
		if (window.innerWidth < 950) {
			this.$appStore.toggleSidebar();
		}
	},

	methods: {
		startResizing(event: Event) {
			// Prevent default action and bubbling
			event.preventDefault();
			document.addEventListener('mousemove', this.resizeSidebar);
			document.addEventListener('mouseup', this.stopResizing);
		},

		resizeSidebar(event: MouseEvent) {
			const sidebarRect = this.$refs.sidebar.getBoundingClientRect();
			const minWidth = 305; // Minimum sidebar width
			const maxWidth = 600; // Maximum sidebar width, adjust as needed

			// Calculate new width based on mouse movement, ensuring it's within the min and max bounds
			let newWidth = event.clientX - sidebarRect.left;

			// Apply minimum and maximum constraints
			if (newWidth < minWidth) {
				newWidth = minWidth;
			} else if (newWidth > maxWidth) {
				newWidth = maxWidth;
			}

			// Update the sidebar width
			this.sidebarWidth = newWidth;
			this.$refs.sidebar.style.width = `${this.sidebarWidth}px`;
		},

		stopResizing() {
			document.removeEventListener('mousemove', this.resizeSidebar);
			document.removeEventListener('mouseup', this.stopResizing);
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

.chat-sidebar-wrapper {
	display: flex;
	align-items: stretch;
	flex-direction: row;
	position: relative;
	height: 100%;
}

.resize-handle {
	z-index: 10;
	position: absolute;
	right: 0;
	top: 0;
	cursor: col-resize;
	width: 5px;
	background-color: #ccc;
	height: 100%;
}

.chat-main {
	width: 100%;
	height: 100%;
}

@media only screen and (max-width: 620px) {
	.sidebar-blur {
		position: absolute;
		width: 100%;
		height: 100%;
		z-index: 2;
		top: 0px;
		left: 0px;
		backdrop-filter: blur(3px);
	}
}

@media only screen and (max-width: 950px) {
	.chat-sidebar {
		position: relative;
		top: 0px;
		box-shadow: 5px 0px 10px rgba(0, 0, 0, 0.4);
	}

	.chat-sidebar-wrapper {
		position: absolute;
		top: 0px;
	}

	.resize-handle {
		display: none;
	}
}
</style>
