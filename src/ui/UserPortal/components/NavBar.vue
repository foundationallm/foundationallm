<template>
	<div>
		<Toast position="top-center" />
		<div class="navbar">
			<!-- Sidebar header -->
			<div class="navbar__header">
				<img
					v-if="appConfigStore.logoUrl !== ''"
					:src="appConfigStore.logoUrl"
					:alt="appConfigStore.logoText"
				/>
				<span v-else>{{ appConfigStore.logoText }}</span>

				<template v-if="!appConfigStore.isKioskMode">
					<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
						<Button
							class="navbar__header__button"
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
									v-if="!appConfigStore.isKioskMode"
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
									:src="appConfigStore.agentIconUrl || '~/assets/FLLM-Agent-Light.svg'"
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
	import { useAppStore } from '@/stores/appStore';
	import { useAppConfigStore } from '@/stores/appConfigStore';

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

		setup() {
			const appStore = useAppStore();
			const appConfigStore = useAppConfigStore();
			return { appStore, appConfigStore };
		},

		data() {
			return {
				agentSelection: null as AgentDropdownOption | null,
				agentOptions: [] as AgentDropdownOption[],
				agentOptionsGroup: [] as AgentDropdownOptionsGroup[],
				virtualUser: null as string | null,
				isMobile: window.screen.width < 950,
				emptyAgentsMessage: null as string | null,
				hasAvailableAgents: false as boolean,
			};
		},

		computed: {
			currentSession() {
				return this.appStore.currentSession;
			},

			showNoAgentsMessage() {
				return this.appStore.isInitialized && !this.hasAvailableAgents && this.emptyAgentsMessage !== null;
			},
		},

		watch: {
			// Rebuild agent options when agents list or user profile changes
			'appStore.agents': {
				handler() {
					this.setAgentOptions();
				},
				deep: true,
			},
			'appStore.userProfile': {
				handler() {
					this.setAgentOptions();
				},
				deep: true,
			},
			'appConfigStore.featuredAgentNames': {
				handler() {
					// Refresh agent options when featuredAgentNames configuration becomes available
					this.setAgentOptions();
				},
			},
			// Update dropdown selection when the centralized agent changes
			'appStore.currentSessionAgent': {
				handler() {
					this.updateAgentSelection();
				},
			},
		},

		mounted() {
			this.setAgentOptions();
			this.updateAgentSelection();
		},

		methods: {
			handleAgentChange() {
				if (isAgentExpired(this.agentSelection!.value)) return;

				this.appStore.setSessionAgent(this.currentSession, this.agentSelection!.value);
				const message = this.agentSelection!.value
					? `Agent changed to ${this.agentSelection!.label}`
					: `Cleared agent hint selection`;

				this.appStore.addToast({
					severity: 'success',
					life: 5000,
					detail: message,
				});

				if (this.appStore.currentMessages?.length > 0) {
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

				this.hasAvailableAgents = this.appStore.agents.some(
					(agent: any) => !isAgentExpired(agent));

				// Filter out expired agents, disabled agents, and agents not enabled in user profile
				const filteredAgents = this.appStore.agents.filter((agent: any) => {
					const isExpiredOrDisabled = isAgentExpired(agent) || agent.enabled === false;

					// Check if agent is in user profile by exact matching object_id
					const isEnabled = agent.properties['enabled'] || false;

					// Include if: (not expired AND not disabled AND in user profile)
					return (!isExpiredOrDisabled && isEnabled)
				});

				// Map filtered agents to dropdown options
				this.agentOptions = filteredAgents.map((agent: any) => ({
					label: agent.resource.display_name ? agent.resource.display_name : agent.resource.name,
					type: agent.resource.type,
					object_id: agent.resource.object_id,
					description: agent.resource.description,
					my_agent: agent.roles.includes('Owner'),
					value: agent,
				}));

				if (this.agentOptions.length === 0) {
					this.emptyAgentsMessage = this.appConfigStore.noAgentsMessage ?? null;
				}

				// Get featured agent names from config
				const featuredAgentNamesList = this.appConfigStore.featuredAgentNames;

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

				this.virtualUser = await this.appStore.getVirtualUser();

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

				// Update dropdown to reflect current agent from centralized store
				this.updateAgentSelection();
			},

			// handleCopySession() {
			// 	const chatLink = `${window.location.origin}?chat=${this.currentSession!.sessionId}`;
			// 	navigator.clipboard.writeText(chatLink);

			// 	this.appStore.addToast({
			// 		severity: 'success',
			// 		detail: 'Chat link copied!',
			// 	});
			// },

			updateAgentSelection() {
				// Read from the centralized source of truth
				const agent = this.appStore.currentSessionAgent;

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
