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

			<div v-if="chartData" class="chart-container">
				<Card>
					<template #title>Active Users per Day</template>
					<template #content>
						<div class="chart-wrapper">
							<canvas ref="chartCanvas"></canvas>
						</div>
					</template>
				</Card>
			</div>

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
				:rows="20" 
				class="users-table"
			>
				<Column field="username" header="Username" sortable>
				</Column>
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
						{{ formatNumber(slotProps.data.total_tokens ?? 0) }}
					</template>
				</Column>
			</DataTable>
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
const users = ref<any[]>([]);
const usernameFilter = ref<string>('');
const chartData = ref<any[]>([]);
const chartCanvas = ref<HTMLCanvasElement | null>(null);
let chartInstance: Chart | null = null;
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
		[users.value, chartData.value] = await Promise.all([
			api.getAllUsersAnalytics(start, end),
			api.getDailyActiveUserCounts(start, end)
		]);
		
		await nextTick();
		updateChart();
	} catch (err: any) {
		error.value = err.data?.error || err.data?.message || err.message || 'Failed to load user analytics';
	} finally {
		loading.value = false;
	}
};

const updateChart = () => {
	if (!chartCanvas.value) {
		return;
	}

	// Destroy existing chart
	if (chartInstance) {
		chartInstance.destroy();
		chartInstance = null;
	}

	if (!chartData.value || chartData.value.length === 0) {
		return;
	}

	// Prepare labels (dates)
	const labels = chartData.value.map(day => {
		const date = typeof day.date === 'string' ? new Date(day.date) : day.date;
		return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
	});

	// Prepare data (counts)
	const data = chartData.value.map(day => day.count || 0);

	chartInstance = new Chart(chartCanvas.value, {
		type: 'bar',
		data: {
			labels,
			datasets: [{
				label: 'Active Users',
				data,
				backgroundColor: '#4A90E2',
				borderColor: '#4A90E2',
				borderWidth: 1
			}]
		},
		options: {
			responsive: true,
			maintainAspectRatio: false,
			scales: {
				x: {
					title: {
						display: true,
						text: 'Date'
					}
				},
				y: {
					beginAtZero: true,
					title: {
						display: true,
						text: 'Number of Active Users'
					}
				}
			},
			plugins: {
				legend: {
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
		});
	}
}, { deep: true });

watch([startDate, endDate], () => {
	loadData();
});

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
.analytics-users-page {
	padding: 2rem;
}

.filters-row {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 2rem;
	justify-content: space-between;
}

.search-filter {
	flex: 0 0 auto;
}

.date-range-selector {
	display: flex;
	gap: 1rem;
	align-items: center;
}

.chart-container {
	margin-top: 0;
	margin-bottom: 2rem;
}

.chart-wrapper {
	position: relative;
	height: 400px;
	width: 100%;
}

.filters-row-bottom {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 2rem;
	justify-content: flex-end;
}

.users-table {
	margin-top: 1rem;
}

.loading,
.error {
	text-align: center;
	padding: 2rem;
}

/* Styles for the custom date range menu */
.date-range-menu {
	max-height: 500px;
	overflow-y: auto;
	padding: 0.5rem;
}

.date-range-menu-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 0.25rem 0.5rem;
	margin-bottom: 0.5rem;
}

.date-range-section {
	margin-bottom: 0.5rem;
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
	font-size: 0.8rem;
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

.dialog-actions {
	display: flex;
	justify-content: flex-end;
	gap: 0.5rem;
	margin-top: 1rem;
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
