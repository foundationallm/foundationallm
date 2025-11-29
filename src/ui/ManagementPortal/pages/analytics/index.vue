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

			<div class="metrics-grid">
			<Card class="metric-card">
				<template #content>
					<div class="metric-value">{{ formatNumberWithCommas(overview.total_conversations ?? 0) }}</div>
					<div class="metric-label">Total Conversations</div>
				</template>
			</Card>

			<Card class="metric-card">
				<template #content>
					<div class="metric-value">{{ formatNumber(overview.total_tokens ?? 0) }}</div>
					<div class="metric-label">Total Tokens</div>
				</template>
			</Card>

			<Card class="metric-card">
				<template #content>
					<div class="metric-value">{{ overview.active_users ?? 0 }}</div>
					<div class="metric-label">Active Users</div>
				</template>
			</Card>

			<Card class="metric-card">
				<template #content>
					<div class="metric-value">{{ overview.total_agents ?? 0 }}</div>
					<div class="metric-label">Active Agents</div>
				</template>
			</Card>
			</div>

			<div v-if="chartData" class="chart-container">
				<Card>
					<template #title>Daily Usage by Agent</template>
					<template #content>
						<div class="chart-wrapper">
							<canvas ref="chartCanvas"></canvas>
						</div>
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
const overview = ref<any>(null);
const chartData = ref<any[]>([]);
const chartCanvas = ref<HTMLCanvasElement | null>(null);
let chartInstance: Chart | null = null;
const filteredAgent = ref<string | null>(null);
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
	
	// Same month and year: "Nov 23-29"
	if (startMonth === endMonth && startYear === endYear) {
		return `${startMonth} ${startDay}-${endDay}`;
	}
	// Same year, different months: "Oct 31-Nov 29"
	if (startYear === endYear) {
		return `${startMonth} ${startDay}-${endMonth} ${endDay}`;
	}
	// Different years: "Nov 2024-Oct 2025"
	return `${startMonth} ${startYear}-${endMonth} ${endYear}`;
});

const toggleDateRangePanel = (event: Event) => {
	dateRangePanel.value.toggle(event);
};

const navigateDateRange = (direction: number) => {
	// Navigate months/quarters based on current view
	const currentMonth = startDate.value.getMonth();
	const currentYear = startDate.value.getFullYear();
	const newDate = new Date(currentYear, currentMonth + direction, 1);
	startDate.value = newDate;
	endDate.value = new Date(newDate.getFullYear(), newDate.getMonth() + 1, 0);
	loadData();
};

const formatDateRange = (range: string): string => {
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

	const startMonth = start.toLocaleDateString('en-US', { month: 'short' });
	const endMonth = end.toLocaleDateString('en-US', { month: 'short' });
	const startDay = start.getDate();
	const endDay = end.getDate();
	const startYear = start.getFullYear();
	const endYear = end.getFullYear();

	if (startMonth === endMonth && startYear === endYear) {
		return `${startMonth} ${startDay}-${endDay}`;
	}
	if (startYear === endYear) {
		return `${startMonth} ${startDay}-${endMonth} ${endDay}`;
	}
	return `${startMonth} ${startYear}-${endMonth} ${endYear}`;
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

const loadData = async () => {
	loading.value = true;
	error.value = null;
	try {
		const start = startDate.value?.toISOString();
		const end = endDate.value?.toISOString();
		[overview.value, chartData.value] = await Promise.all([
			api.getAnalyticsOverview(start, end),
			api.getDailyMessageCounts(start, end)
		]);
		console.log('Analytics overview data:', overview.value);
		console.log('Chart data:', chartData.value);
		console.log('Date range:', { start, end, startDate: startDate.value, endDate: endDate.value });
		
		await nextTick();
		updateChart();
	} catch (err: any) {
		// Extract error message from API response
		// The API returns { error: "message" } in the response body
		const errorMessage = err.data?.error || err.data?.message || err.message || 'Failed to load analytics data';
		error.value = errorMessage;
		console.error('Analytics API error:', {
			message: errorMessage,
			status: err.status,
			statusText: err.statusText,
			data: err.data,
			fullError: err
		});
	} finally {
		loading.value = false;
	}
};

const updateChart = () => {
	if (!chartCanvas.value || !chartData.value || chartData.value.length === 0) {
		return;
	}

	// Destroy existing chart
	if (chartInstance) {
		chartInstance.destroy();
	}

	// Get all unique agent names
	const allAgents = new Set<string>();
	chartData.value.forEach(day => {
		Object.keys(day.agent_counts || {}).forEach(agent => allAgents.add(agent));
	});
	const agentNames = Array.from(allAgents).sort();

	// Filter agents if a filter is active
	const agentsToShow = filteredAgent.value 
		? agentNames.filter(agent => agent === filteredAgent.value)
		: agentNames;

	// Generate colors for each agent
	const colors = [
		'#4A90E2', '#50C878', '#FF6B6B', '#FFD93D', '#9B59B6',
		'#1ABC9C', '#E74C3C', '#F39C12', '#3498DB', '#E67E22',
		'#95A5A6', '#34495E', '#16A085', '#27AE60', '#2980B9'
	];

	// Prepare datasets
	const datasets = agentsToShow.map((agent, index) => {
		const originalIndex = agentNames.indexOf(agent);
		return {
			label: agent,
			data: chartData.value.map(day => day.agent_counts?.[agent] || 0),
			backgroundColor: colors[originalIndex % colors.length],
			borderColor: colors[originalIndex % colors.length],
			borderWidth: 1
		};
	});

	// Prepare labels (dates)
	const labels = chartData.value.map(day => {
		// Handle both ISO string and Date object
		const date = typeof day.date === 'string' ? new Date(day.date) : day.date;
		return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
	});

	chartInstance = new Chart(chartCanvas.value, {
		type: 'bar',
		data: {
			labels,
			datasets
		},
		options: {
			responsive: true,
			maintainAspectRatio: false,
			onClick: (event, elements) => {
				if (elements.length > 0) {
					const element = elements[0];
					const datasetIndex = element.datasetIndex;
					const clickedAgent = datasets[datasetIndex].label;
					
					// Toggle filter - if already filtered to this agent, clear filter
					if (filteredAgent.value === clickedAgent) {
						filteredAgent.value = null;
					} else {
						filteredAgent.value = clickedAgent;
					}
					updateChart();
				}
			},
			onHover: (event, elements) => {
				if (event.native) {
					(event.native.target as HTMLElement).style.cursor = elements.length > 0 ? 'pointer' : 'default';
				}
			},
			scales: {
				x: {
					stacked: true,
					title: {
						display: true,
						text: 'Date'
					}
				},
				y: {
					stacked: true,
					beginAtZero: true,
					title: {
						display: true,
						text: 'Number of Messages'
					}
				}
			},
			plugins: {
				legend: {
					display: true,
					position: 'bottom',
					onClick: (e, legendItem, legend) => {
						e.native?.stopPropagation();
						const clickedAgent = legendItem.text;
						
						// Toggle filter - if already filtered to this agent, clear filter
						if (filteredAgent.value === clickedAgent) {
							filteredAgent.value = null;
						} else {
							filteredAgent.value = clickedAgent;
						}
						updateChart();
						return false; // Prevent default legend behavior
					}
				},
				title: {
					display: false
				}
			}
		}
	});
};

watch([chartData, chartCanvas], () => {
	if (chartData.value && chartCanvas.value) {
		nextTick(() => {
			updateChart();
			// Add double-click handler once when canvas is available
			if (chartCanvas.value && !chartCanvas.value.hasAttribute('data-dblclick-handler')) {
				chartCanvas.value.setAttribute('data-dblclick-handler', 'true');
				chartCanvas.value.addEventListener('dblclick', () => {
					filteredAgent.value = null;
					updateChart();
				});
			}
		});
	}
}, { deep: true });

onMounted(() => {
	loadData();
});

onUnmounted(() => {
	if (chartInstance) {
		chartInstance.destroy();
		chartInstance = null;
	}
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
	margin-bottom: 2rem;
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

.chart-container {
	margin-top: 2rem;
}

.chart-wrapper {
	position: relative;
	height: 400px;
	width: 100%;
}

.loading,
.error {
	text-align: center;
	padding: 2rem;
}
</style>
