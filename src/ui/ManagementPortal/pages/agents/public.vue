<template>
	<main id="main-content">
		<h2 class="page-header">All Agents</h2>
		<div class="page-subheader">View your publicly accessible agents.</div>

		<AgentsList
			:agents="agents"
			:loading="loading"
			:loading-status-text="loadingStatusText"
			@refresh-agents="getAgents"
		/>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import AgentsList from '@/components/AgentsList.vue';
import type { Agent, ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'PublicAgents',

	components: {
		AgentsList,
	},

	data() {
		return {
			agents: [] as ResourceProviderGetResult<Agent>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
		};
	},

	async created() {
		await this.getAgents();
	},

	methods: {
		async getAgents() {
			this.loading = true;
			try {
				this.agents = await api.getAgents();
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

<style lang="scss" scoped></style>
