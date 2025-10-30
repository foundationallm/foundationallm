<template>
	<main id="main-content">
		<h1>Deployment Information</h1>
		<p>This page provides information about the FoundationaLLM deployment.</p>
		<p><strong>Instance ID:</strong> {{ $appConfigStore.instanceId }}</p>
		<h3>API Status</h3>
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

		<!-- <h3>Orchestration and External Services</h3>
		<div class="api-cards">
			<ApiStatusCard
				v-for="api in externalOrchestrationServices"
				:key="api.name"
				:api-name="api.name"
				:api-url="api.url"
				:status-endpoint="api.status_endpoint"
				:description="api.description"
			/>
		</div> -->
	</main>
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
					name: 'apiUrl',
					display_name: 'Management API',
					description:
						'The Management API is used by the Management Portal to manage the FoundationaLLM platform.',
					url: this.$appConfigStore.apiUrl,
					status_endpoint: `/${instancePart}/status`,
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

			// try {
			// 	this.loadingStatusText = 'Retrieving external orchestration services...';
			// 	const externalOrchestrationServicesResult = await api.getExternalOrchestrationServices();
			// 	this.externalOrchestrationServices = externalOrchestrationServicesResult?.map(
			// 		(result) => result?.resource,
			// 	);
			// 	this.externalOrchestrationServices = this.externalOrchestrationServices.filter(
			// 		(service) => service.url,
			// 	);
			// } catch (error) {
			// 	this.$toast.add({
			// 		severity: 'error',
			// 		detail: error?.response?._data || error,
			// 		life: 5000,
			// 	});
			// }

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
