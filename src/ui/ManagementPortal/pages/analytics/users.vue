<template>
	<div class="analytics-users-page">
		<h1>User Analytics</h1>

		<div v-if="loading" class="loading">
			<ProgressSpinner />
		</div>

		<div v-else-if="error" class="error">
			<Message severity="error">{{ error }}</Message>
		</div>

		<div v-else>
			<div class="controls">
				<label>Sort By:</label>
				<Select v-model="sortBy" :options="sortOptions" optionLabel="label" optionValue="value" />
				<label>Top:</label>
				<InputNumber v-model="topCount" :min="1" :max="100" />
				<Button label="Refresh" @click="loadData" />
			</div>

			<DataTable :value="topUsers" :paginator="true" :rows="20" class="users-table">
				<Column field="username" header="Username" />
				<Column field="total_requests" header="Total Requests" />
				<Column field="total_tokens" header="Total Tokens">
					<template #body="slotProps">
						{{ formatNumber(slotProps.data.total_tokens) }}
					</template>
				</Column>
				<Column field="active_sessions" header="Active Sessions" />
				<Column field="abuse_risk_score" header="Risk Score">
					<template #body="slotProps">
						<span :class="getRiskScoreClass(slotProps.data.abuse_risk_score)">
							{{ slotProps.data.abuse_risk_score }}
						</span>
					</template>
				</Column>
				<Column field="last_activity" header="Last Activity">
					<template #body="slotProps">
						{{ formatDate(slotProps.data.last_activity) }}
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
const topUsers = ref<any[]>([]);
const sortBy = ref('Requests');
const topCount = ref(10);

const sortOptions = [
	{ label: 'Requests', value: 'Requests' },
	{ label: 'Tokens', value: 'Tokens' },
	{ label: 'Sessions', value: 'Sessions' },
	{ label: 'Risk Score', value: 'RiskScore' },
];

const formatNumber = (num: number) => {
	if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
	if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
	return num.toString();
};

const formatDate = (date: string) => {
	return new Date(date).toLocaleString();
};

const getRiskScoreClass = (score: number) => {
	if (score >= 80) return 'risk-critical';
	if (score >= 60) return 'risk-high';
	if (score >= 40) return 'risk-medium';
	return 'risk-low';
};

const loadData = async () => {
	loading.value = true;
	error.value = null;
	try {
		topUsers.value = await api.getTopUsers(topCount.value, sortBy.value);
	} catch (err: any) {
		error.value = err.message || 'Failed to load user analytics';
	} finally {
		loading.value = false;
	}
};

onMounted(() => {
	loadData();
});
</script>

<style scoped>
.analytics-users-page {
	padding: 2rem;
}

.controls {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 2rem;
}

.users-table {
	margin-top: 1rem;
}

.risk-critical {
	color: red;
	font-weight: bold;
}

.risk-high {
	color: orange;
	font-weight: bold;
}

.risk-medium {
	color: yellow;
}

.risk-low {
	color: green;
}
</style>
