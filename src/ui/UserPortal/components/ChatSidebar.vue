<template>
	<div class="chat-sidebar">
		<!-- Sidebar section header -->
			<div class="chat-sidebar__section-header--mobile" :key="`header-${configLoadedTrigger}`">
				<img
					v-if="appConfigStore.logoUrl && appConfigStore.logoUrl !== ''"
					:src="$filters.enforceLeadingSlash(appConfigStore.logoUrl)"
					:alt="appConfigStore.logoText || 'FoundationaLLM'"
				/>
				<span v-else-if="appConfigStore.logoText">{{ appConfigStore.logoText }}</span>
				<span v-else>FoundationaLLM</span>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					class="chat-sidebar__button"
					:icon="appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
					size="small"
					severity="secondary"
					aria-label="Toggle sidebar"
					:aria-expanded="!appStore.isSidebarClosed"
					@click="appStore.toggleSidebar"
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
							tabindex="0"
							:class="{
								'chat--selected': currentSession?.sessionId === session.sessionId,
								'chat--editing': session?.sessionId === conversationToUpdate?.sessionId,
								'chat--deleting': session?.sessionId === conversationToDelete?.sessionId,
							}"
							@keydown="handleChatKeydown($event, session)"
						>
							<!-- Chat name -->
							<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
								<span class="chat__name" tabindex="-1" @keydown.esc="hideAllPoppers">{{
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
										icon="pi pi-cog"
										size="small"
										severity="secondary"
										text
										class="chat-sidebar__button"
										style="color: var(--primary-text) !important"
										aria-label="Update conversation"
										@click.stop="openUpdateModal(session)"
										@keydown.esc="hideAllPoppers"
									/>
									<template #popper><div role="tooltip">Update conversation</div></template>
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
										aria-label="Delete conversation"
										@click.stop="confirmDeleteSession(session)"
										@keydown.esc="hideAllPoppers"
									/>
									<template #popper><div role="tooltip">Delete conversation</div></template>
								</VTooltip>
							</span>
						</div>
					</li>
				</ul>
			</nav>
		</div>

		<!-- Logged in user -->
		<div v-if="authStore.currentAccount?.name" class="chat-sidebar__account" :key="`account-${authStateTrigger}`">
			<UserAvatar size="large" class="chat-sidebar__avatar" />

			<div>
				<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
					<span
						class="chat-sidebar__username"
						aria-label="Logged in as {{ authStore.currentAccount?.username }}"
					>
						{{ authStore.currentAccount?.name }}
					</span>
					<template #popper>
						<div role="tooltip">Logged in as {{ authStore.currentAccount?.username }}</div>
					</template>
				</VTooltip>
				<div class="chat-sidebar__options">
					<Button
						class="chat-sidebar__sign-out chat-sidebar__button"
						icon="pi pi-sign-out"
						label="Sign Out"
						severity="secondary"
						size="small"
						@click="authStore.logout()"
					/>
					<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
						<Button
							class="chat-sidebar__settings chat-sidebar__button"
							icon="pi pi-cog"
							severity="secondary"
							size="small"
							aria-label="Settings"
							aria-controls="settings-modal"
							:aria-expanded="appStore.settingsModalVisible"
							@click="appStore.settingsModalVisible = true"
						/>
						<template #popper><div role="tooltip">Settings</div></template>
					</VTooltip>
				</div>
			</div>
		</div>

		<!-- Update conversation dialog -->
		<Dialog
			v-if="conversationToUpdate !== null"
			:visible="conversationToUpdate !== null"
			:header="`Conversation properties`"
			:closable="false"
			class="sidebar-dialog"
			modal
		>
			<label for="update-conversation-name" style="margin-bottom: 0.5rem; font-weight: 500; display: block;">Name:</label>
			<InputText
				id="update-conversation-name"
				v-model="newConversationName"
				:style="{ width: '100%', minWidth: '400px' }"
				type="text"
				placeholder="New chat name"
				aria-label="New chat name"
				autofocus
				@keydown="updateConversationInputKeydown"
			></InputText>
			<label for="update-conversation-description" style="margin-top: 1rem; margin-bottom: 0.5rem; font-weight: 500; display: block;">Metadata:</label>
			<Textarea
				id="update-conversation-description"
				v-model="newConversationMetadata"
				:rows="4"
				:style="{ width: '100%', minWidth: '400px' }"
				placeholder="Add metadata for the conversation (must be valid JSON)"
				aria-label="Conversation description"
			></Textarea>
			<template #footer>
				<Button class="sidebar-dialog__button" label="Cancel" text autofocus @click="closeUpdateModal" />
				<Button class="sidebar-dialog__button" label="Update" @click="handleUpdateConversation" />
			</template>
		</Dialog>

		<Dialog
			id="settings-modal"
			v-model:visible="appStore.settingsModalVisible"
			modal
			class="sidebar-dialog csm-profile-setting-modal-1 w-full"
			header="Settings"
			@keydown.esc="appStore.settingsModalVisible = false"
		>
			<TabView :activeIndex="activeTabIndex" @tab-change="onTabChange">
				<TabPanel header="Agents">
					<div class="flex flex-wrap items-center -mx-4 mb-5">
						<div class="w-full max-w-[50%] px-4 mb-5 text-center md:text-left">
							<!-- Show enabled agents only checkbox -->
							<div class="flex items-center csm-sEnabled-checkbox-1">
								<Checkbox
									v-model="showEnabledOnly"
									inputId="show-enabled-only"
									:binary="true"
								/>
								<label for="show-enabled-only" class="ml-2 text-sm font-medium">
									Show enabled agents only
								</label>
							</div>
						</div>

						<div class="w-full max-w-[50%] px-4 mb-5 text-center md:text-left">
							<!-- Search input for filtering agents -->
							<InputText
								v-model="agentSearchTerm"
								placeholder="Search agents by name..."
								class="w-full"
								aria-label="Search agents by name"
							/>
						</div>
					</div>

					<div class="csm-table-container-1 mb-4">
						<table class="csm-table-1">
							<thead>
								<tr>
									<th>Name</th>
									<th>Enabled</th>
									<th v-if="appConfigStore.agentSelfServiceFeatureEnabled">Edit</th>
								</tr>
							</thead>
							<tbody>
								<tr v-for="getAgents in filteredAgents" :key="getAgents.object_id">
									<td>{{ getAgents.display_name || getAgents.name }}</td>
									<td>
										<div
											class="custom-checkbox"
											:class="{
												'checked': getAgents.enabled,
												'disabled': preventDisable(getAgents)
											}"
											@click="preventDisable(getAgents) ? null : toggleAgentStatus(getAgents)"
											:aria-label="`Toggle agent status - ${getAgents.enabled ? 'enabled' : 'disabled'}${getAgents.isFeatured ? ' (featured agent)' : ''}`"
											role="checkbox"
											:aria-checked="getAgents.enabled"
											:tabindex="preventDisable(getAgents) ? -1 : 0"
											@keydown.enter="preventDisable(getAgents) ? null : toggleAgentStatus(getAgents)"
											@keydown.space.prevent="preventDisable(getAgents) ? null : toggleAgentStatus(getAgents)"
										>
											<i v-if="getAgents.enabled" class="pi pi-check"></i>
										</div>
									</td>
									<td v-if="appConfigStore.agentSelfServiceFeatureEnabled">
										<Button
											link
											class="csm-table-edit-btn-1"
											:disabled="getAgents.isReadonly"
											:class="{'csm-table-edit-btn-strong': getAgents.enabled, 'csm-table-edit-btn-faded': !getAgents.enabled}"
											@click="editAgent(getAgents)"
										>
											<i class="pi pi-pencil"></i>
										</Button>
									</td>
								</tr>
							</tbody>
						</table>

						<!-- Loading state -->
						<div v-if="loadingAgents2" class="loading-container">
							<i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
							<p>Loading agents...</p>
						</div>

						<!-- Empty Message -->
						<div v-else-if="filteredAgents.length === 0 && !loadingAgents2" class="empty-state">
							<i class="pi pi-info-circle" style="font-size: 2rem; color: #6c757d;"></i>
							<p>{{ getEmptyMessage() }}</p>
						</div>

						<!-- Error Message -->
						<div v-else-if="agentError2" class="error-message">
							<i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: #e74c3c;"></i>
							<p>{{ agentError2 }}</p>
						</div>
					</div>

					<!-- Permission Request Link -->
					<div v-if="!hasAgentsContributorRole || !hasPromptsContributorRole">
						<nuxt-link
							v-if="appConfigStore.agentManagementPermissionRequestUrl && appConfigStore.agentSelfServiceFeatureEnabled"
							:to="appConfigStore.agentManagementPermissionRequestUrl"
							external
							target="_blank"
							class="p-component csm-only-text-btn-1"
						>
							Request permission to manage agents
							<i class="pi pi-external-link ml-1"></i>
						</nuxt-link>
					</div>
				</TabPanel>

				<TabPanel header="Accessibility">
					<div class="setting-option">
						<h4 id="auto-hide-toasts" class="setting-option-label">
							Auto hide popup notifications
						</h4>
						<InputSwitch v-model="appStore.autoHideToasts" aria-labelledby="auto-hide-toasts" />
					</div>
					<div class="setting-option">
						<h4 id="text-size" class="setting-option-label">Text size</h4>
						<div class="text-size-slider-container">
							<Slider
								v-model="appStore.textSize"
								:style="{ width: '100%', marginRight: '1rem' }"
								:min="0.8"
								:max="1.5"
								:step="0.1"
								aria-labelledby="text-size"
								aria-valuemin="80%"
								aria-valuemax="150%"
								:aria-valuenow="Math.round((appStore.textSize / 1) * 100) + '%'"
							/>
							<p>{{ Math.round((appStore.textSize / 1) * 100) }}%</p>
						</div>
					</div>
					<!-- <div class="setting-option">
						<h4 id="contrast" class="setting-option-label">High contrast mode</h4>
						<InputSwitch v-model="appStore.highContrastMode" aria-labelledby="contrast" />
					</div> -->
				</TabPanel>
			</TabView>

			<template #footer>
				<div class="flex w-full justify-between items-center px-2">
					<div class="w-full max-w-[50%] px-2">
						<nuxt-link
							v-if="activeTabIndex !== 1 && appConfigStore.agentSelfServiceFeatureEnabled"
							to="/manage-agents"
							class="p-component csm-only-text-btn-1">
							Manage Agents <i class="pi pi-external-link ml-1"></i>
						</nuxt-link>
					</div>

					<div class="w-full max-w-[50%] px-2 text-right">
						<Button
							class="sidebar-dialog__button"
							label="Close"
							text
							@click="appStore.settingsModalVisible = false"
						/>
					</div>
				</div>
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import eventBus from '@/js/eventBus';
import { isAgentExpired, isAgentReadonly } from '@/js/helpers';
import type { AgentOption, Session } from '@/js/types';
import '@/styles/loading.scss';
import { hideAllPoppers } from 'floating-vue';
import Checkbox from 'primevue/checkbox';
import { useAppStore } from '@/stores/appStore';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAuthStore } from '@/stores/authStore';

	declare const process: any;

	import api from '@/js/api';
	import { useConfirmationStore } from '@/stores/confirmationStore';

	export default {
		name: 'ChatSidebar',

		components: {
			Checkbox,
		},

		setup() {
			const appStore = useAppStore();
			const appConfigStore = useAppConfigStore();
			const authStore = useAuthStore();
			return { appStore, appConfigStore, authStore };
		},

		data() {
			return {
				conversationToUpdate: null as Session | null,
				conversationToDelete: null as Session | null,
				newConversationName: '' as string,
				newConversationMetadata: null as any | null,
				isMobile: window.screen.width < 950,
				createProcessing: false,
				debounceTimeout: null as ReturnType<typeof setTimeout> | null,

				agentOptions: [],
				emptyAgentsMessage: null,

				agentOptions2: [] as AgentOption[],
				loadingAgents2: false,
				agentError2: '',
				emptyAgentsMessage2: 'No agents available.',
				userProfile: null as any,
				agentSearchTerm: '',
				showEnabledOnly: false,
				activeTabIndex: 0,
				configLoadedTrigger: 0,
				authStateTrigger: 0,
				authPollingInterval: null as ReturnType<typeof setInterval> | null,
				hasAgentsContributorRole: false,
				hasPromptsContributorRole: false,
			};
		},

		computed: {
			sessions(): Session[] {
				return (this.appStore as any).sessions.filter((session: Session) => !session.is_temp);
			},

			currentSession(): Session | null {
				return (this.appStore as any).currentSession;
			},

			filteredAgents(): AgentOption[] {
				let filtered = this.agentOptions2;

				// Filter by enabled status if checkbox is checked
				if (this.showEnabledOnly) {
					filtered = filtered.filter((agent: AgentOption) => agent.enabled);
				}

				// Filter by search term if provided
				if (this.agentSearchTerm.trim()) {
					const searchTerm = this.agentSearchTerm.toLowerCase().trim();
					filtered = filtered.filter((agent: AgentOption) => {
						const name = (agent.display_name || agent.name || '').toLowerCase();
						return name.includes(searchTerm);
					});
				}

				return filtered;
			},
		},

		watch: {
			'appStore.agents': {
				handler() {
					this.setAgentOptions();
				},
				deep: true,
			},
			'appStore.lastSelectedAgent': {
				handler() {
					this.setAgentOptions();
				},
				deep: true,
			},
			'appConfigStore.isConfigurationLoaded': {
				handler(newVal) {
					if (newVal) {
						// Trigger reactivity by updating a local data property
						this.configLoadedTrigger = Date.now();
					}
				},
				immediate: true,
			},
			'appConfigStore.isFeaturedAgentNamesLoaded': {
				async handler(newVal) {
					if (newVal) {
						// Refresh agents now that config (and featuredAgentNames) is loaded
        		await this.loadAllowedAgents();
					}
				},
				immediate: true,
			},
			'authStore.isAuthenticated': {
				handler(newVal) {
					if (newVal) {
						// Trigger reactivity by updating a local data property
						this.authStateTrigger = Date.now();
					}
				},
				immediate: true,
			},
		},

		async created() {
			if (window.screen.width < 950) {
				(this.appStore as any).isSidebarClosed = true;
			}

			if (process.client) {
				await (this.appStore as any).init((this.$nuxt as any)._route.query.chat);
			}

			// Listen for the agent change event.
			eventBus.on('agentChanged', this.handleAddSession);

		},

		async mounted() {
			// Add event listeners in mounted to ensure DOM is ready
			if (process.client && window) {
				window.addEventListener('config-loaded', this.handleConfigLoaded);
				window.addEventListener('auth-updated', this.handleAuthUpdated);

				// Check if configuration is already loaded and trigger update
				if (this.appConfigStore.logoUrl) {
					this.handleConfigLoaded({ detail: {
						logoUrl: this.appConfigStore.logoUrl,
						logoText: this.appConfigStore.logoText
					}} as CustomEvent);
				}

				// Check if authentication is already available and trigger update
				if (this.authStore.currentAccount) {
					this.handleAuthUpdated({ detail: {
						isAuthenticated: this.authStore.isAuthenticated,
						currentAccount: this.authStore.currentAccount
					}} as CustomEvent);
				}

				// Start polling for authentication changes (fallback if events don't work)
				this.startAuthPolling();
			}

			await this.setAgentOptions();
			await this.loadUserProfile();
			await this.loadAllowedAgents();
			await this.checkContributorRoles();
		},

		unmounted() {
			// Remove the agent change event listener.
			eventBus.off('agentChanged', this.handleAddSession);

			// Remove config loaded event listener
			if (process.client && window) {
				window.removeEventListener('config-loaded', this.handleConfigLoaded);
				window.removeEventListener('auth-updated', this.handleAuthUpdated);
			}

			// Clear auth polling interval
			if (this.authPollingInterval) {
				clearInterval(this.authPollingInterval);
				this.authPollingInterval = null;
			}
		},

		methods: {
			handleConfigLoaded(event: CustomEvent) {
				this.configLoadedTrigger = Date.now();
			},

			handleAuthUpdated(event: CustomEvent) {
				this.authStateTrigger = Date.now();

				// If we have authentication, stop polling
				if (this.authStore.currentAccount && this.authPollingInterval) {
					clearInterval(this.authPollingInterval);
					this.authPollingInterval = null;
				}
			},

			startAuthPolling() {
				let pollCount = 0;
				const maxPolls = 30; // 15 seconds with 500ms intervals

				this.authPollingInterval = setInterval(() => {
					pollCount++;

					// Check if we have authentication now
					if (this.authStore.currentAccount) {
						this.handleAuthUpdated({ detail: {
							isAuthenticated: this.authStore.isAuthenticated,
							currentAccount: this.authStore.currentAccount
						}} as CustomEvent);
						return; // handleAuthUpdated will clear the interval
					}

					// Stop polling after max attempts
					if (pollCount >= maxPolls) {
						if (this.authPollingInterval) {
							clearInterval(this.authPollingInterval);
							this.authPollingInterval = null;
						}
					}
				}, 500);
			},
			openUpdateModal(session: Session) {
				this.conversationToUpdate = session;
				this.newConversationName = session.display_name;
				this.newConversationMetadata = session.metadata
			},

			closeUpdateModal() {
				this.conversationToUpdate = null;
				this.newConversationName = '';
				this.newConversationMetadata = null;
			},

			handleSessionSelected(session: Session) {
				(this.appStore as any).changeSession(session);
				const sessionAgent = (this.appStore as any).getSessionAgent(session);
				if (sessionAgent) {
					(this.appStore as any).setSessionAgent(session, sessionAgent, true);
				}
			},

			async handleAddSession() {
				if (this.createProcessing) return;

				if (this.debounceTimeout) {
					(this.appStore as any).addToast({
						severity: 'warn',
						summary: 'Warning',
						detail: 'Please wait before creating another session.',
						life: 3000,
					});
					return;
				}

				this.createProcessing = true;

				try {
				const currentAgent = this.currentSession ? (this.appStore as any).getSessionAgent(this.currentSession) : null;
					const mostRecentSession = this.sessions[0];
					if (mostRecentSession) {
						const isEmptySession = await (this.appStore as any).isSessionEmpty(mostRecentSession.sessionId);
						if (isEmptySession) {
							const timestamp = (this.appStore as any).getDefaultChatSessionProperties().name;
							await (this.appStore as any).updateConversation(mostRecentSession, timestamp, mostRecentSession.metadata || '');
							if (currentAgent) {
								(this.appStore as any).setSessionAgent(mostRecentSession, currentAgent, true);
							}
							this.handleSessionSelected(mostRecentSession);
							this.debounceTimeout = setTimeout(() => {
								this.debounceTimeout = null;
							}, 2000);
							return;
						}
					}
					const newSession = await (this.appStore as any).addSession();
					if (currentAgent) {
						(this.appStore as any).setSessionAgent(newSession, currentAgent, true);
					}
					this.handleSessionSelected(newSession);

					this.debounceTimeout = setTimeout(() => {
						this.debounceTimeout = null;
					}, 2000);
				} catch (error) {
					(this.appStore as any).addToast({
						severity: 'error',
						summary: 'Error',
						detail: 'Could not create a new session. Please try again.',
						life: 5000,
					});
				} finally {
					this.createProcessing = false; // Re-enable the button
				}
			},

			async confirmDeleteSession(session: Session) {
				this.conversationToDelete = session;
				const confirmationStore = useConfirmationStore();
				const confirmed = await confirmationStore.confirmAsync({
					title: 'Delete conversation',
					message: `Do you want to delete the chat "${session.display_name}" ?`,
					confirmText: 'Yes',
					cancelText: 'No',
					confirmButtonSeverity: 'danger',
					hasCancelButton: true
				});

				if (!confirmed) {
					this.conversationToDelete = null;
					return;
				}

				try {
					await (this.appStore as any).deleteSession(session);
				} catch (error) {
					(this.appStore as any).addToast({
						severity: 'error',
						summary: 'Error',
						detail: 'Could not delete the session. Please try again.',
						life: 5000,
					});
				} finally {
					this.conversationToDelete = null;
				}
			},

			handleUpdateConversation() {
				let metadataJson = this.newConversationMetadata;
				if (metadataJson !== null && typeof metadataJson === 'string' && metadataJson.trim() !== '') {
					try {
						metadataJson = JSON.parse(metadataJson);
					} catch (e) {
						(this.appStore as any).addToast({
							severity: 'error',
							summary: 'Invalid Metadata',
							detail: 'Metadata must be valid JSON.',
							life: 4000,
						});
						return;
					}
				}
				(this.appStore as any).updateConversation(this.conversationToUpdate!, this.newConversationName, this.newConversationMetadata);
				this.conversationToUpdate = null;
				this.newConversationMetadata = '';
			},

			updateConversationInputKeydown(event: KeyboardEvent) {
				if (event.key === 'Enter') {
					this.handleUpdateConversation();
				}
				if (event.key === 'Escape') {
					this.closeUpdateModal();
				}
			},

			async handleChatKeydown(event: KeyboardEvent, session: Session) {
				// MacOS'ta "Delete" tuşu aslında "Backspace" olarak algılanır.
				if (event.key === 'Delete' || event.key === 'Backspace') {
					event.preventDefault();
					await this.confirmDeleteSession(session);
				}
			},

			async setAgentOptions() {
				const isCurrentAgent = (agent: any): boolean => {
					return (
						agent.resource.name ===
						(this.appStore as any).getSessionAgent(this.currentSession)?.resource?.name
					);
				};

				// Filter out expired agents, but keep the currently selected agent even if it is expired
				const notExpiredOrCurrentAgents = (this.appStore as any).agents.filter(
					(agent: any) => !isAgentExpired(agent) || isCurrentAgent(agent),
				);

				this.agentOptions = notExpiredOrCurrentAgents.map((agent: any) => ({
					label: agent.resource.display_name ? agent.resource.display_name : agent.resource.name,
					type: agent.resource.type,
					object_id: agent.resource.object_id,
					description: agent.resource.description,
					my_agent: agent.roles.includes('Owner'),
					value: agent,
				}));
			},

			async loadUserProfile() {
				try {
					await this.appStore.getUserProfile();
        			this.userProfile = this.appStore.userProfile || null;
				} catch (error) {
					console.error('Failed to load user profile:', error);
					this.userProfile = null;
				}
			},

			async loadAllowedAgents() {

				if (!this.appConfigStore.isFeaturedAgentNamesLoaded) {
					return; // the watcher will re-invoke this when the featured agent names are available.
				}

				this.loadingAgents2 = true;
				this.agentError2 = '';


				try {
					// Ensure agents are loaded first (this will be instant if already loaded).
        			await this.appStore.getAgents();

					const agentsArray = this.appStore.agents || [];

					this.agentOptions2 = agentsArray.map((ResourceProviderGetResult: any, index: number): AgentOption => {
						const agent = ResourceProviderGetResult.resource || ResourceProviderGetResult;

						// Check if this agent is in the user's selected agents list
						const isAgentSelected = this.userProfile?.agents?.includes(agent.object_id) || false;

						// Check if this agent is a featured agent (by name, as per memory: resource names are reliable identifiers)
						const isFeaturedAgent = this.appConfigStore.featuredAgentNames?.includes(agent.name);
						const isPinnedFeaturedAgent = this.appConfigStore.pinnedFeaturedAgentNames?.includes(agent.name);

						return {
							object_id: agent.object_id,
							name: agent.name || 'Unknown Agent',
							display_name: agent.display_name,
							label: agent.display_name || agent.name || 'Unknown Agent',
							value: agent.object_id,
							type: agent.type,
							description: agent.description,
							enabled: isAgentSelected,
							isReadonly: isAgentReadonly(ResourceProviderGetResult.roles || []),
							isFeatured: isFeaturedAgent, // Add featured flag for UI logic
							isPinnedFeatured: isPinnedFeaturedAgent
						};
					});
				} catch (error) {
					console.error('Failed to load agents:', error);
					this.agentError2 = 'Failed to load agents. Please try again later.';
					this.agentOptions2 = [];
				} finally {
					this.loadingAgents2 = false;
				}
			},

			async refreshAgents() {
				await this.loadUserProfile();
				await this.loadAllowedAgents();
			},

			isCurrentAgent(agent: AgentOption): boolean {
				const currentAgent = (this.appStore as any).getSessionAgent((this.appStore as any).currentSession);
				if (!currentAgent) return false;
				return currentAgent.resource?.object_id === agent.object_id;
			},

			preventDisable(agent: AgentOption): boolean {
				// Prevent disabling if it's the current agent in use
				return agent.enabled
					&& (this.isCurrentAgent(agent) || agent.isPinnedFeatured);
			},

			selectAgent(getAgents: AgentOption) {
				this.$emit('agent-selected', getAgents);
			},

			async toggleAgentStatus(agent: AgentOption) {

				// Prevent disabling the current conversation's agent
				if (this.preventDisable(agent)) {
					(this.appStore as any).addToast({
						severity: 'warn',
						life: 5000,
						detail: 'Cannot disable the agent currently being used in this conversation.',
					});
					return;
				}

				const originalStatus = agent.enabled;

				try {
					// Toggle the enabled status optimistically
					agent.enabled = !agent.enabled;

					// Make API call to update the agent status on the server
					if (agent.enabled) {
						await api.addAgentToUserProfile(agent.object_id!);
						// Update local user profile state
						if (!this.userProfile) {
							this.userProfile = { agents: [] };
						}
						if (!this.userProfile.agents) {
							this.userProfile.agents = [];
						}
						if (!this.userProfile.agents.includes(agent.object_id!)) {
							this.userProfile.agents.push(agent.object_id!);
						}
					} else {
						await api.removeAgentFromUserProfile(agent.object_id!);
						// Update local user profile state
						if (this.userProfile?.agents) {
							const index = this.userProfile.agents.indexOf(agent.object_id!);
							if (index > -1) {
								this.userProfile.agents.splice(index, 1);
							}
						}
					}

					// Update the global app store user profile
					this.appStore.updateUserProfileAgent(agent.object_id!, agent.enabled);

					// Show success message with appropriate severity
					(this.appStore as any).addToast({
						severity: agent.enabled ? 'success' : 'warn',
						summary: 'Agent Status Updated',
						detail: `Agent "${agent.display_name || agent.name}" is now ${agent.enabled ? 'enabled' : 'disabled'}`,
						life: 2000,
					});
				} catch (error) {
					// Revert the change if the API call fails
					agent.enabled = originalStatus;

					console.error('Failed to update agent status:', error);
					(this.appStore as any).addToast({
						severity: 'error',
						summary: 'Update Failed',
						detail: 'Failed to update agent status. Please try again.',
						life: 3000,
					});
				}
			},

			editAgent(agent: AgentOption) {
				// Navigate to create-agent page with agent data for editing
				this.$router.push({
					path: '/create-agent',
					query: {
						edit: 'true',
						agentName: agent.name,
						agentId: agent.object_id,
						returnTo: 'manage-agents'
					}
				});
			},

			hideAllPoppers,

			onTabChange(e: { index: number }) {
				this.activeTabIndex = e.index;
			},

			getEmptyMessage(): string {
				if (this.agentSearchTerm.trim()) {
					return 'No agents found matching your search.';
				}
				if (this.showEnabledOnly) {
					return 'No enabled agents available.';
				}
				return this.emptyAgentsMessage2 || 'No agents available.';
			},

			async checkContributorRoles() {
				try {
					const result = await api.checkContributorRoles();
					this.hasAgentsContributorRole = result.hasAgentsContributorRole;
					this.hasPromptsContributorRole = result.hasPromptsContributorRole;
				} catch (error) {
					console.error('Failed to check contributor roles:', error);
					this.hasAgentsContributorRole = false;
					this.hasPromptsContributorRole = false;
				}
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

	.chat:focus-within .chat__icons,
	.chat:focus .chat__icons,
	.chat--selected .chat__icons {
		display: flex;
		opacity: 1;
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
	}

	.config-loading {
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 12px;
		color: var(--text-color-secondary);
		font-size: 14px;
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

<style>
	.csm-profile-setting-modal-1{
		max-width: 750px;
	}
	.csm-table-container-1{
		max-height: 240px;
		min-height: 240px;
		overflow-y: auto;
		border: 1px solid #dedede;
		margin-bottom: 20px;
	}
	.csm-table-1{
		width: 100%;
		border-collapse: collapse;
	}
	.csm-table-1 thead th{
		text-align: left;
		background-color: #eeeeee;
		padding: 10px;
		border-right: 2px solid #ffffff;
		font-weight: 500;
		position: sticky;
		top: 0;
		z-index: 1;
	}
	.csm-table-1 thead tr th:last-child{
		border-right: 0px;
	}
	.csm-table-1 tbody td{
		text-align: left;
		padding: 10px;
		border-bottom: 1px solid #dedede;
	}
	.csm-table-1 tbody tr:last-child td{
		border-bottom: 0px;
	}
	.csm-table-1 thead tr th:last-child, .csm-table-1 tbody tr td:last-child{
		text-align: center;
	}
	.csm-table-edit-btn-1.p-button:not(.p-button-text){
		background-color: transparent !important;
		color: #b0b0b0 !important;
		transition: color 0.2s, box-shadow 0.2s;
	}

	.csm-table-edit-btn-strong.p-button:not(.p-button-text) {
		color: #2d5be3 !important;
		font-weight: 700;
		filter: drop-shadow(0 0 2px #2d5be3aa);
	}

	.csm-table-edit-btn-faded.p-button:not(.p-button-text) {
		color: #b0b0b0 !important;
		opacity: 0.5;
	}

	.csm-table-edit-btn-1.p-button:not(.p-button-text):hover,
	.csm-table-edit-btn-strong.p-button:not(.p-button-text):hover {
		color: #1746a2 !important;
		opacity: 1;
		filter: drop-shadow(0 0 4px #1746a2aa);
	}

	.csm-table-edit-btn-1.p-button:not(.p-button-text):active,
	.csm-table-edit-btn-strong.p-button:not(.p-button-text):active {
		color: #0d2c6c !important;
		filter: drop-shadow(0 0 6px #0d2c6c88);
	}

	/* Custom checkbox styling */
	.custom-checkbox {
		width: 21px;
		height: 21px;
		border: 2px solid #ced4da;
		border-radius: 4px;
		display: flex;
		align-items: center;
		justify-content: center;
		cursor: pointer;
		transition: all 0.2s ease;
		background-color: transparent;
		position: relative;
		margin: 0 auto;
	}

	.custom-checkbox:hover {
		border-color: #5472d4;
		background-color: rgba(84, 114, 212, 0.1);
	}

	.custom-checkbox.checked {
		background-color: #5472d4;
		border-color: #5472d4;
	}

	.custom-checkbox.checked i {
		color: white;
		font-size: 12px;
		font-weight: bold;
	}

	.custom-checkbox.disabled {
		cursor: not-allowed;
		opacity: 0.6;
		background-color: #e9ecef !important;
		border-color: #adb5bd !important;
	}

	.custom-checkbox.disabled:hover {
		background-color: #e9ecef !important;
		border-color: #adb5bd !important;
	}

	.custom-checkbox.disabled.checked {
		background-color: #6c757d !important;
		border-color: #6c757d !important;
	}
	.csm-only-text-btn-1{
		text-decoration: none;
		padding: 0px;
		color: #5472d4;
		font-weight: 500;
	}
	.csm-sEnabled-checkbox-1 label{
		cursor: pointer;
	}

	/* Permission request container styling */
	.permission-request-container {
		padding: 16px;
		margin-top: 16px;
		background-color: #f8f9fa;
		border: 1px solid #e9ecef;
		border-radius: 8px;
	}

	.permission-request-message {
		display: flex;
		align-items: center;
		flex-wrap: wrap;
		gap: 8px;
		font-size: 14px;
		color: #6c757d;
	}

	.permission-request-link {
		color: #5472d4;
		text-decoration: none;
		font-weight: 500;
		transition: color 0.2s ease;
	}

	.permission-request-link:hover {
		color: #1746a2;
		text-decoration: underline;
	}
</style>
