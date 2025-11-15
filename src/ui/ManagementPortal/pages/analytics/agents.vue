<template>
	<div class="analytics-agents-page">
		<h1>Agent Analytics</h1>

		<div v-if="loading" class="loading">
			<ProgressSpinner />
		</div>

		<div v-else-if="error" class="error">
			<Message severity="error">{{ error }}</Message>
		</div>

		<div v-else>
			<DataTable :value="agents" :paginator="true" :rows="20" class="agents-table">
				<Column field="agent_name" header="Agent Name" />
				<Column field="unique_users" header="Unique Users" />
				<Column field="total_conversations" header="Total Conversations" />
				<Column field="total_tokens" header="Total Tokens">
					<template #body="slotProps">
						{{ formatNumber(slotProps.data.total_tokens) }}
					</template>
				</Column>
				<Column field="avg_response_time_ms" header="Avg Response Time (ms)">
					<template #body="slotProps">
						{{ Math.round(slotProps.data.avg_response_time_ms) }}
					</template>
				</Column>
			</DataTable>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '@/js/api';

const loading = ref(true);
const error = ref<string | null>(null);
const agents = ref<any[]>([]);

const formatNumber = (num: number) => {
	if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
	if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
	return num.toString();
};

const loadData = async () => {
	loading.value = true;
	error.value = null;
	try {
		agents.value = await api.getAllAgentsAnalytics();
	} catch (err: any) {
		error.value = err.message || 'Failed to load agent analytics';
	} finally {
		loading.value = false;
	}
};

onMounted(() => {
	loadData();
});
</script>

<style scoped>
.analytics-agents-page {
	padding: 2rem;
}

.agents-table {
	margin-top: 1rem;
}
</style>
