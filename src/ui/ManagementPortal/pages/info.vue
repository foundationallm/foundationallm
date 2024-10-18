<template>
	<div>
		<h1>Deployment Information</h1>
		<p>This page provides information about the FoundationaLLM deployment.</p>
		<p><strong>Instance ID:</strong> {{ $appConfigStore.instanceId }}</p>
		<h3>API Status</h3>
		<div class="api-cards">
			<ApiStatusCard
				v-for="api in apiUrls"
				:key="api.name"
				:api-name="api.displayName"
				:api-url="api.url"
				:status-url="api.statusUrl"
				:description="api.description"
			/>
		</div>

		<h3>Orchestration and External Services</h3>
		<div class="api-cards">
			<ApiStatusCard
				v-for="api in externalOrchestrationServices"
				:key="api.name"
				:api-name="api.name"
				:api-url="api.url"
				:status-url="api.status_url"
				:description="api.description"
			/>
		</div>
	</div>
</template>

<script lang="ts">
import ApiStatusCard from '@/components/ApiStatusCard.vue';
import api from '@/js/api';
import type { ExternalOrchestrationService } from '@/js/types';

export default {
	components: {
		ApiStatusCard,
	},

	data() {
		return {
			apiUrls: [] as Array<any>,
			externalOrchestrationServices: [] as ExternalOrchestrationService[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
		};
	},

	async mounted() {
		await this.fetchApiUrls();
	},

	methods: {
		async fetchApiUrls() {
			this.loading = true;
			const instancePart = `/instances/${this.$appConfigStore.instanceId}`;

			this.apiUrls = [
				{
					name: 'coreApiUrl',
					displayName: 'Core API',
					description:
						'The Core API is the main user-based API for the FoundationaLLM platform. It is accessed by the User Portal.',
					url: this.$appConfigStore.coreApiUrl,
					statusUrl: `${this.$appConfigStore.coreApiUrl}${instancePart}/status`,
				},
				{
					name: 'apiUrl',
					displayName: 'Management API',
					description:
						'The Management API is used by the Management Portal to manage the FoundationaLLM platform.',
					url: this.$appConfigStore.apiUrl,
					statusUrl: `${this.$appConfigStore.apiUrl}${instancePart}/status`,
				},
				{
					name: 'authorizationApiUrl',
					displayName: 'Authorization API',
					description:
						'The Authorization API manages role-based access control (RBAC) and other auth-related functions for the FoundationaLLM platform.',
					url: this.$appConfigStore.authorizationApiUrl,
					statusUrl: `${this.$appConfigStore.authorizationApiUrl}/status`,
				},
				{
					name: 'stateApiUrl',
					displayName: 'State API',
					description:
						'The State API manages background task and long-running operation state information for the FoundationaLLM platform.',
					url: this.$appConfigStore.stateApiUrl,
					statusUrl: `${this.$appConfigStore.stateApiUrl}/status`,
				},
			] as Array<any>;

			try {
				this.loadingStatusText = 'Retrieving external orchestration services...';
				const externalOrchestrationServicesResult = await api.getExternalOrchestrationServices();
				this.externalOrchestrationServices = externalOrchestrationServicesResult?.map(
					(result) => result?.resource,
				);
				this.externalOrchestrationServices = this.externalOrchestrationServices.filter(
					(service) => service.url,
				);
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			this.loading = false;
		},
	},
};
</script>

<style>
.api-cards {
	display: flex;
	flex-wrap: wrap;
}
</style>
