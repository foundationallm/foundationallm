<template>
	<main id="main-content">
		<h2 class="page-header">Welcome to the Management Portal</h2>
		<div class="page-subheader">Select a section below to get started, or browse the navigation in the sidebar.</div>

		<!-- Back button when viewing sub-widgets -->
		<div v-if="activeSection" class="back-navigation">
			<Button
				icon="pi pi-arrow-left"
				label="Back to all sections"
				text
				@click="activeSection = null"
			/>
		</div>

		<!-- Main Widgets -->
		<div v-if="!activeSection" class="navigation-widgets">
			<!-- Agents -->
			<div class="widget" tabindex="0" @click="activeSection = 'agents'" @keydown.enter="activeSection = 'agents'">
				<div class="widget__icon">
					<i class="pi pi-users"></i>
				</div>
				<div class="widget__content">
					<h3>Agents</h3>
					<p>Create and manage AI agents, configure prompts, and customize agent behavior</p>
				</div>
				<i class="pi pi-chevron-right widget__chevron"></i>
			</div>

			<!-- Data -->
			<div class="widget" tabindex="0" @click="activeSection = 'data'" @keydown.enter="activeSection = 'data'">
				<div class="widget__icon">
					<i class="pi pi-database"></i>
				</div>
				<div class="widget__content">
					<h3>Data</h3>
					<p>Manage data pipelines, sources, knowledge units, and vector databases</p>
				</div>
				<i class="pi pi-chevron-right widget__chevron"></i>
			</div>

			<!-- Models and Endpoints -->
			<div class="widget" tabindex="0" @click="activeSection = 'models'" @keydown.enter="activeSection = 'models'">
				<div class="widget__icon">
					<i class="pi pi-box"></i>
				</div>
				<div class="widget__content">
					<h3>Models &amp; Endpoints</h3>
					<p>Configure AI models and API endpoint connections</p>
				</div>
				<i class="pi pi-chevron-right widget__chevron"></i>
			</div>

			<!-- Security -->
			<div class="widget" tabindex="0" @click="activeSection = 'security'" @keydown.enter="activeSection = 'security'">
				<div class="widget__icon">
					<i class="pi pi-shield"></i>
				</div>
				<div class="widget__content">
					<h3>Security</h3>
					<p>Manage access control, role assignments, and permissions</p>
				</div>
				<i class="pi pi-chevron-right widget__chevron"></i>
			</div>

			<!-- FLLM Platform -->
			<div class="widget" tabindex="0" @click="activeSection = 'platform'" @keydown.enter="activeSection = 'platform'">
				<div class="widget__icon">
					<i class="pi pi-cog"></i>
				</div>
				<div class="widget__content">
					<h3>Platform Settings</h3>
					<p>Configure branding, deployment settings, and view system information</p>
				</div>
				<i class="pi pi-chevron-right widget__chevron"></i>
			</div>
		</div>

		<!-- Sub-Widgets for Agents -->
		<div v-if="activeSection === 'agents'" class="navigation-widgets">
			<div class="widget widget--sub" tabindex="0" @click="$router.push('/agents/create')" @keydown.enter="$router.push('/agents/create')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-plus"></i>
				</div>
				<div class="widget__content">
					<h3>Create New Agent</h3>
					<p>Build a new AI agent from scratch</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/agents/public')" @keydown.enter="$router.push('/agents/public')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-list"></i>
				</div>
				<div class="widget__content">
					<h3>All Agents</h3>
					<p>View and manage all available agents</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/agents/private')" @keydown.enter="$router.push('/agents/private')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-user"></i>
				</div>
				<div class="widget__content">
					<h3>My Agents</h3>
					<p>View and manage your personal agents</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/prompts')" @keydown.enter="$router.push('/prompts')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-comment"></i>
				</div>
				<div class="widget__content">
					<h3>Prompts</h3>
					<p>Manage prompt templates for agents</p>
				</div>
			</div>
		</div>

		<!-- Sub-Widgets for Data -->
		<div v-if="activeSection === 'data'" class="navigation-widgets">
			<div class="widget widget--sub" tabindex="0" @click="$router.push('/pipelines')" @keydown.enter="$router.push('/pipelines')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-sitemap"></i>
				</div>
				<div class="widget__content">
					<h3>Data Pipelines</h3>
					<p>Configure data ingestion and processing pipelines</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/pipeline-runs')" @keydown.enter="$router.push('/pipeline-runs')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-play"></i>
				</div>
				<div class="widget__content">
					<h3>Pipeline Runs</h3>
					<p>Monitor and manage pipeline executions</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/data-sources')" @keydown.enter="$router.push('/data-sources')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-folder-open"></i>
				</div>
				<div class="widget__content">
					<h3>Data Sources</h3>
					<p>Configure connections to data sources</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/knowledge-units')" @keydown.enter="$router.push('/knowledge-units')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-book"></i>
				</div>
				<div class="widget__content">
					<h3>Knowledge Units</h3>
					<p>Manage knowledge unit configurations</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/knowledge-sources')" @keydown.enter="$router.push('/knowledge-sources')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-file"></i>
				</div>
				<div class="widget__content">
					<h3>Knowledge Sources</h3>
					<p>View indexed knowledge sources</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/vector-databases')" @keydown.enter="$router.push('/vector-databases')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-th-large"></i>
				</div>
				<div class="widget__content">
					<h3>Vector Databases</h3>
					<p>Configure vector storage connections</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/knowledge-explorer')" @keydown.enter="$router.push('/knowledge-explorer')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-search"></i>
				</div>
				<div class="widget__content">
					<h3>Knowledge Explorer</h3>
					<p>Explore and search indexed knowledge</p>
				</div>
			</div>
		</div>

		<!-- Sub-Widgets for Models & Endpoints -->
		<div v-if="activeSection === 'models'" class="navigation-widgets">
			<div class="widget widget--sub" tabindex="0" @click="$router.push('/models')" @keydown.enter="$router.push('/models')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-box"></i>
				</div>
				<div class="widget__content">
					<h3>AI Models</h3>
					<p>Configure and manage AI model definitions</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/api-endpoints')" @keydown.enter="$router.push('/api-endpoints')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-link"></i>
				</div>
				<div class="widget__content">
					<h3>API Endpoints</h3>
					<p>Manage API endpoint configurations</p>
				</div>
			</div>
		</div>

		<!-- Sub-Widgets for Security -->
		<div v-if="activeSection === 'security'" class="navigation-widgets">
			<div class="widget widget--sub" tabindex="0" @click="$router.push('/security/role-assignments')" @keydown.enter="$router.push('/security/role-assignments')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-key"></i>
				</div>
				<div class="widget__content">
					<h3>Access Control</h3>
					<p>Manage role assignments and permissions</p>
				</div>
			</div>
		</div>

		<!-- Sub-Widgets for Platform Settings -->
		<div v-if="activeSection === 'platform'" class="navigation-widgets">
			<div class="widget widget--sub" tabindex="0" @click="$router.push('/branding')" @keydown.enter="$router.push('/branding')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-palette"></i>
				</div>
				<div class="widget__content">
					<h3>Branding</h3>
					<p>Customize portal appearance and branding</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/configuration')" @keydown.enter="$router.push('/configuration')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-cog"></i>
				</div>
				<div class="widget__content">
					<h3>Configuration</h3>
					<p>Manage platform configuration settings</p>
				</div>
			</div>

			<div class="widget widget--sub" tabindex="0" @click="$router.push('/info')" @keydown.enter="$router.push('/info')">
				<div class="widget__icon widget__icon--sub">
					<i class="pi pi-server"></i>
				</div>
				<div class="widget__content">
					<h3>Deployment Information</h3>
					<p>View API status and deployment details</p>
				</div>
			</div>
		</div>
	</main>
</template>

<script lang="ts">
export default {
	name: 'ManagementPortal',

	data() {
		return {
			activeSection: null as string | null,
		};
	},
};
</script>

<style lang="scss" scoped>
.back-navigation {
	margin-bottom: 1rem;
}

.navigation-widgets {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
	gap: 1.5rem;
	margin-top: 1rem;
}

.widget {
	display: flex;
	align-items: flex-start;
	padding: 1.5rem;
	border: 1px solid var(--surface-border);
	border-radius: 12px;
	background-color: var(--surface-card);
	cursor: pointer;
	transition: all 0.2s ease;

	&:hover {
		transform: translateY(-4px);
		box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
		border-color: var(--primary-color);
	}

	&:focus-visible {
		outline: 2px solid var(--primary-color);
		outline-offset: 2px;
	}
}

.widget--sub {
	background-color: var(--surface-ground);
}

.widget__icon {
	display: flex;
	align-items: center;
	justify-content: center;
	width: 56px;
	height: 56px;
	border-radius: 12px;
	background-color: var(--primary-color);
	flex-shrink: 0;
	margin-right: 1rem;

	.pi {
		font-size: 1.5rem;
		color: var(--primary-text);
	}
}

.widget__icon--sub {
	width: 48px;
	height: 48px;
	background-color: var(--surface-200);

	.pi {
		font-size: 1.25rem;
		color: var(--text-color);
	}
}

.widget__content {
	flex: 1;

	h3 {
		margin: 0 0 0.5rem 0;
		font-size: 1.125rem;
		font-weight: 600;
		color: var(--text-color);
	}

	p {
		margin: 0;
		font-size: 0.875rem;
		color: var(--text-color-secondary);
		line-height: 1.4;
	}
}

.widget__chevron {
	align-self: center;
	color: var(--text-color-secondary);
	font-size: 1rem;
	margin-left: 0.5rem;
}
</style>
