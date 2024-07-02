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
                :description="api.description"
			/>
		</div>

        <h3>External Orchestration Service Status</h3>
		<div class="api-cards">
			<ApiStatusCard
				v-for="api in externalOrchestrationServices"
				:key="api.name"
				:apiName="api.name"
				:apiUrl="api.resolved_api_url"
                :description="api.description"
			/>
		</div>

	</div>
</template>

<script lang="ts">
import ApiStatusCard from '@/components/ApiStatusCard.vue';
import api from '@/js/api';
import type {
	ExternalOrchestrationService,
} from '@/js/types';

export default {
	components: {
		ApiStatusCard,
	},
	data() {
		return {
			apiUrls: [],
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

			this.apiUrls = [
				{
                    name: 'coreApiUrl',
                    displayName: 'Core API',
                    description: 'The Core API is the main user-based API for the FoundationaLLM platform. It is accessed by the User Portal.',
                    url: this.$appConfigStore.coreApiUrl
                },
				{
                    name: 'apiUrl',
                    displayName: 'Management API',
                    description: 'The Management API is used by the Management Portal to manage the FoundationaLLM platform.',
                    url: this.$appConfigStore.apiUrl
                },
				{
					name: 'gatekeeperApiUrl',
					displayName: 'Gatekeeper API',
                    description: 'The Gatekeeper API adds an additional layer of security to the FoundationaLLM platform. It is accessed by the Core API.',
					url: this.$appConfigStore.gatekeeperApiUrl,
				},
				{
					name: 'gatekeeperIntegrationApiUrl',
					displayName: 'Gatekeeper Integration API',
                    description: 'The Gatekeeper Integration API is used to integrate additional filtering and safety services into the Gatekeeper API.',
					url: this.$appConfigStore.gatekeeperIntegrationApiUrl,
				},
				{
					name: 'gatewayApiUrl',
					displayName: 'Gateway API',
                    description: 'The Gateway API is used to manage the connection between the FoundationaLLM platform and LLMs.',
					url: this.$appConfigStore.gatewayApiUrl,
				},
				{
					name: 'orchestrationApiUrl',
					displayName: 'Orchestration API',
                    description: 'The Orchestration API is used to manage the orchestration of LLMs and other services in the FoundationaLLM platform.',
					url: this.$appConfigStore.orchestrationApiUrl,
				},
				{
					name: 'vectorizationApiUrl',
					displayName: 'Vectorization API',
                    description: 'The Vectorization API is used to manage vectorization requests for the Vectorization Worker service.',
					url: this.$appConfigStore.vectorizationApiUrl,
				},
			];

            try {
                this.loadingStatusText = 'Retrieving external orchestration services...';
                const externalOrchestrationServicesResult = await api.getExternalOrchestrationServices();
			    this.externalOrchestrationServices = externalOrchestrationServicesResult.map(result => result.resource);
            } catch (error) {
                this.$toast.add({
                    severity: 'error',
                    detail: error?.response?._data || error,
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
