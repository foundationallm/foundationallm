<template>
	<div class="analytics-page">
		<h1>Analytics Overview</h1>

		<div v-if="loading" class="loading">
			<ProgressSpinner />
		</div>

		<div v-else-if="error" class="error">
			<Message severity="error">{{ error }}</Message>
		</div>

		<div v-else-if="overview" class="analytics-overview">
			<div class="metrics-grid">
				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ overview.total_conversations }}</div>
						<div class="metric-label">Total Conversations</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumber(overview.total_tokens) }}</div>
						<div class="metric-label">Total Tokens</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ overview.active_users }}</div>
						<div class="metric-label">Active Users</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ overview.total_agents }}</div>
						<div class="metric-label">Total Agents</div>
					</template>
				</Card>
			</div>

			<div class="date-range-selector">
				<label>Date Range:</label>
				<Calendar v-model="startDate" dateFormat="yy-mm-dd" placeholder="Start Date" />
				<Calendar v-model="endDate" dateFormat="yy-mm-dd" placeholder="End Date" />
				<Button label="Refresh" @click="loadData" />
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '@/js/api';

const loading = ref(true);
const error = ref<string | null>(null);
const overview = ref<any>(null);
const startDate = ref<Date | null>(null);
const endDate = ref<Date | null>(null);

const formatNumber = (num: number) => {
	if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
	if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
	return num.toString();
};

const loadData = async () => {
	loading.value = true;
	error.value = null;
	try {
		const start = startDate.value?.toISOString();
		const end = endDate.value?.toISOString();
		overview.value = await api.getAnalyticsOverview(start, end);
	} catch (err: any) {
		error.value = err.message || 'Failed to load analytics data';
	} finally {
		loading.value = false;
	}
};

onMounted(() => {
	loadData();
});
</script>

<style scoped>
.analytics-page {
	padding: 2rem;
}

.metrics-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
	gap: 1.5rem;
	margin-bottom: 2rem;
}

.metric-card {
	text-align: center;
}

.metric-value {
	font-size: 2rem;
	font-weight: bold;
	color: var(--primary-color);
}

.metric-label {
	margin-top: 0.5rem;
	color: var(--text-color-secondary);
}

.date-range-selector {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-top: 2rem;
}

.loading,
.error {
	text-align: center;
	padding: 2rem;
}
</style>
