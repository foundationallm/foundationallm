<template>
	<div class="chat-app">
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
		<div v-else class="normal-app-content h-full">
			<Button
				class="sr-only skip-to-input-button"
				role="link"
				label="Skip to input"
				aria-label="Skip to input"
				@click="focusInput"
			/>
			<header role="banner">
				<NavBar />
			</header>
			<div class="chat-content">
				<aside
					v-show="!$appStore.isSidebarClosed"
					ref="sidebar"
					class="chat-sidebar-wrapper"
					role="navigation"
				>
					<ChatSidebar
						class="chat-sidebar"
						:style="{ width: sidebarWidth + 'px' }"
						style="padding-right: 5px"
					/>
					<VTooltip
						:auto-hide="isMobile"
						:popper-triggers="isMobile ? [] : ['hover']"
						:skidding="skidding"
					>
						<div
							class="resize-handle"
							tabindex="0"
							aria-label="Resize sidebar"
							@mousedown="startResizing"
							@keydown.left.prevent="resizeSidebarWithKeyboard(-10)"
							@keydown.right.prevent="resizeSidebarWithKeyboard(10)"
						/>
						<template #popper>
							<div role="tooltip">Resize sidebar (Use left and right arrow keys)</div>
						</template>
					</VTooltip>
				</aside>
				<div
					v-show="!$appStore.isSidebarClosed"
					class="sidebar-blur"
					@click="$appStore.toggleSidebar"
				/>
				<main role="main" class="chat-main">
					<ChatThread ref="thread" :is-dragging="isDragging" />
				</main>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import '@/styles/access-denied.scss';

export default {
	name: 'Index',

	data() {
		return {
			sidebarWidth: 305,
			isDragging: false,
			isMobile: window.screen.width < 950,
			skidding: 0,
		};
	},

	mounted() {
		if (window.innerWidth < 950) {
			this.$appStore.toggleSidebar();
		}

		window.addEventListener('dragenter', this.showDropZone);
		window.addEventListener('dragleave', this.hideDropZone);
		window.addEventListener('dragover', this.handleDragOver);
		window.addEventListener('drop', this.handleDrop);
	},

	beforeUnmount() {
		window.removeEventListener('dragenter', this.showDropZone);
		window.removeEventListener('dragleave', this.hideDropZone);
		window.removeEventListener('dragover', this.handleDragOver);
		window.removeEventListener('drop', this.handleDrop);
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

		resizeSidebarWithKeyboard(offset: number) {
			const newWidth = this.sidebarWidth + offset;

			if (newWidth < 305) {
				this.sidebarWidth = 305;
			} else if (newWidth > 600) {
				this.sidebarWidth = 600;
			} else {
				this.sidebarWidth = newWidth;
			}

			this.$refs.sidebar.style.width = `${this.sidebarWidth}px`;
			// resets tooltip location
			this.skidding = this.sidebarWidth * 0.001;
		},

		showDropZone(event) {
			if (event.dataTransfer && event.dataTransfer.types.includes('Files')) {
				this.isDragging = true;
			}
		},

		hideDropZone(event) {
			if (!event.relatedTarget) {
				this.isDragging = false;
			}
		},

		handleDragOver(event) {
			event.preventDefault();
		},

		handleDrop(event) {
			if (event.dataTransfer && event.dataTransfer.types.includes('Files')) {
				event.preventDefault();

				this.isDragging = false;

				const dropZone = this.$refs.thread.$refs.dropZone;

				const dropZoneRect = dropZone.getBoundingClientRect();

				const isInDropZone =
					event.clientX >= dropZoneRect.left &&
					event.clientX <= dropZoneRect.right &&
					event.clientY >= dropZoneRect.top &&
					event.clientY <= dropZoneRect.bottom;

				if (isInDropZone) {
					this.$refs.thread.handleParentDrop(event);
				}
			}
		},

		stopResizing() {
			document.removeEventListener('mousemove', this.resizeSidebar);
			document.removeEventListener('mouseup', this.stopResizing);
		},

		focusInput() {
			this.$refs.thread.$refs.chatInput.$refs.inputRef.focus();
		},
	},
};
</script>

<style lang="scss" scoped>
.sr-only {
	position: absolute;
	width: 1px;
	height: 1px;
	padding: 0;
	margin: -1px;
	overflow: hidden;
	clip: rect(0, 0, 0, 0);
	border: 0;
}

.skip-to-input-button:focus {
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
	overflow: hidden;
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
