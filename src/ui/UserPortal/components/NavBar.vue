<template>
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
						:icon="$appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
						size="small"
						severity="secondary"
						class="secondary-button"
						aria-label="Toggle sidebar"
						@click="$appStore.toggleSidebar"
					/>
					<template #popper>Toggle sidebar</template>
				</VTooltip>
			</template>
		</div>

		<!-- Navbar content -->
		<div class="navbar__content">
			<div class="navbar__content__left">
				<div class="navbar__content__left__item">
					<template v-if="currentSession">
						<span class="current_session_name">{{ currentSession.name }}</span>
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
						<Toast position="top-center" />
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
							/>
							<template #popper> Select an agent </template>
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
							@change="handleAgentChange"
						/>
					</span>
				</template>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { Session } from '@/js/types';

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
		};
	},

	computed: {
		currentSession() {
			return this.$appStore.currentSession;
		},
	},

	watch: {
		currentSession(newSession: Session, oldSession: Session) {
			if (newSession.id !== oldSession?.id) {
				this.updateAgentSelection();
			}
		},
		'$appStore.selectedAgents': {
			handler() {
				this.updateAgentSelection();
			},
			deep: true,
		},
	},

	async created() {
		await this.$appStore.getAgents();

		this.agentOptions = this.$appStore.agents.map((agent) => ({
			label: agent.resource.name,
			type: agent.resource.type,
			object_id: agent.resource.object_id,
			description: agent.resource.description,
			my_agent: agent.roles.includes('Owner'),
			value: agent,
		}));

		const publicAgentOptions = this.agentOptions;
		const privateAgentOptions = this.agentOptions.filter((agent) => agent.my_agent);
		const noAgentOptions = [{ label: 'None', value: null, disabled: true }];
		this.virtualUser = await this.$appStore.getVirtualUser();

		this.agentOptionsGroup.push({
			label: '',
			items: [{ label: '--select--', value: null }],
		});

		this.agentOptionsGroup.push({
			label: 'All Agents',
			items: publicAgentOptions.length > 0 ? publicAgentOptions : noAgentOptions,
		});

		this.agentOptionsGroup.push({
			label: 'My Agents',
			items: privateAgentOptions.length > 0 ? privateAgentOptions : noAgentOptions,
		});
	},

	mounted() {
		this.updateAgentSelection();
	},

	methods: {
		handleAgentChange() {
			this.$appStore.setSessionAgent(this.currentSession, this.agentSelection!.value);
			const message = this.agentSelection!.value
				? `Agent changed to ${this.agentSelection!.label}`
				: `Cleared agent hint selection`;

			this.$toast.add({
				severity: 'success',
				detail: message,
				life: 5000,
			});
		},

		async handleLogout() {
			await this.$authStore.logout();
		},

		// handleCopySession() {
		// 	const chatLink = `${window.location.origin}?chat=${this.currentSession!.id}`;
		// 	navigator.clipboard.writeText(chatLink);

		// 	this.$toast.add({
		// 		severity: 'success',
		// 		detail: 'Chat link copied!',
		// 		life: 5000,
		// 	});
		// },

		updateAgentSelection() {
			const agent = this.$appStore.getSessionAgent(this.currentSession);
			this.agentSelection =
				this.agentOptions.find(
					(option) => option.value.resource.object_id === agent.resource.object_id,
				) || null;
		},
	},
};
</script>

<style lang="scss" scoped>
.navbar {
	height: 70px;
	width: 100%;
	overflow: hidden;
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
	border-bottom: 1px solid #eaeaea;
	color: var(--accent-text);
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
	color: var(--accent-text);
}

.button--auth {
	margin-left: 24px;
}

.secondary-button {
	background-color: var(--secondary-button-bg) !important;
	border-color: var(--secondary-button-bg) !important;
	color: var(--secondary-button-text) !important;
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

.p-dropdown-items-wrapper {
	max-height: 300px !important;
}
</style>
