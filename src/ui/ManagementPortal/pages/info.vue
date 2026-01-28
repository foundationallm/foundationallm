<template>
	<main id="main-content" class="page-container">
		<h2 class="page-header">Deployment Information</h2>
		<div class="page-subheader">
			<p>This page provides information about the FoundationaLLM deployment.</p>
		</div>

		<section class="info-section mt-6">
			<h3 class="text-lg font-semibold mb-2">Instance Details</h3>
			<p class="text-sm text-gray-500 mb-4 instance-id-row">
				<strong>Instance ID:</strong> {{ $appConfigStore.instanceId }}
				<button 
					class="copy-button" 
					@click="copyToClipboard($appConfigStore.instanceId)"
					title="Copy to clipboard"
				>
					<i class="pi pi-copy"></i>
				</button>
			</p>
		</section>

		<section class="info-section mt-6">
			<h3 class="text-lg font-semibold mb-2">Installed Plugins</h3>
			<p class="text-sm text-gray-500 mb-4">
				View installed plugin packages and their versions.
			</p>
			<div v-if="pluginsLoading" class="loading-text">Loading plugins...</div>
			<div v-else-if="pluginPackages.length === 0" class="text-sm text-gray-500">No plugins installed.</div>
			<div v-else class="plugin-cards">
				<PluginCard
					v-for="plugin in pluginPackages"
					:key="plugin.name"
					:plugin="plugin"
				/>
			</div>
		</section>

		<section class="info-section mt-6">
			<h3 class="text-lg font-semibold mb-2">Agent Templates</h3>
			<p class="text-sm text-gray-500 mb-4">
				View available agent templates and their versions.
			</p>
			<div v-if="templatesLoading" class="loading-text">Loading agent templates...</div>
			<div v-else-if="agentTemplates.length === 0" class="text-sm text-gray-500">No agent templates available.</div>
			<div v-else class="template-cards">
				<AgentTemplateCard
					v-for="template in agentTemplates"
					:key="template.name"
					:template="template"
				/>
			</div>
		</section>

		<section class="info-section mt-6">
			<h3 class="text-lg font-semibold mb-2">API Status</h3>
			<p class="text-sm text-gray-500 mb-4">
				Monitor the status of the FoundationaLLM APIs.
			</p>
			<div class="api-cards">
				<ApiStatusCard
					v-for="api in apiUrls"
					:key="api.name"
					:api-name="api.display_name"
					:api-url="api.url"
					:status-endpoint="api.status_endpoint"
					:description="api.description"
				/>
			</div>
		</section>

		<!-- <section class="info-section mt-6">
			<h3 class="text-lg font-semibold mb-2">Orchestration and External Services</h3>
			<div class="api-cards">
				<ApiStatusCard
					v-for="api in externalOrchestrationServices"
					:key="api.name"
					:api-name="api.name"
					:api-url="api.url"
					:status-endpoint="api.status_endpoint"
					:description="api.description"
				/>
			</div>
		</section> -->
	</main>
</template>

<script lang="ts">
import ApiStatusCard from '@/components/ApiStatusCard.vue';
import PluginCard from '@/components/PluginCard.vue';
import AgentTemplateCard from '@/components/AgentTemplateCard.vue';
import api from '@/js/api';
import type { ExternalOrchestrationService } from '@/js/types';

export default {
	components: {
		ApiStatusCard,
		PluginCard,
		AgentTemplateCard,
	},

	data() {
		return {
			apiUrls: [] as Array<any>,
			pluginPackages: [] as Array<any>,
			agentTemplates: [] as Array<any>,
			pluginsLoading: false as boolean,
			templatesLoading: false as boolean,
			externalOrchestrationServices: [] as ExternalOrchestrationService[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
		};
	},

	async mounted() {
		await this.fetchApiUrls();
		await this.fetchPluginPackages();
		await this.fetchAgentTemplates();
	},

	methods: {
		async copyToClipboard(text: string) {
			try {
				await navigator.clipboard.writeText(text);
			} catch (err) {
				console.error('Failed to copy to clipboard:', err);
			}
		},

		async fetchApiUrls() {
			this.loading = true;
			const instancePart = `/instances/${this.$appConfigStore.instanceId}`;

			this.apiUrls = [
				{
					name: 'coreApiUrl',
					display_name: 'Core API',
					description:
						'The Core API is the main user-facing API that handles completions and conversations.',
					url: this.$appConfigStore.coreApiUrl,
					status_endpoint: `${instancePart}/status`,
				},
				{
					name: 'apiUrl',
					display_name: 'Management API',
					description:
						'The Management API is used by the Management Portal to manage the FoundationaLLM platform.',
					url: this.$appConfigStore.apiUrl,
					status_endpoint: `${instancePart}/status`,
				},
				{
					name: 'authorizationApiUrl',
					display_name: 'Authorization API',
					description:
						'The Authorization API manages role-based access control (RBAC) and other auth-related functions for the FoundationaLLM platform.',
					url: this.$appConfigStore.authorizationApiUrl,
					status_endpoint: `/status`,
				}
			] as Array<any>;

			this.loading = false;
		},

		async fetchPluginPackages() {
			this.pluginsLoading = true;
			try {
				const result = await api.getPluginPackages();
				this.pluginPackages = result
					.map((item: any) => item.resource)
					.sort((a: any, b: any) => {
						const nameA = (a.display_name || a.name).toLowerCase();
						const nameB = (b.display_name || b.name).toLowerCase();
						return nameA.localeCompare(nameB);
					});
			} catch (error: any) {
				console.error('Failed to fetch plugin packages:', error);
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || 'Failed to fetch plugin packages',
					life: 5000,
				});
			} finally {
				this.pluginsLoading = false;
			}
		},

		async fetchAgentTemplates() {
			this.templatesLoading = true;
			try {
				const result = await api.getAgentTemplates();
				this.agentTemplates = result
					.map((item: any) => item.resource)
					.sort((a: any, b: any) => {
						const nameA = (a.display_name || a.name).toLowerCase();
						const nameB = (b.display_name || b.name).toLowerCase();
						return nameA.localeCompare(nameB);
					});
			} catch (error: any) {
				console.error('Failed to fetch agent templates:', error);
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || 'Failed to fetch agent templates',
					life: 5000,
				});
			} finally {
				this.templatesLoading = false;
			}
		},
	},
};
</script>

<style scoped>
.info-section {
	margin-top: 2rem;
	padding-top: 1.5rem;
	border-top: 1px solid var(--surface-border);
}

.info-section:first-of-type {
	border-top: none;
	padding-top: 0;
}

.api-cards {
	display: flex;
	flex-direction: column;
	gap: 1rem;
}

.plugin-cards {
	display: flex;
	flex-direction: column;
	gap: 1rem;
}

.template-cards {
	display: flex;
	flex-direction: column;
	gap: 1rem;
}

.loading-text {
	font-size: 0.875rem;
	color: #666;
}

.instance-id-row {
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.copy-button {
	background: none;
	border: none;
	padding: 0.25rem;
	cursor: pointer;
	color: #666;
	border-radius: 4px;
	transition: color 0.2s, background-color 0.2s;
}

.copy-button:hover {
	color: #333;
	background-color: #f0f0f0;
}

.copy-button i {
	font-size: 0.875rem;
}
</style>
