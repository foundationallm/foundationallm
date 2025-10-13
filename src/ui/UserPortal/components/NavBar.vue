<template>
	<div>
		<Toast position="top-center" />
		<div class="navbar">
			<!-- Sidebar header -->
			<div class="navbar__header">
				<img
					v-if="$appConfigStore.logoUrl !== ''"
					:src="$appConfigStore.logoUrl"
					:alt="$appConfigStore.logoText"
				/>
				<span v-else>{{ $appConfigStore.logoText }}</span>

				<template v-if="!$appConfigStore.isKioskMode">
					<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
						<Button
							class="navbar__header__button"
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
				</template>
			</div>

			<!-- Navbar content -->
			<div class="navbar__content">
				<div class="navbar__content__left">
					<div class="navbar__content__left__item">
						<template v-if="currentSession">
							<span class="current_session_name">{{ currentSession.display_name }}</span>
							<!-- <VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button
									v-if="!$appConfigStore.isKioskMode"
									class="button--share"
									icon="pi pi-copy"
									text
									severity="secondary"
									aria-label="Copy link to chat session"
									@click="handleCopySession"
								/>
								<template #popper>Copy link to chat session</template>
							</VTooltip> -->
						</template>
						<template v-else>
							<span>Please select a session</span>
						</template>
						<template v-if="virtualUser">
							<span style="margin-left: 10px">{{ virtualUser }}</span>
						</template>
					</div>
				</div>

				<!-- Right side content -->
				<div class="navbar__content__right">
					<template v-if="currentSession">
						<span class="header__dropdown">
							<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
								<AgentIcon
									:src="$appConfigStore.agentIconUrl || '~/assets/FLLM-Agent-Light.svg'"
									alt="Select an agent"
									tabindex="0"
									@keydown.esc="hideAllPoppers"
								/>
								<template #popper><div role="tooltip">Select an agent</div></template>
							</VTooltip>
							<Dropdown
								v-model="agentSelection"
								:options="agentOptionsGroup"
								:style="{ maxHeight: '300px' }"
								class="dropdown--agent"
								option-group-label="label"
								option-group-children="items"
								option-disabled="disabled"
								option-label="label"
								placeholder="--Select--"
								aria-label="Select an agent"
								aria-activedescendant="selected-agent-{{ agentSelection?.label }}"
								@change="handleAgentChange"
								@show="handleDropdownShow"
							/>
							<Button
								class="print-button"
								icon="pi pi-print"
								aria-label="Print"
								@click="handlePrint"
							/>
						</span>
					</template>
				</div>
			</div>
		</div>

		<!-- No agents message -->
		<template v-if="showNoAgentsMessage">
			<div class="no-agents">
				<!-- eslint-disable-next-line vue/no-v-html -->
				<div class="body" v-html="emptyAgentsMessage"></div>
			</div>
		</template>
	</div>
</template>

<script lang="ts">
	import { hideAllPoppers } from 'floating-vue';
	import eventBus from '@/js/eventBus';
	import { isAgentExpired } from '@/js/helpers';
	import type { Session } from '@/js/types';
	import '@/styles/navbar.scss';

	interface AgentDropdownOption {
		label: string;
		value: any;
		disabled?: boolean;
		my_agent?: boolean;
		type: string;
		object_id: string;
		description: string;
	}

	interface AgentDropdownOptionsGroup {
		label: string;
		items: AgentDropdownOption[];
	}

	export default {
		name: 'NavBar',

		data() {
			return {
				agentSelection: null as AgentDropdownOption | null,
				agentOptions: [] as AgentDropdownOption[],
				agentOptionsGroup: [] as AgentDropdownOptionsGroup[],
				virtualUser: null as string | null,
				isMobile: window.screen.width < 950,
				emptyAgentsMessage: null as string | null,
			};
		},

		computed: {
			currentSession() {
				return this.$appStore.currentSession;
			},

			showNoAgentsMessage() {
				return this.$appStore.isInitialized && this.agentOptions.length === 0 && this.emptyAgentsMessage !== null;
			},
		},

		watch: {
			currentSession(newSession: Session, oldSession: Session) {
				if (newSession.sessionId !== oldSession?.sessionId) {
					this.updateAgentSelection();
				}
			},
			'$appStore.selectedAgents': {
				handler() {
					this.updateAgentSelection();
				},
				deep: true,
			},
			'$appStore.agents': {
				handler() {
					this.setAgentOptions();
				},
				deep: true,
			},
			'$appStore.lastSelectedAgent': {
				handler() {
					this.setAgentOptions();
					this.updateAgentSelection();
				},
				deep: true,
			},
			'$appStore.userProfiles': {
				handler() {
					this.setAgentOptions();
				},
				deep: true,
			},
			'$appConfigStore.featuredAgentNames': {
				handler() {
					// Refresh agent options when featuredAgentNames configuration becomes available
					this.setAgentOptions();
				},
			},
		},

		mounted() {
			this.updateAgentSelection();
		},

		methods: {
			async handleDropdownShow() {
				// Check if featuredAgentNames configuration is available and refresh agent options if needed
				// This handles the case where config is loaded after login
				if (this.$appConfigStore.featuredAgentNames === null) {
					try {
						await this.$appConfigStore.loadAppConfigurationSet();
						// setAgentOptions will be called automatically by the watcher
					} catch (error) {
						console.warn('Failed to load configuration on dropdown show:', error);
					}
				}
			},

			handleAgentChange() {
				if (isAgentExpired(this.agentSelection!.value)) return;

				this.$appStore.setSessionAgent(this.currentSession, this.agentSelection!.value, true);
				const message = this.agentSelection!.value
					? `Agent changed to ${this.agentSelection!.label}`
					: `Cleared agent hint selection`;

				this.$appStore.addToast({
					severity: 'success',
					detail: message,
				});

				if (this.$appStore.currentMessages?.length > 0) {
					// Emit the event to create a new session.
					// TODO: Add flag on the agent to determine whether to create a new session.
					eventBus.emit('agentChanged');
				}
			},

			async handleLogout() {
				await this.$authStore.logout();
			},

			handlePrint() {
				window.print();
			},

			async setAgentOptions() {
				// Check if configuration is loaded, if not try to load it
				if (this.$appConfigStore.featuredAgentNames === null) {
					try {
						await this.$appConfigStore.loadAppConfigurationSet();
					} catch (error) {
						console.warn('Failed to load configuration in setAgentOptions:', error);
					}
				}

				const isCurrentAgent = (agent: any): boolean => {
					return (
						agent.resource.name ===
						this.$appStore.getSessionAgent(this.currentSession)?.resource?.name
					);
				};

				// Get user profiles to filter enabled agents
				const userProfile = this.$appStore.userProfiles;
				const enabledAgentIds = userProfile?.agents || [];

				// Filter out expired agents, disabled agents, and agents not enabled in user profile
				// but keep the currently selected agent even if it doesn't meet these criteria				
				const filteredAgents = this.$appStore.agents.filter((agent: any) => {
					const isExpiredOrDisabled = isAgentExpired(agent) || agent.enabled === false;
					
					// Check if agent is in user profile by exact matching object_id
					const foundAgent = enabledAgentIds.find((agentId: any) => 
						agentId == agent.resource.object_id
					);
					const isNotInUserProfile = enabledAgentIds.length > 0 && !foundAgent;
					
					// Include if: (not expired AND not disabled AND in user profile) OR is current agent
					return (!isExpiredOrDisabled && !isNotInUserProfile) || isCurrentAgent(agent);
				});

				// Map filtered agents to dropdown options - no additional filtering needed since filtering was done above
				this.agentOptions = filteredAgents.map((agent: any) => ({
					label: agent.resource.display_name ? agent.resource.display_name : agent.resource.name,
					type: agent.resource.type,
					object_id: agent.resource.object_id,
					description: agent.resource.description,
					my_agent: agent.roles.includes('Owner'),
					value: agent,
				}));

				if (this.agentOptions.length === 0) {
					this.emptyAgentsMessage = this.$appConfigStore.noAgentsMessage ?? null;
				}

				// Get featured agent names from config
				const featuredAgentNames = this.$appConfigStore.featuredAgentNames;
				const featuredAgentNamesList = featuredAgentNames 
					? featuredAgentNames.split(',').map((name: string) => name.trim())
					: [];

				// Separate agents into featured and non-featured
				const featuredAgents: AgentDropdownOption[] = [];
				const nonFeaturedAgents: AgentDropdownOption[] = [];

				// First, add featured agents in the order specified in config
				featuredAgentNamesList.forEach((featuredName: string) => {
					const agent = this.agentOptions.find(option => option.value.resource.name === featuredName);
					if (agent) {
						featuredAgents.push(agent);
					}
				});

				// Add remaining agents to non-featured list
				this.agentOptions.forEach((agent: AgentDropdownOption) => {
					const isFeatured = featuredAgentNamesList.includes(agent.value.resource.name);
					if (!isFeatured) {
						nonFeaturedAgents.push(agent);
					}
				});

				this.virtualUser = await this.$appStore.getVirtualUser();

				this.agentOptionsGroup = [];
				
				// Add Featured group if there are featured agents
				if (featuredAgents.length > 0) {
					this.agentOptionsGroup.push({
						label: '-- Featured --',
						items: featuredAgents,
					});
				}
					
				// Add Other Agents group if there are non-featured agents
				if (nonFeaturedAgents.length > 0) {
					this.agentOptionsGroup.push({
						label: featuredAgents.length > 0 ? '-- Other --' : 'Other Agents',
						items: nonFeaturedAgents,
					});
				}

				// Update agent selection after options are set
				this.updateAgentSelection();
			},

			// handleCopySession() {
			// 	const chatLink = `${window.location.origin}?chat=${this.currentSession!.sessionId}`;
			// 	navigator.clipboard.writeText(chatLink);

			// 	this.$appStore.addToast({
			// 		severity: 'success',
			// 		detail: 'Chat link copied!',
			// 	});
			// },

			updateAgentSelection() {
				const agent = this.$appStore.getSessionAgent(this.currentSession);

				if (!agent) {
					this.agentSelection = null;
					return;
				}

				this.agentSelection =
					this.agentOptions.find(
						(option) => option.value.resource.object_id === agent.resource.object_id,
					) || null;
			},

			hideAllPoppers() {
				hideAllPoppers();
			},
		},
	};
</script>

<style lang="scss" scoped>
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
		color: var(--accent-text);
	}

	.button--auth {
		margin-left: 24px;
	}

	.header__dropdown {
		display: flex;
		align-items: center;
	}

	.avatar {
		width: 32px;
		height: 32px;
		border-radius: 50%;
		margin-right: 12px;
	}

	.navbar__header__button:focus {
		box-shadow: 0 0 0 0.1rem #fff;
	}

	.print-button {
		margin-left: 8px;
	}

	.no-agents {
		position: fixed;
		top: 60px;
		left: 50%;
		transform: translateX(-50%);
		background-color: #fafafa;
		box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
		border-radius: 6px;
		width: 55%;
		text-align: center;
		z-index: 1000;

		.body {
			color: #5f0000;
			padding: 10px 14px;
		}
	}

	@media only screen and (max-width: 620px) {
		.navbar__header {
			width: 95px;
			justify-content: center;

			img {
				display: none;
			}
		}
	}
</style>

<style>
	.dropdown--agent:focus {
		box-shadow: 0 0 0 0.1rem #000;
	}

	@media only screen and (max-width: 545px) {
		.dropdown--agent .p-dropdown-label {
			/* display: none; */
		}
		.dropdown--agent .p-dropdown-trigger {
			height: 40px;
		}
		.current_session_name {
			display: none;
		}
	}

	@media only screen and (max-width: 500px) {
		.dropdown--agent {
			max-width: 200px;
		}
	}

	@media only screen and (max-width: 450px) {
		.dropdown--agent {
			max-width: 160px;
		}
	}

	.p-dropdown-items-wrapper {
		max-height: 300px !important;
	}
</style>
