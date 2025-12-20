<template>
	<div class="analytics-tokens-page">
		<h1>Token Usage Analytics</h1>

		<div v-if="loading" class="loading">
			<ProgressSpinner />
		</div>

		<div v-else-if="error" class="error">
			<Message severity="error">{{ error }}</Message>
		</div>

		<div v-else>
			<div class="filters-row">
				<div class="date-range-selector">
					<Button 
						:label="dateRangeLabel" 
						icon="pi pi-calendar" 
						@click="toggleDateRangePanel"
					/>
					<OverlayPanel ref="dateRangePanel" :dismissable="true">
						<div class="date-range-menu">
							<div class="date-range-menu-header">
								<Button 
									label="< PREVIOUS" 
									text 
									severity="secondary"
									@click="navigateDateRange(-1)"
								/>
								<Button 
									label="NEXT >" 
									text 
									severity="secondary"
									@click="navigateDateRange(1)"
								/>
							</div>
							
							<div class="date-range-section">
								<div class="section-title">Recommended</div>
								<div class="date-range-option" @click="setDateRange('last7days')">
									<span>Last 7 days</span>
									<span class="date-preview">{{ formatDateRange('last7days') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('thisMonth')">
									<span>This month</span>
									<span class="date-preview">{{ formatDateRange('thisMonth') }}</span>
								</div>
								<div class="date-range-option" @click="showCustomDateRange">
									<span>Custom date range</span>
									<span class="pi pi-chevron-right"></span>
								</div>
							</div>

							<Divider />

							<div class="date-range-section">
								<div class="section-title">Relative dates</div>
								<div class="date-range-option" @click="setDateRange('last7days')">
									<span>Last 7 days</span>
									<span class="date-preview">{{ formatDateRange('last7days') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last30days')">
									<span>Last 30 days</span>
									<span class="date-preview">{{ formatDateRange('last30days') }}</span>
								</div>
							</div>

							<Divider />

							<div class="date-range-section">
								<div class="section-title">Calendar months</div>
								<div class="date-range-option" @click="setDateRange('thisMonth')">
									<span>This month</span>
									<span class="date-preview">{{ formatDateRange('thisMonth') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('thisQuarter')">
									<span>This quarter</span>
									<span class="date-preview">{{ formatDateRange('thisQuarter') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('thisYear')">
									<span>This year</span>
									<span class="date-preview">{{ formatDateRange('thisYear') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('lastMonth')">
									<span>Last month</span>
									<span class="date-preview">{{ formatDateRange('lastMonth') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('lastQuarter')">
									<span>Last quarter</span>
									<span class="date-preview">{{ formatDateRange('lastQuarter') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last3months')">
									<span>Last 3 months</span>
									<span class="date-preview">{{ formatDateRange('last3months') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last6months')">
									<span>Last 6 months</span>
									<span class="date-preview">{{ formatDateRange('last6months') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last12months')">
									<span>Last 12 months</span>
									<span class="date-preview">{{ formatDateRange('last12months') }}</span>
								</div>
								<div class="date-range-option" @click="showCustomDateRange">
									<span>Custom date range</span>
									<span class="pi pi-chevron-right"></span>
								</div>
							</div>
						</div>
					</OverlayPanel>

					<Dialog 
						v-model:visible="showCustomDialog" 
						modal 
						header="Custom Date Range"
						:style="{ width: '400px' }"
					>
						<div class="custom-date-range-dialog">
							<div class="date-input-group">
								<label>Start Date:</label>
								<Calendar v-model="startDate" dateFormat="yy-mm-dd" placeholder="Start Date" />
							</div>
							<div class="date-input-group">
								<label>End Date:</label>
								<Calendar v-model="endDate" dateFormat="yy-mm-dd" placeholder="End Date" />
							</div>
							<div class="dialog-actions">
								<Button label="Cancel" severity="secondary" @click="showCustomDialog = false" />
								<Button label="Apply" @click="applyCustomDateRange" />
							</div>
						</div>
					</Dialog>
				</div>
			</div>

			<!-- Summary Cards -->
			<div class="metrics-grid">
				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumber(totalTokens) }}</div>
						<div class="metric-label">Total Tokens Used</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumberWithCommas(totalUsers) }}</div>
						<div class="metric-label">Total Users</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumberWithCommas(totalAgents) }}</div>
						<div class="metric-label">Total Agents</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumber(averageTokensPerUser) }}</div>
						<div class="metric-label">Avg Tokens/User</div>
					</template>
				</Card>
			</div>

			<!-- Top Users Pie Chart -->
			<div v-if="topUsersChartData" class="chart-container">
				<Card>
					<template #title>Top 10 Users by Token Consumption</template>
					<template #content>
						<div class="pie-chart-wrapper">
							<canvas ref="topUsersPieCanvas"></canvas>
						</div>
					</template>
				</Card>
			</div>

			<!-- Token Usage by Agent -->
			<div class="table-section">
				<Card>
					<template #title>Token Usage by Agent</template>
					<template #content>
						<DataTable 
							:value="agents" 
							:paginator="true" 
							:rows="10" 
							class="tokens-table"
							sortField="total_tokens"
							:sortOrder="-1"
						>
							<Column field="agent_name" header="Agent Name" sortable></Column>
							<Column field="unique_users" header="Unique Users" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.unique_users ?? 0) }}
								</template>
							</Column>
							<Column field="total_conversations" header="Total Conversations" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_conversations ?? 0) }}
								</template>
							</Column>
							<Column field="total_tokens" header="Total Tokens" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_tokens ?? 0) }}
								</template>
							</Column>
							<Column header="% of Total">
								<template #body="slotProps">
									{{ getPercentageOfTotal(slotProps.data.total_tokens ?? 0, totalAgentTokens) }}%
								</template>
							</Column>
						</DataTable>
					</template>
				</Card>
			</div>

			<!-- Token Usage by User -->
			<div class="table-section">
				<Card>
					<template #title>Token Usage by User</template>
					<template #content>
						<div class="filters-row-bottom">
							<div class="search-filter">
								<span class="p-input-icon-left">
									<i class="pi pi-search" />
									<InputText 
										v-model="usernameFilter" 
										placeholder="Search usernames" 
										class="p-inputtext-sm"
									/>
								</span>
							</div>
						</div>
						<DataTable 
							:value="filteredUsers" 
							:paginator="true" 
							:rows="10" 
							class="tokens-table"
							sortField="total_tokens"
							:sortOrder="-1"
						>
							<Column field="username" header="Username" sortable></Column>
							<Column field="total_conversations" header="Total Conversations" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_conversations ?? 0) }}
								</template>
							</Column>
							<Column field="total_messages" header="Total Messages" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_messages ?? 0) }}
								</template>
							</Column>
							<Column field="total_tokens" header="Total Tokens" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_tokens ?? 0) }}
								</template>
							</Column>
							<Column header="% of Total">
								<template #body="slotProps">
									{{ getPercentageOfTotal(slotProps.data.total_tokens ?? 0, totalUserTokens) }}%
								</template>
							</Column>
						</DataTable>
					</template>
				</Card>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch, nextTick } from 'vue';
import api from '@/js/api';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

const loading = ref(true);
const error = ref<string | null>(null);
const agents = ref<any[]>([]);
const users = ref<any[]>([]);
const usernameFilter = ref<string>('');
const topUsersPieCanvas = ref<HTMLCanvasElement | null>(null);
let topUsersPieChart: Chart | null = null;
const topUsersChartData = ref<any[]>([]);

// Initialize date range to last 3 months
const endDate = ref<Date>(new Date());
const startDate = ref<Date>(new Date(new Date().setMonth(new Date().getMonth() - 3)));
const dateRangePanel = ref();
const showCustomDialog = ref(false);

const formatNumber = (num: number) => {
	if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
	if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
	return num.toString();
};

const formatNumberWithCommas = (num: number) => {
	return num.toLocaleString('en-US');
};

const getPercentageOfTotal = (value: number, total: number) => {
	if (total === 0) return '0.00';
	return ((value / total) * 100).toFixed(2);
};

// Computed properties for summary metrics
const totalAgentTokens = computed(() => {
	return agents.value.reduce((sum, agent) => sum + (agent.total_tokens ?? 0), 0);
});

const totalUserTokens = computed(() => {
	return users.value.reduce((sum, user) => sum + (user.total_tokens ?? 0), 0);
});

const totalTokens = computed(() => {
	return Math.max(totalAgentTokens.value, totalUserTokens.value);
});

const totalUsers = computed(() => {
	return users.value.length;
});

const totalAgents = computed(() => {
	return agents.value.length;
});

const averageTokensPerUser = computed(() => {
	if (totalUsers.value === 0) return 0;
	return Math.round(totalTokens.value / totalUsers.value);
});

const filteredUsers = computed(() => {
	if (!usernameFilter.value || usernameFilter.value.trim() === '') {
		return users.value;
	}
	const filterLower = usernameFilter.value.toLowerCase().trim();
	return users.value.filter(user => 
		user.username?.toLowerCase().includes(filterLower)
	);
});

const dateRangeLabel = computed(() => {
	if (!startDate.value || !endDate.value) {
		return 'Select date range';
	}

	const start = startDate.value;
	const end = endDate.value;
	const startMonth = start.toLocaleDateString('en-US', { month: 'short' });
	const endMonth = end.toLocaleDateString('en-US', { month: 'short' });
	const startYear = start.getFullYear();
	const endYear = end.getFullYear();
	const startDay = start.getDate();
	const endDay = end.getDate();

	if (startMonth === endMonth && startYear === endYear) {
		return `${startMonth} ${startDay}-${endDay}`;
	}
	if (startYear === endYear) {
		return `${startMonth} ${startDay}-${endMonth} ${endDay}`;
	}
	return `${startMonth} ${startYear}-${endMonth} ${endYear}`;
});

const toggleDateRangePanel = (event: Event) => {
	dateRangePanel.value.toggle(event);
};

const navigateDateRange = (direction: number) => {
	const currentMonth = startDate.value.getMonth();
	const currentYear = startDate.value.getFullYear();
	const newDate = new Date(currentYear, currentMonth + direction, 1);
	startDate.value = newDate;
	endDate.value = new Date(newDate.getFullYear(), newDate.getMonth() + 1, 0);
	loadData();
};

const setDateRange = (range: string) => {
	const now = new Date();
	let start: Date;
	let end: Date = new Date();

	switch (range) {
		case 'last7days':
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
			break;
		case 'last30days':
			start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
			break;
		case 'thisMonth':
			start = new Date(now.getFullYear(), now.getMonth(), 1);
			end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
			break;
		case 'lastMonth':
			start = new Date(now.getFullYear(), now.getMonth() - 1, 1);
			end = new Date(now.getFullYear(), now.getMonth(), 0);
			break;
		case 'thisQuarter':
			const quarter = Math.floor(now.getMonth() / 3);
			start = new Date(now.getFullYear(), quarter * 3, 1);
			end = new Date(now.getFullYear(), (quarter + 1) * 3, 0);
			break;
		case 'lastQuarter':
			const lastQuarter = Math.floor(now.getMonth() / 3) - 1;
			start = new Date(now.getFullYear(), lastQuarter * 3, 1);
			end = new Date(now.getFullYear(), (lastQuarter + 1) * 3, 0);
			break;
		case 'last3months':
			start = new Date(now.getFullYear(), now.getMonth() - 3, 1);
			break;
		case 'last6months':
			start = new Date(now.getFullYear(), now.getMonth() - 6, 1);
			break;
		case 'last12months':
			start = new Date(now.getFullYear(), now.getMonth() - 12, 1);
			break;
		case 'thisYear':
			start = new Date(now.getFullYear(), 0, 1);
			end = new Date(now.getFullYear(), 11, 31);
			break;
		default:
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
	}

	startDate.value = start;
	endDate.value = end;
	dateRangePanel.value.hide();
	loadData();
};

const showCustomDateRange = () => {
	dateRangePanel.value.hide();
	showCustomDialog.value = true;
};

const applyCustomDateRange = () => {
	showCustomDialog.value = false;
	loadData();
};

const formatDateRange = (range: string) => {
	const now = new Date();
	let start: Date;
	let end: Date = new Date();

	switch (range) {
		case 'last7days':
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
			break;
		case 'last30days':
			start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
			break;
		case 'thisMonth':
			start = new Date(now.getFullYear(), now.getMonth(), 1);
			end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
			break;
		case 'lastMonth':
			start = new Date(now.getFullYear(), now.getMonth() - 1, 1);
			end = new Date(now.getFullYear(), now.getMonth(), 0);
			break;
		case 'thisQuarter':
			const quarter = Math.floor(now.getMonth() / 3);
			start = new Date(now.getFullYear(), quarter * 3, 1);
			end = new Date(now.getFullYear(), (quarter + 1) * 3, 0);
			break;
		case 'lastQuarter':
			const lastQuarter = Math.floor(now.getMonth() / 3) - 1;
			start = new Date(now.getFullYear(), lastQuarter * 3, 1);
			end = new Date(now.getFullYear(), (lastQuarter + 1) * 3, 0);
			break;
		case 'last3months':
			start = new Date(now.getFullYear(), now.getMonth() - 3, 1);
			break;
		case 'last6months':
			start = new Date(now.getFullYear(), now.getMonth() - 6, 1);
			break;
		case 'last12months':
			start = new Date(now.getFullYear(), now.getMonth() - 12, 1);
			break;
		case 'thisYear':
			start = new Date(now.getFullYear(), 0, 1);
			end = new Date(now.getFullYear(), 11, 31);
			break;
		default:
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
	}

	return `${start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${end.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}`;
};

const loadData = async () => {
	loading.value = true;
	error.value = null;
	try {
		const start = startDate.value?.toISOString();
		const end = endDate.value?.toISOString();
		
		[agents.value, users.value] = await Promise.all([
			api.getAllAgentsAnalytics(start, end),
			api.getAllUsersAnalytics(start, end)
		]);
		
		// Prepare top users data for pie chart
		prepareTopUsersChartData();
		
		await nextTick();
		updateTopUsersPieChart();
	} catch (err: any) {
		error.value = err.data?.error || err.data?.message || err.message || 'Failed to load token analytics';
	} finally {
		loading.value = false;
	}
};

const prepareTopUsersChartData = () => {
	// Sort users by total_tokens descending
	const sortedUsers = [...users.value].sort((a, b) => (b.total_tokens ?? 0) - (a.total_tokens ?? 0));
	
	// Get top 9 users
	const top9 = sortedUsers.slice(0, 9);
	
	// Calculate "Other" as sum of remaining users
	const otherUsers = sortedUsers.slice(9);
	const otherTokens = otherUsers.reduce((sum, user) => sum + (user.total_tokens ?? 0), 0);
	
	// Build chart data
	const chartData: any[] = top9.map(user => ({
		label: user.username || 'Unknown',
		value: user.total_tokens ?? 0
	}));
	
	// Add "Other" category if there are more than 9 users
	if (otherUsers.length > 0) {
		chartData.push({
			label: `Other (${otherUsers.length} users)`,
			value: otherTokens
		});
	}
	
	topUsersChartData.value = chartData;
};

const updateTopUsersPieChart = () => {
	if (!topUsersPieCanvas.value) {
		return;
	}

	// Destroy existing chart
	if (topUsersPieChart) {
		topUsersPieChart.destroy();
		topUsersPieChart = null;
	}

	if (!topUsersChartData.value || topUsersChartData.value.length === 0) {
		return;
	}

	const colors = [
		'#4A90E2', '#50C878', '#FF6B6B', '#FFD93D', '#9B59B6',
		'#1ABC9C', '#E74C3C', '#F39C12', '#3498DB', '#95A5A6'
	];

	const labels = topUsersChartData.value.map(item => item.label);
	const data = topUsersChartData.value.map(item => item.value);

	topUsersPieChart = new Chart(topUsersPieCanvas.value, {
		type: 'pie',
		data: {
			labels,
			datasets: [{
				data,
				backgroundColor: colors.slice(0, data.length),
				borderColor: '#ffffff',
				borderWidth: 2
			}]
		},
		options: {
			responsive: true,
			maintainAspectRatio: false,
			plugins: {
				legend: {
					display: true,
					position: 'right',
					labels: {
						generateLabels: (chart) => {
							const data = chart.data;
							if (data.labels && data.datasets.length) {
								const total = data.datasets[0].data.reduce((sum: number, val: any) => sum + val, 0);
								return data.labels.map((label, i) => {
									const value = data.datasets[0].data[i] as number;
									const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
									return {
										text: `${label}: ${formatNumber(value)} (${percentage}%)`,
										fillStyle: colors[i % colors.length],
										hidden: false,
										index: i,
										strokeStyle: '#ffffff',
										lineWidth: 2
									};
								});
							}
							return [];
						}
					}
				},
				tooltip: {
					callbacks: {
						label: (context) => {
							const value = context.parsed;
							const total = context.dataset.data.reduce((sum: number, val: any) => sum + val, 0);
							const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
							return `${context.label}: ${formatNumberWithCommas(value)} tokens (${percentage}%)`;
						}
					}
				}
			}
		}
	});
};

watch([topUsersChartData, topUsersPieCanvas], () => {
	if (topUsersChartData.value && topUsersPieCanvas.value) {
		nextTick(() => {
			updateTopUsersPieChart();
		});
	}
}, { deep: true });

onMounted(() => {
	loadData();
});

onUnmounted(() => {
	if (topUsersPieChart) {
		topUsersPieChart.destroy();
		topUsersPieChart = null;
	}
});
</script>

<style scoped>
.analytics-tokens-page {
	padding: 2rem;
}

.filters-row {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 2rem;
	justify-content: flex-start;
}

.date-range-selector {
	display: flex;
	gap: 1rem;
	align-items: center;
}

.date-range-menu {
	min-width: 300px;
	max-height: 500px;
	overflow-y: auto;
}

.date-range-menu-header {
	display: flex;
	justify-content: space-between;
	margin-bottom: 0.5rem;
	padding-bottom: 0.5rem;
	border-bottom: 1px solid var(--surface-border);
}

.section-title {
	font-weight: 600;
	font-size: 0.75rem;
	color: var(--text-color-secondary);
	margin-bottom: 0.25rem;
	padding: 0.25rem 0;
	text-transform: uppercase;
	letter-spacing: 0.5px;
}

.date-range-section {
	margin-bottom: 0.5rem;
}

.date-range-option {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 0.5rem 0.75rem;
	cursor: pointer;
	border-radius: 4px;
	transition: background-color 0.2s;
	font-size: 0.875rem;
}

.date-range-option:hover {
	background-color: var(--surface-hover);
}

.date-preview {
	color: var(--text-color-secondary);
	font-size: 0.875rem;
}

.custom-date-range-dialog {
	display: flex;
	flex-direction: column;
	gap: 1rem;
}

.date-input-group {
	display: flex;
	flex-direction: column;
	gap: 0.5rem;
}

.date-input-group label {
	font-weight: 500;
}

.dialog-actions {
	display: flex;
	justify-content: flex-end;
	gap: 0.5rem;
	margin-top: 1rem;
}

.metrics-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
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

.chart-container {
	margin-bottom: 2rem;
}

.pie-chart-wrapper {
	position: relative;
	height: 400px;
	width: 100%;
	display: flex;
	justify-content: center;
	align-items: center;
}

.table-section {
	margin-bottom: 2rem;
}

.tokens-table {
	margin-top: 0;
}

.filters-row-bottom {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 1rem;
	justify-content: flex-end;
}

.search-filter {
	flex: 0 0 auto;
}

.loading,
.error {
	text-align: center;
	padding: 2rem;
}

.p-input-icon-left {
	position: relative;
}

.p-input-icon-left i {
	position: absolute;
	left: 0.75rem;
	top: 50%;
	margin-top: -0.5rem;
	color: var(--text-color-secondary);
}

.p-input-icon-left input {
	padding-left: 2.5rem;
}
</style>
