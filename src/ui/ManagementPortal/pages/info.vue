<template>
	<div>
		<h1>Deployment Information</h1>
		<p>This page provides information about the FoundationaLLM deployment.</p>
		<p>
			<strong>Instance ID:</strong> {{ $appConfigStore.instanceId }}
        </p>
		<h3>API Status</h3>
		<div class="api-cards">
			<ApiStatusCard
				v-for="api in apiUrls"
				:key="api.name"
				:apiName="api.displayName"
				:apiUrl="api.url"
			/>
		</div>
	</div>
</template>

<script>
import ApiStatusCard from '@/components/ApiStatusCard.vue';

export default {
	components: {
		ApiStatusCard,
	},
	data() {
		return {
			apiUrls: [],
			loading: true,
		};
	},
	async mounted() {
		await this.fetchApiUrls();
	},
	methods: {
		async fetchApiUrls() {
			this.loading = true;

			this.apiUrls = [
				{ name: 'coreApiUrl', displayName: 'Core API', url: this.$appConfigStore.coreApiUrl },
				{ name: 'apiUrl', displayName: 'Management API', url: this.$appConfigStore.apiUrl },
				{
					name: 'gatekeeperApiUrl',
					displayName: 'Gatekeeper API',
					url: this.$appConfigStore.gatekeeperApiUrl,
				},
				{
					name: 'gatekeeperIntegrationApiUrl',
					displayName: 'Gatekeeper Integration API',
					url: this.$appConfigStore.gatekeeperIntegrationApiUrl,
				},
				{
					name: 'gatewayApiUrl',
					displayName: 'Gateway API',
					url: this.$appConfigStore.gatewayApiUrl,
				},
				{
					name: 'orchestrationApiUrl',
					displayName: 'Orchestration API',
					url: this.$appConfigStore.orchestrationApiUrl,
				},
				{
					name: 'vectorizationApiUrl',
					displayName: 'Vectorization API',
					url: this.$appConfigStore.vectorizationApiUrl,
				},
				{
					name: 'vectorizationWorkerApiUrl',
					displayName: 'Vectorization Worker API',
					url: this.$appConfigStore.vectorizationWorkerApiUrl,
				},
			];
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
