<template>
	<div class="chat-sidebar">
		<!-- Sidebar section header -->
		<div class="chat-sidebar__section-header--mobile">
			<img
				v-if="$appConfigStore.logoUrl !== ''"
				:src="$filters.enforceLeadingSlash($appConfigStore.logoUrl)"
				:alt="$appConfigStore.logoText"
			/>
			<span v-else>{{ $appConfigStore.logoText }}</span>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					class="chat-sidebar__button"
					:icon="$appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
					size="small"
					severity="secondary"
					aria-label="Toggle sidebar"
					:aria-expanded="!$appStore.isSidebarClosed"
					@click="$appStore.toggleSidebar"
					@keydown.esc="hideAllPoppers"
				/>
				<template #popper><div role="tooltip">Toggle sidebar</div></template>
			</VTooltip>
		</div>
		<div class="chat-sidebar__section-header">
			<h2 class="chat-sidebar__section-header__text">Chats</h2>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					icon="pi pi-plus"
					text
					severity="secondary"
					class="chat-sidebar__button"
					style="color: var(--primary-text) !important"
					aria-label="Add new chat"
					:disabled="createProcessing"
					@click="handleAddSession"
					@keydown.esc="hideAllPoppers"
				/>
				<template #popper><div role="tooltip">Add new chat</div></template>
			</VTooltip>
		</div>

		<!-- Chats -->
		<div class="chat-sidebar__chats">
			<nav>
				<ul role="list" class="chat-list">
					<li v-if="!sessions" role="listitem" class="chat-list-item">No sessions</li>
					<li
						v-for="session in sessions"
						:key="session.sessionId"
						class="chat-sidebar__chat chat-list-item"
						role="listitem"
						@click="handleSessionSelected(session)"
						@keydown.enter="handleSessionSelected(session)"
					>
						<div
							class="chat"
							:class="{
								'chat--selected': currentSession?.sessionId === session.sessionId,
								'chat--editing': session?.sessionId === conversationToUpdate?.sessionId,
								'chat--deleting': session?.sessionId === conversationToDelete?.sessionId,
							}"
						>
							<!-- Chat name -->
							<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
								<span class="chat__name" tabindex="0" @keydown.esc="hideAllPoppers">{{
									session.display_name
								}}</span>
								<template #popper>
									<div role="tooltip">
										{{ session.display_name }}
									</div>
								</template>
							</VTooltip>

							<!-- Chat icons -->
							<span class="chat__icons">
								<!-- Rename session -->
								<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
									<Button
										icon="pi pi-pencil"
										size="small"
										severity="secondary"
										text
										class="chat-sidebar__button"
										style="color: var(--primary-text) !important"
										aria-label="Rename chat session"
										@click.stop="openRenameModal(session)"
										@keydown.esc="hideAllPoppers"
									/>
									<template #popper><div role="tooltip">Rename chat session</div></template>
								</VTooltip>

								<!-- Delete session -->
								<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
									<Button
										icon="pi pi-trash"
										size="small"
										severity="danger"
										text
										class="chat-sidebar__button"
										style="color: var(--primary-text) !important"
										aria-label="Delete chat session"
										@click.stop="conversationToDelete = session"
										@keydown.esc="hideAllPoppers"
									/>
									<template #popper><div role="tooltip">Delete chat session</div></template>
								</VTooltip>
							</span>
						</div>
					</li>
				</ul>
			</nav>
		</div>

		<!-- Logged in user -->
		<div v-if="$authStore.currentAccount?.name" class="chat-sidebar__account">
			<UserAvatar size="large" class="chat-sidebar__avatar" />

			<div>
				<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
					<span
						class="chat-sidebar__username"
						aria-label="Logged in as {{ $authStore.currentAccount?.username }}"
					>
						{{ $authStore.currentAccount?.name }}
					</span>
					<template #popper>
						<div role="tooltip">Logged in as {{ $authStore.currentAccount?.username }}</div>
					</template>
				</VTooltip>
				<div class="chat-sidebar__options">
					<Button
						class="chat-sidebar__sign-out chat-sidebar__button"
						icon="pi pi-sign-out"
						label="Sign Out"
						severity="secondary"
						size="small"
						@click="$authStore.logout()"
					/>
					<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
						<Button
							class="chat-sidebar__settings chat-sidebar__button"
							icon="pi pi-cog"
							severity="secondary"
							size="small"
							aria-label="Settings"
							aria-controls="settings-modal"
							:aria-expanded="settingsModalVisible"
							@click="settingsModalVisible = true"
						/>
						<template #popper><div role="tooltip">Settings</div></template>
					</VTooltip>
				</div>
			</div>
		</div>

		<!-- Rename session dialog -->
		<Dialog
			v-if="conversationToUpdate !== null"
			v-focustrap
			:visible="conversationToUpdate !== null"
			:header="`Update conversation`"
			:closable="false"
			class="sidebar-dialog"
			modal
		>
			<InputText
				v-model="newSessionName"
				:style="{ width: '100%' }"
				type="text"
				placeholder="New chat name"
				aria-label="New chat name"
				autofocus
				@keydown="renameSessionInputKeydown"
			></InputText>
			<template #footer>
				<Button class="sidebar-dialog__button" label="Cancel" text @click="closeRenameModal" />
				<Button class="sidebar-dialog__button" label="Rename" @click="handleUpdateConversation" />
			</template>
		</Dialog>

		<!-- Delete session dialog -->
		<Dialog
			v-if="conversationToDelete !== null"
			v-focustrap
			:visible="conversationToDelete !== null"
			:closable="false"
			class="sidebar-dialog"
			modal
			header="Delete a Chat"
			@keydown="deleteSessionKeydown"
		>
			<div v-if="deleteProcessing" class="delete-dialog-content">
				<div role="status">
					<i
						class="pi pi-spin pi-spinner"
						style="font-size: 2rem"
						role="img"
						aria-label="Loading"
					></i>
				</div>
			</div>
			<div v-else>
				<p>Do you want to delete the chat "{{ conversationToDelete.display_name }}" ?</p>
			</div>
			<template #footer>
				<Button
					class="sidebar-dialog__button"
					label="Cancel"
					text
					:disabled="deleteProcessing"
					@click="conversationToDelete = null"
				/>
				<Button
					class="sidebar-dialog__button"
					label="Delete"
					severity="danger"
					autofocus
					:disabled="deleteProcessing"
					@click="handleDeleteSession"
				/>
			</template>
		</Dialog>

		<Dialog
			id="settings-modal"
			v-model:visible="settingsModalVisible"
			v-focustrap
			modal
			class="sidebar-dialog"
			header="Settings"
			@keydown.esc="settingsModalVisible = false"
		>
			<TabView>
				<TabPanel header="Accessibility">
					<div class="setting-option">
						<h4 id="auto-hide-toasts" class="setting-option-label">
							Auto hide popup notifications
						</h4>
						<InputSwitch v-model="$appStore.autoHideToasts" aria-labelledby="auto-hide-toasts" />
					</div>
					<div class="setting-option">
						<h4 id="text-size" class="setting-option-label">Text size</h4>
						<div class="text-size-slider-container">
							<Slider
								v-model="$appStore.textSize"
								:style="{ width: '100%', marginRight: '1rem' }"
								:min="0.8"
								:max="1.5"
								:step="0.1"
								aria-labelledby="text-size"
								aria-valuemin="80%"
								aria-valuemax="150%"
								:aria-valuenow="Math.round(($appStore.textSize / 1) * 100) + '%'"
							/>
							<p>{{ Math.round(($appStore.textSize / 1) * 100) }}%</p>
						</div>
					</div>
					<!-- <div class="setting-option">
						<h4 id="contrast" class="setting-option-label">High contrast mode</h4>
						<InputSwitch v-model="$appStore.highContrastMode" aria-labelledby="contrast" />
					</div> -->
				</TabPanel>
			</TabView>
			<template #footer>
				<Button
					class="sidebar-dialog__button"
					label="Close"
					text
					@click="settingsModalVisible = false"
				/>
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { hideAllPoppers } from 'floating-vue';
import eventBus from '@/js/eventBus';
import type { Session } from '@/js/types';
declare const process: any;

export default {
	name: 'ChatSidebar',

	data() {
		return {
			conversationToUpdate: null as Session | null,
			newSessionName: '' as string,
			conversationToDelete: null as Session | null,
			deleteProcessing: false,
			isMobile: window.screen.width < 950,
			createProcessing: false,
			debounceTimeout: null as NodeJS.Timeout | null,
			settingsModalVisible: false,
		};
	},

	computed: {
		sessions() {
			return this.$appStore.sessions.filter((session) => !session.is_temp);
		},

		currentSession() {
			return this.$appStore.currentSession;
		},
	},

	async created() {
		if (window.screen.width < 950) {
			this.$appStore.isSidebarClosed = true;
		}

		if (process.client) {
			await this.$appStore.init(this.$nuxt._route.query.chat);
		}

		// Listen for the agent change event.
		eventBus.on('agentChanged', this.handleAddSession);
	},

	methods: {
		openRenameModal(session: Session) {
			this.conversationToUpdate = session;
			this.newSessionName = session.display_name;
		},

		closeRenameModal() {
			this.conversationToUpdate = null;
			this.newSessionName = '';
		},

		handleSessionSelected(session: Session) {
			this.$appStore.changeSession(session);
		},

		async handleAddSession() {
			if (this.createProcessing) return;

			if (this.debounceTimeout) {
				this.$appStore.addToast({
					severity: 'warn',
					summary: 'Warning',
					detail: 'Please wait before creating another session.',
					life: 3000,
				});
				return;
			}

			this.createProcessing = true;

			try {
				const newSession = await this.$appStore.addSession();
				this.handleSessionSelected(newSession);

				this.debounceTimeout = setTimeout(() => {
					this.debounceTimeout = null;
				}, 2000);
			} catch (error) {
				this.$appStore.addToast({
					severity: 'error',
					summary: 'Error',
					detail: 'Could not create a new session. Please try again.',
				});
			} finally {
				this.createProcessing = false; // Re-enable the button
			}
		},

		handleUpdateConversation() {
			this.$appStore.updateConversation(this.conversationToUpdate!, this.newSessionName);
			this.conversationToUpdate = null;
		},

		async handleDeleteSession() {
			this.deleteProcessing = true;
			try {
				await this.$appStore.deleteSession(this.conversationToDelete!);
				this.conversationToDelete = null;
			} catch (error) {
				this.$appStore.addToast({
					severity: 'error',
					summary: 'Error',
					detail: 'Could not delete the session. Please try again.',
				});
			} finally {
				this.deleteProcessing = false;
			}
		},

		renameSessionInputKeydown(event: KeyboardEvent) {
			if (event.key === 'Enter') {
				this.handleUpdateConversation();
			}
			if (event.key === 'Escape') {
				this.closeRenameModal();
			}
		},

		deleteSessionKeydown(event: KeyboardEvent) {
			if (event.key === 'Escape') {
				this.conversationToDelete = null;
			}
		},

		hideAllPoppers() {
			hideAllPoppers();
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-sidebar {
	width: 300px;
	max-width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	flex: 1;
	background-color: var(--primary-color);
	z-index: 3;
	scrollbar-color: var(--sidebar-scrollbar-default) transparent;

	&:hover {
		scrollbar-color: var(--sidebar-scrollbar-focused) transparent;
	}
}

.chat-sidebar__header {
	height: 70px;
	width: 100%;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	display: flex;
	align-items: center;
	color: var(--primary-text);

	img {
		max-height: 100%;
		width: auto;
		max-width: 148px;
		margin-right: 12px;
	}
}

.chat-sidebar__section-header {
	height: 64px;
	padding: 24px;
	padding-bottom: 12px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--primary-text);
	text-transform: uppercase;
	// font-size: 14px;
	font-size: 0.875rem;
	font-weight: 600;
}

.chat-sidebar__section-header__text {
	font-size: 0.875rem;
	font-weight: 600;
}

.chat-sidebar__section-header--mobile {
	display: none;
}

.chat-sidebar__chats {
	flex: 1;
	overflow-y: auto;
}

.chat {
	padding: 24px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--primary-text);
	transition: all 0.1s ease-in-out;
	font-size: 13px;
	font-size: 0.8125rem;
	height: 72px;

	&:hover {
		.chat__icons {
			display: flex;
			opacity: 1;
		}
	}
}

.chat__name {
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
	font-size: 0.8125rem;
	font-weight: 400;
}

.chat__icons {
	display: none;
	justify-content: space-between;
	opacity: 0;
	flex-shrink: 0;
	margin-left: 12px;
	transition: all 0.1s ease-in-out;
}

.chat:hover {
	background-color: rgba(217, 217, 217, 0.05);
}

.chat--selected {
	color: var(--secondary-text);
	background-color: var(--secondary-color);
	border-left: 4px solid rgba(217, 217, 217, 0.5);

	.chat__icons {
		display: flex;
		opacity: 1;
	}
}

.chat--selected .option {
	background-color: rgba(245, 245, 245, 1);
}

@mixin blink-background($startColor, $endColor, $duration: 2s, $iterations: infinite) {
	$animation-name: blink-background-#{random(1000)};

	@keyframes #{$animation-name} {
		0% {
			background-color: $startColor;
		}
		40% {
			background-color: $endColor;
		}
		100% {
			background-color: $startColor;
		}
	}

	animation: $animation-name ease-out $duration $iterations;
}

.chat--editing {
	@include blink-background(transparent, var(--secondary-color));
}

.chat--deleting {
	@include blink-background(transparent, var(--red-600));
}

.option {
	background-color: rgba(220, 220, 220, 1);
	padding: 4px;
	border-radius: 3px;
}

.option:hover {
	background-color: rgba(200, 200, 200, 1);
	cursor: pointer;
}

.delete {
	margin-left: 8px;
}

.chat__name {
	cursor: pointer;
}

.chat-sidebar__account {
	display: grid;
	grid-template-columns: min-content auto;
	// added extra padding to the right to account for resize handle width
	padding: 12px 29px 12px 24px;
	text-transform: inherit;
	align-items: center;
}

.chat-sidebar__avatar {
	margin-right: 12px;
	height: 61px;
	width: 61px;
	border-radius: 50%; /* circular crop */
	background: transparent;
	overflow: hidden;
	display: flex;
	align-items: center;
	justify-content: center;
}

.chat-sidebar__avatar img,
.chat-sidebar__avatar picture,
.chat-sidebar__avatar svg {
	width: 100%;
	height: 100%;
	object-fit: cover;
	border-radius: 50%;
	display: block;
}

.chat-sidebar__sign-out {
	width: 100%;
}

.p-button-text.sidebar-dialog__button:focus {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.sidebar-dialog__button:focus {
	box-shadow: 0 0 0 0.1rem #000;
}

.chat-sidebar__button:focus {
	box-shadow: 0 0 0 0.1rem #fff;
}

.chat-sidebar__username {
	color: var(--primary-text);
	font-weight: 600;
	font-size: 0.875rem;
	text-transform: capitalize;
	line-height: 0;
	vertical-align: super;
}

.chat-sidebar__options {
	display: flex;
	align-items: stretch;
}

.chat-sidebar__settings {
	margin-left: 4px;
	height: 100%;
}

.p-overlaypanel-content {
	background-color: var(--primary-color);
}

.overlay-panel__option {
	display: flex;
	align-items: center;
	cursor: pointer;
}

.overlay-panel__option:hover {
	color: var(--primary-color);
}

.delete-dialog-content {
	display: flex;
	justify-content: center;
	padding: 20px 150px;
}

ul.chat-list {
	list-style-type: none;
	padding-left: 0;
	margin: 0;
}

li.chat-list-item {
	padding: 0;
	margin: 0;
}

.setting-option {
	display: flex;
	flex-direction: row;
	align-items: center;
	justify-content: space-between;
	gap: 1rem;
}

.text-size-slider-container {
	display: flex;
	align-items: center;
	width: 100%;
	max-width: 300px;
}

#text-size {
	text-wrap: nowrap;
}

@media only screen and (max-width: 950px) {
	.chat-sidebar__section-header--mobile {
		height: 70px;
		padding: 12px 24px;
		display: flex;
		justify-content: space-between;
		align-items: center;
		img {
			max-height: 100%;
			width: auto;
			max-width: 148px;
			margin-right: 12px;
		}
	}
}
</style>

<style lang="scss">
.p-inputswitch:not(.p-disabled):has(.p-inputswitch-input:focus-visible) .p-inputswitch-slider {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.p-inputswitch.p-highlight:not(.p-disabled):has(.p-inputswitch-input:focus-visible)
	.p-inputswitch-slider {
	box-shadow: 0 0 0 0.1rem #000; /* Black box-shadow when p-highlight is also present */
}

.p-inputswitch:not(.p-disabled):has(.p-inputswitch-input:focus-visible) .p-inputswitch-slider {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.p-slider .p-slider-handle:focus-visible {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.p-tabview .p-tabview-nav li .p-tabview-nav-link:not(.p-disabled):focus-visible {
	box-shadow: inset 0 0 0 0.1rem var(--primary-button-bg);
}

.p-dialog .p-dialog-header .p-dialog-header-icon:focus-visible {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.p-inputtext:focus:not(.p-dropdown-label) {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.p-dropdown:not(.p-disabled).p-focus {
	border-color: var(--primary-button-bg);
}

.sidebar-dialog {
	max-width: 90vw;
}

@media only screen and (max-width: 950px) {
	.sidebar-dialog {
		width: 95vw;
	}
}
</style>
