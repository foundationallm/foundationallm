<template>
	<div
		:class="$appStore.sidebarCollapsed ? 'sidebar sidebar-collapsed' : 'sidebar'"
		role="navigation"
	>
		<h2 id="sidebar-title" class="visually-hidden">Main Navigation</h2>
		<!-- Sidebar section header -->
		<div class="sidebar__header">
			<template v-if="$appConfigStore.logoUrl">
				<NuxtLink to="/" aria-label="Navigate to homepage">
					<img
						:src="$filters.publicDirectory($appConfigStore.logoUrl)"
						aria-label="Logo as link to home"
						:alt="$appConfigStore.logoText"
					/>
				</NuxtLink>
			</template>
			<template v-else>
				<NuxtLink to="/">{{ $appConfigStore.logoText }}</NuxtLink>
			</template>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					:icon="$appStore.sidebarCollapsed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
					:aria-label="$appStore.sidebarCollapsed ? 'Open Sidebar' : 'Close Sidebar'"
					@click="$appStore.sidebarCollapsed = !$appStore.sidebarCollapsed"
				/>
				<template #popper>
					<div role="tooltip">
						{{ $appStore.sidebarCollapsed ? 'Open Sidebar' : 'Close Sidebar' }}
					</div>
				</template>
			</VTooltip>
		</div>

		<div v-if="!$appStore.sidebarCollapsed" class="sidebar__content">
			<div class="sidebar__navigation">
				<!-- Agents -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-users" aria-hidden="true"></span>
					<span>Agents</span>
				</h3>
				<ul>
					<li><NuxtLink to="/agents/create" class="sidebar__item">Create New Agent</NuxtLink></li>
					<li>
						<NuxtLink
							to="/agents/public"
							class="sidebar__item"
							:class="{ 'router-link-active': isRouteActive('/agents/edit') }"
							>All Agents</NuxtLink
						>
					</li>
					<li><NuxtLink to="/agents/private" class="sidebar__item">My Agents</NuxtLink></li>
					<li>
						<NuxtLink
							to="/prompts"
							class="sidebar__item"
							:class="{ 'router-link-active': isRouteActive('/prompts') }"
							>Prompts</NuxtLink
						>
					</li>
				</ul>
				<!-- <div class="sidebar__item">Performance</div> -->

				<!-- Analytics -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-chart-bar" aria-hidden="true"></span>
					<span>Analytics</span>
				</h3>
				<ul>
					<li>
						<NuxtLink
							to="/analytics"
							class="sidebar__item"
							:class="{ 'router-link-active': isRouteActive('/analytics') }"
							>Overview</NuxtLink
						>
					</li>
					<li>
						<NuxtLink
							to="/analytics/users"
							class="sidebar__item"
							:class="{ 'router-link-active': isRouteActive('/analytics/users') }"
							>Users</NuxtLink
						>
					</li>
					<li>
						<NuxtLink
							to="/analytics/agents"
							class="sidebar__item"
							:class="{ 'router-link-active': isRouteActive('/analytics/agents') }"
							>Agents</NuxtLink
						>
					</li>
				</ul>

				<!-- Data Catalog -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-database" aria-hidden="true"></span>
					<span>Data</span>
				</h3>
				<ul>
					<li>
						<NuxtLink
							to="/data-sources"
							:class="{ 'router-link-active': isRouteActive('/data-sources') }"
							class="sidebar__item"
							>Data Sources</NuxtLink
						>
					</li>
					<!-- <li>
						<NuxtLink
							to="/vector-stores"
							:class="{ 'router-link-active': isRouteActive('/vector-stores') }"
							class="sidebar__item"
							>Vector Stores</NuxtLink
						>
					</li> -->
					<li>
						<NuxtLink
							to="/pipelines"
							:class="{ 'router-link-active': isRouteActive('/pipelines') }"
							class="sidebar__item"
							>Data Pipelines</NuxtLink
						>
					</li>
					<li>
						<NuxtLink
							to="/pipeline-runs"
							:class="{ 'router-link-active': isRouteActive('/pipeline-runs') }"
							class="sidebar__item"
							>Data Pipeline Runs</NuxtLink
						>
					</li>
					<li>
						<NuxtLink
							to="/knowledge-sources"
							:class="{ 'router-link-active': isRouteActive('/knowledge-sources') }"
							class="sidebar__item"
							>Knowledge Sources</NuxtLink
						>
					</li>
				</ul>

				<!-- Models and Endpoints -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-box" aria-hidden="true"></span>
					<span>Models and Endpoints</span>
				</h3>
				<ul>
					<li>
						<NuxtLink
							to="/models"
							:class="{ 'router-link-active': isRouteActive('/models') }"
							class="sidebar__item"
							>AI Models</NuxtLink
						>
					</li>
					<li>
						<NuxtLink
							to="/api-endpoints"
							:class="{ 'router-link-active': isRouteActive('/api-endpoints') }"
							class="sidebar__item"
							>API Endpoints</NuxtLink
						>
					</li>
				</ul>

				<!-- Security -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-shield" aria-hidden="true"></span>
					<span>Security</span>
				</h3>
				<ul>
					<li>
						<NuxtLink to="/security/role-assignments" class="sidebar__item">
							Instance Access Control
						</NuxtLink>
					</li>
				</ul>

				<!-- FLLM Deployment -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-cloud" aria-hidden="true"></span>
					<span>FLLM Platform</span>
				</h3>
				<ul>
					<li><NuxtLink to="/branding" class="sidebar__item">Branding</NuxtLink></li>
					<li><NuxtLink to="/configuration" class="sidebar__item">Configuration</NuxtLink></li>
					<li><NuxtLink to="/info" class="sidebar__item">Deployment Information</NuxtLink></li>
				</ul>
			</div>

			<!-- Logged in user -->
			<div v-if="$authStore.currentAccount?.name" class="sidebar__account">
				<UserAvatar
					class="sidebar__avatar"
					size="large"
					:aria-label="`User Avatar for ${$authStore.currentAccount?.name}`"
				/>

				<div class="sidebar__username-container">
					<span
						:id="`username-tooltip-trigger-${$authStore.currentAccount?.username}`"
						class="sidebar__username"
						tabindex="0"
						:aria-describedby="`username-tooltip-${$authStore.currentAccount?.username}`"
						aria-label="Logged in as {{ $authStore.currentAccount?.username }}"
						@mouseenter="showUsernameTooltip"
						@mouseleave="hideUsernameTooltip"
						@focus="showUsernameTooltip"
						@blur="hideUsernameTooltip"
						@keydown.escape="hideUsernameTooltip"
					>
						{{ $authStore.currentAccount?.name }}
					</span>
					<Button
						class="sidebar__sign-out-button"
						icon="pi pi-sign-out"
						label="Sign Out"
						severity="secondary"
						size="small"
						aria-label="Sign out of the application"
						@click="$authStore.logout()"
					/>
				</div>
			</div>
		</div>
		
		<!-- Tooltip outside sidebar to avoid overflow issues -->
		<div
			v-if="$authStore.currentAccount?.name && showTooltip && tooltipReady"
			:id="`username-tooltip-${$authStore.currentAccount?.username}`"
			role="tooltip"
			class="username-tooltip"
			:aria-hidden="!showTooltip"
			:style="{
				top: tooltipPosition.top + 'px',
				left: tooltipPosition.left + 'px'
			}"
		>
			Logged in as {{ $authStore.currentAccount?.username }}
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'Sidebar',

	data() {
		return {
			isMobile: window.screen.width < 950,
			showTooltip: false,
			tooltipPosition: { top: 0, left: 0 },
			tooltipReady: false,
		};
	},

	created() {
		this.$appStore.initializeSidebarState();
	},

	methods: {
		isRouteActive(route) {
			return this.$route.path.startsWith(route);
		},
		showUsernameTooltip() {
			this.tooltipReady = false;
			this.updateTooltipPosition();
		},
		hideUsernameTooltip() {
			this.showTooltip = false;
			this.tooltipReady = false;
		},
		updateTooltipPosition() {
			this.$nextTick(() => {
				const trigger = document.getElementById(`username-tooltip-trigger-${this.$authStore.currentAccount?.username}`);
				if (trigger) {
					const rect = trigger.getBoundingClientRect();
					const viewportWidth = window.innerWidth;
					const tooltipWidth = 300; // Conservative estimate
					const margin = 35;
					
					// Try to center on username first
					let left = rect.left + (rect.width / 2);
					
					// Ensure tooltip doesn't go off the left edge
					const minLeft = tooltipWidth / 2 + margin;
					if (left < minLeft) {
						left = minLeft;
					}
					
					// Ensure tooltip doesn't go off the right edge
					const maxLeft = viewportWidth - tooltipWidth / 2 - margin;
					if (left > maxLeft) {
						left = maxLeft;
					}
					
					// Position tooltip above the username
					this.tooltipPosition = {
						top: rect.top - 40,
						left: left,
					};
					
					this.tooltipReady = true;
					this.showTooltip = true;
				}
			});
		},
	},
};
</script>

<style lang="scss" scoped>
a {
	text-decoration: none;
}

.sidebar {
	width: 300px;
	max-width: 300px;
	height: 100%;
	display: flex;
	flex-direction: column;
	background-color: var(--primary-color);
	z-index: 3;
	flex-shrink: 1;
	flex-grow: 1;
	overflow-y: auto;
}

.sidebar-collapsed {
	position: absolute;
	background-color: transparent;
	width: 100%;
	max-width: 100%;
	height: max-content;
}

.sidebar ul {
	list-style-type: none;
	padding: 0;
	margin: 0;
}

.sidebar li {
	margin: 0;
	padding: 0;
}

.sidebar__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	background-color: var(--primary-color);
	height: 70px;
	width: 300px;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	padding-bottom: 12px;
	color: var(--primary-text);

	img {
		max-height: 100%;
		width: 100%;
		max-width: 180px;
		margin-right: 12px;
	}
}

.sidebar__content {
	display: flex;
	height: 100%;
	flex-direction: column;
	flex-wrap: nowrap;
	align-items: stretch;
	overflow: hidden;
}

.sidebar__navigation {
	overflow-y: auto;
}

.sidebar__section-header {
	display: flex;
	align-items: center;
	gap: 10px;
	padding-bottom: 12px;
	padding-top: 12px;
	margin-left: 24px;
	margin-right: 24px;
	margin-bottom: 8px;
	margin-top: 24px;
	border-bottom: 1px solid rgba(255, 255, 255, 0.3);
	color: var(--primary-text);
	text-transform: uppercase;
	font-size: 0.95rem;
	font-weight: 600;
}

.sidebar__item {
	padding-bottom: 16px;
	padding-top: 16px;
	padding-left: 32px;
	padding-right: 32px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--primary-text);
	transition: all 0.1s ease-in-out;
	font-size: 0.8725rem;

	&.router-link-active,
	&:hover {
		background-color: rgba(217, 217, 217, 0.05);
	}
}

.sidebar__account {
	flex-grow: 1;
	color: var(--primary-text);
	display: grid;
	grid-template-columns: auto auto;
	align-items: end;
	justify-content: flex-start;
	padding: 12px 24px;
	text-transform: inherit;
}

.sidebar__avatar {
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

.sidebar__avatar img, .sidebar__avatar picture, .sidebar__avatar svg {
	   width: 100%;
	   height: 100%;
	   object-fit: cover;
	   border-radius: 50%;
	   display: block;
}

.sidebar__username-container {
	position: relative;
	display: inline-block;
}

.sidebar__username {
	color: var(--primary-text);
	font-size: 0.875rem;
	text-transform: capitalize;
	line-height: 0;
	vertical-align: super;
	cursor: pointer;
	outline: none;
	
	&:focus {
		outline: 2px solid var(--primary-text);
		outline-offset: 2px;
	}
}

.username-tooltip {
	position: fixed;
	background-color: rgba(0, 0, 0, 0.8);
	color: white;
	padding: 8px 12px;
	border-radius: 4px;
	font-size: 0.875rem;
	white-space: nowrap;
	z-index: 9999 !important;
	pointer-events: none;
	transform: translateX(-50%);
	
	&[aria-hidden="true"] {
		visibility: hidden;
		opacity: 0;
	}
	
	&[aria-hidden="false"] {
		visibility: visible;
		opacity: 1;
	}
}

.sidebar__sign-out-button {
	width: 100%;
}

.visually-hidden {
	position: absolute;
	overflow: hidden;
	clip: rect(0 0 0 0);
	height: 1px;
	width: 1px;
	margin: -1px;
	padding: 0;
	border: 0;
}

@media only screen and (max-width: 960px) {
	.sidebar {
		position: absolute;
	}
}
</style>
