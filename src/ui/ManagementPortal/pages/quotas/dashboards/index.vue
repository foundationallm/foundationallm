<template>
	<div>
		<div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px">
			<div>
				<h2 class="page-header">Quota Dashboards</h2>
				<div class="page-subheader">Monitor quota health and analyze quota configuration effectiveness.</div>
			</div>

			<div style="display: flex; align-items: center; gap: 12px">
				<div class="flex align-items-center gap-2">
					<InputSwitch v-model="autoRefresh" />
					<span>Auto-refresh</span>
				</div>
				<Button
					type="button"
					icon="pi pi-refresh"
					:loading="loading"
					@click="refreshData"
				/>
			</div>
		</div>

		<!-- Summary Cards -->
		<div class="summary-cards">
			<div class="summary-card summary-card--info">
				<div class="summary-card__icon">
					<i class="pi pi-exclamation-triangle"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ events24h }}</div>
					<div class="summary-card__label">Total Events (24h)</div>
				</div>
			</div>

			<div class="summary-card" :class="activeLockouts > 0 ? 'summary-card--danger' : 'summary-card--success'">
				<div class="summary-card__icon">
					<i class="pi pi-lock"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ activeLockouts }}</div>
					<div class="summary-card__label">Active Lockouts</div>
				</div>
			</div>

			<div class="summary-card" :class="getEvents7dClass()">
				<div class="summary-card__icon">
					<i class="pi pi-calendar"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ events7d }}</div>
					<div class="summary-card__label">Events (7d)</div>
				</div>
			</div>

			<div class="summary-card" :class="getHealthScoreClass()">
				<div class="summary-card__icon">
					<i class="pi pi-heart"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ healthScore }}</div>
					<div class="summary-card__label">Quota Health Score</div>
				</div>
			</div>
		</div>

		<!-- Time Range Filter -->
		<div class="filters-section">
			<div class="filter-item">
				<label>Time Range</label>
				<Dropdown
					v-model="selectedTimeRange"
					:options="timeRangeOptions"
					option-label="label"
					option-value="value"
					class="w-full"
					@change="onTimeRangeChange"
				/>
			</div>
			<div class="filter-item">
				<label>Quota</label>
				<Dropdown
					v-model="selectedQuota"
					:options="quotaFilterOptions"
					option-label="label"
					option-value="value"
					placeholder="All Quotas"
					class="w-full"
					@change="applyFilters"
				/>
			</div>
		</div>

		<!-- Quota Health Table -->
		<Card style="margin-bottom: 24px">
			<template #title>Quota Health</template>
			<template #content>
				<DataTable
					:value="quotaHealthData"
					striped-rows
					:sort-field="'exceeded_event_count'"
					:sort-order="-1"
					table-style="max-width: 100%"
					size="small"
					paginator
					:rows="10"
				>
					<template #empty>
						No quota health data available. Quotas may not have been hit yet.
					</template>

					<Column field="quota_name" header="Quota Name" sortable style="min-width: 150px">
						<template #body="{ data }">
							<a href="#" class="quota-name-link" @click.prevent="viewQuotaDetails(data.quota_name)">
								{{ data.quota_name }}
							</a>
						</template>
					</Column>
					<Column field="quota_context" header="Context" sortable style="min-width: 180px">
						<template #body="{ data }">
							<code class="context-code">{{ data.quota_context }}</code>
						</template>
					</Column>
					<Column field="exceeded_event_count" header="# of Quota Exceeded events" sortable style="min-width: 200px" />
					<Column field="health_status" header="Health Status" sortable style="min-width: 120px">
						<template #body="{ data }">
							<Tag :severity="getHealthStatusSeverity(data.health_status)">
								{{ data.health_status }}
							</Tag>
						</template>
					</Column>
				</DataTable>
			</template>
		</Card>

		<!-- Recent Events Table -->
		<Card>
			<template #title>Recent Events</template>
			<template #content>
				<DataTable
					:value="recentEvents"
					striped-rows
					:sort-field="'timestamp'"
					:sort-order="-1"
					table-style="max-width: 100%"
					size="small"
					paginator
					:rows="20"
				>
					<template #empty>
						No quota events found for the selected time range.
					</template>

					<Column field="timestamp" header="Timestamp" sortable style="min-width: 180px">
						<template #body="{ data }">
							{{ formatRelativeTime(data.timestamp) }}
						</template>
					</Column>
					<Column field="quota_name" header="Quota Name" sortable style="min-width: 150px" />
					<Column field="quota_context" header="Context" sortable style="min-width: 180px">
						<template #body="{ data }">
							<span :title="data.quota_context">
								{{ formatQuotaContext(data.quota_context) }}
							</span>
						</template>
					</Column>
					<Column field="partition_id" header="Partition" sortable style="min-width: 140px">
						<template #body="{ data }">
							<span :title="data.partition_id || 'Global'" class="partition-id">
								{{ formatPartition(data.partition_id) }}
							</span>
						</template>
					</Column>
					<Column field="event_type" header="Event Type" sortable style="min-width: 150px">
						<template #body="{ data }">
							<Tag :severity="data.event_type === 'quota-exceeded' ? 'danger' : 'info'">
								{{ data.event_type === 'quota-exceeded' ? 'Quota Exceeded' : 'Lockout Expired' }}
							</Tag>
						</template>
					</Column>
					<Column field="count_at_event" header="Count" sortable style="min-width: 80px">
						<template #body="{ data }">
							{{ data.count_at_event }} / {{ data.limit }}
						</template>
					</Column>
				</DataTable>
			</template>
		</Card>

		<!-- Last Updated -->
		<div class="last-updated" v-if="lastUpdated">
			Last updated: {{ lastUpdated.toLocaleTimeString() }}
			<span v-if="autoRefresh" class="auto-refresh-indicator">
				(auto-refreshing every 30s)
			</span>
		</div>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { QuotaEventDocument, QuotaEventSummary, QuotaEventFilter } from '@/js/types';

export default {
	name: 'QuotaDashboards',

	data() {
		return {
			events: [] as QuotaEventDocument[],
			eventSummary: [] as QuotaEventSummary[],
			loading: false as boolean,
			autoRefresh: true as boolean,
			refreshInterval: null as number | null,
			lastUpdated: null as Date | null,

			selectedTimeRange: '24h' as string,
			selectedQuota: '' as string,

			timeRangeOptions: [
				{ label: 'Last 24 hours', value: '24h' },
				{ label: 'Last 7 days', value: '7d' },
				{ label: 'Last 30 days', value: '30d' },
			],
		};
	},

	computed: {
		quotaFilterOptions(): { label: string; value: string }[] {
			const quotaNames = [...new Set(this.eventSummary.map(s => s.quota_name))];
			return [
				{ label: 'All Quotas', value: '' },
				...quotaNames.map(name => ({ label: name, value: name })),
			];
		},

		events24h(): number {
			const now = new Date();
			const yesterday = new Date(now.getTime() - 24 * 60 * 60 * 1000);
			return this.events.filter(e => {
				const eventTime = new Date(e.timestamp);
				return eventTime >= yesterday && e.event_type === 'quota-exceeded';
			}).length;
		},

		activeLockouts(): number {
			// This would need to come from current metrics, for now return 0
			// In a real implementation, you'd query current metrics to get active lockouts
			return 0;
		},

		events7d(): number {
			const now = new Date();
			const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
			return this.events.filter(e => {
				const eventTime = new Date(e.timestamp);
				return eventTime >= weekAgo && e.event_type === 'quota-exceeded';
			}).length;
		},

		healthScore(): number {
			if (this.eventSummary.length === 0) return 100;
			const totalEvents = this.eventSummary.reduce((sum, s) => sum + s.exceeded_event_count, 0);
			const quotaCount = this.eventSummary.length;
			const score = 100 - Math.min(100, (totalEvents / quotaCount) * 10);
			return Math.max(0, Math.round(score));
		},

		quotaHealthData(): any[] {
			let summary = this.eventSummary;
			if (this.selectedQuota) {
				summary = summary.filter(s => s.quota_name === this.selectedQuota);
			}

			return summary.map(s => ({
				quota_name: s.quota_name,
				quota_context: s.quota_context,
				exceeded_event_count: s.exceeded_event_count,
				unique_partitions_affected: s.unique_partitions_affected,
				health_status: this.getHealthStatus(s.exceeded_event_count),
			}));
		},

		recentEvents(): QuotaEventDocument[] {
			let events = this.events;
			if (this.selectedQuota) {
				events = events.filter(e => e.quota_name === this.selectedQuota);
			}
			return events.slice(0, 100); // Limit to 100 most recent
		},
	},

	watch: {
		autoRefresh(newVal: boolean) {
			if (newVal) {
				this.startAutoRefresh();
			} else {
				this.stopAutoRefresh();
			}
		},
	},

	async created() {
		await this.refreshData();
		if (this.autoRefresh) {
			this.startAutoRefresh();
		}
	},

	beforeUnmount() {
		this.stopAutoRefresh();
	},

	methods: {
		async refreshData() {
			this.loading = true;
			try {
				const timeRange = this.getTimeRange();
				const filter: QuotaEventFilter = {
					start_time: timeRange.start.toISOString(),
					end_time: timeRange.end.toISOString(),
				};

				// Fetch events and summary in parallel
				const [events, summary] = await Promise.all([
					api.getQuotaEvents(filter),
					api.getQuotaEventSummary({
						start_time: timeRange.start.toISOString(),
						end_time: timeRange.end.toISOString(),
					}),
				]);

				this.events = events;
				this.eventSummary = summary;
				this.lastUpdated = new Date();
			} catch (error) {
				if (!this.autoRefresh) {
					this.$toast.add({
						severity: 'error',
						detail: error?.response?._data || error,
						life: 5000,
					});
				}
				console.error('Failed to fetch quota events:', error);
			}
			this.loading = false;
		},

		getTimeRange(): { start: Date; end: Date } {
			const end = new Date();
			let start = new Date();

			switch (this.selectedTimeRange) {
				case '24h':
					start = new Date(end.getTime() - 24 * 60 * 60 * 1000);
					break;
				case '7d':
					start = new Date(end.getTime() - 7 * 24 * 60 * 60 * 1000);
					break;
				case '30d':
					start = new Date(end.getTime() - 30 * 24 * 60 * 60 * 1000);
					break;
			}

			return { start, end };
		},

		onTimeRangeChange() {
			this.refreshData();
		},

		applyFilters() {
			// Filters are computed, no action needed
		},

		startAutoRefresh() {
			this.refreshInterval = window.setInterval(() => {
				this.refreshData();
			}, 30000); // 30 seconds
		},

		stopAutoRefresh() {
			if (this.refreshInterval) {
				clearInterval(this.refreshInterval);
				this.refreshInterval = null;
			}
		},

		getHealthStatus(eventCount: number): string {
			// Get the number of days based on selected time range
			const daysMap: Record<string, number> = {
				'24h': 1,
				'7d': 7,
				'30d': 30,
			};
			const days = daysMap[this.selectedTimeRange] || 1;
			
			// Calculate average events per day
			const eventsPerDay = eventCount / days;
			
			// Thresholds based on daily average:
			// Good: ≤2 events per day
			// Fair: ≤10 events per day
			// Poor: >10 events per day
			if (eventsPerDay <= 2) return 'Good';
			if (eventsPerDay <= 10) return 'Fair';
			return 'Poor';
		},

		getHealthStatusSeverity(status: string): string {
			if (status === 'Good') return 'success';
			if (status === 'Fair') return 'warning';
			return 'danger';
		},

		getEvents7dClass(): string {
			if (this.events7d > 50) return 'summary-card--warning';
			return 'summary-card--success';
		},

		getHealthScoreClass(): string {
			if (this.healthScore >= 80) return 'summary-card--success';
			if (this.healthScore >= 50) return 'summary-card--warning';
			return 'summary-card--danger';
		},

		truncatePartition(partition: string): string {
			if (partition.length <= 25) return partition;
			return partition.substring(0, 22) + '...';
		},

		formatPartition(partition: string): string {
			if (!partition || partition.trim() === '' || partition.toLowerCase() === 'global') {
				return 'Global';
			}
			return this.truncatePartition(partition);
		},

		formatQuotaContext(context: string): string {
			if (!context) return 'Unknown';
			
			// Parse context like "GatewayAPI:Completions" or "Agent:agent-name"
			const parts = context.split(':');
			if (parts.length >= 2) {
				const type = parts[0];
				const name = parts.slice(1).join(':'); // Handle names with colons
				
				if (type === 'Agent') {
					return `Agent: ${name}`;
				} else if (type.endsWith('API')) {
					return `${type}: ${name}`;
				}
				return context;
			}
			return context;
		},

		formatRelativeTime(timestamp: string): string {
			const eventTime = new Date(timestamp);
			const now = new Date();
			const diffMs = now.getTime() - eventTime.getTime();
			const diffMins = Math.floor(diffMs / 60000);
			const diffHours = Math.floor(diffMs / 3600000);
			const diffDays = Math.floor(diffMs / 86400000);

			if (diffMins < 1) return 'Just now';
			if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
			if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
			if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
			return eventTime.toLocaleString();
		},

		viewQuotaDetails(quotaName: string) {
			this.$router.push(`/quotas/edit/${quotaName}`);
		},
	},
};
</script>

<style lang="scss" scoped>
.summary-cards {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
	gap: 20px;
	margin-bottom: 24px;
}

.summary-card {
	background-color: var(--primary-color);
	border-radius: 0;
	padding: 20px;
	color: var(--primary-text);
	display: flex;
	align-items: center;
	gap: 16px;
	box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
	transition: transform 0.2s, box-shadow 0.2s;
	border-left: 4px solid var(--primary-button-bg);

	&:hover {
		transform: translateY(-2px);
		box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
	}

	&--success {
		border-left-color: #28a745;
	}

	&--warning {
		border-left-color: #ffc107;
	}

	&--info {
		border-left-color: #17a2b8;
	}

	&--danger {
		border-left-color: #dc3545;
	}

	&__icon {
		font-size: 2rem;
		opacity: 0.9;
	}

	&__content {
		display: flex;
		flex-direction: column;
	}

	&__value {
		font-size: 2rem;
		font-weight: 700;
		line-height: 1;
	}

	&__label {
		font-size: 0.9rem;
		opacity: 0.9;
		margin-top: 4px;
	}
}

.filters-section {
	display: flex;
	gap: 16px;
	margin-bottom: 20px;
	flex-wrap: wrap;
}

.filter-item {
	display: flex;
	flex-direction: column;
	gap: 6px;
	min-width: 200px;

	label {
		font-weight: 500;
		font-size: 0.9rem;
	}
}

.context-code {
	background-color: #f5f5f5;
	padding: 2px 6px;
	border-radius: 0;
	font-family: monospace;
	font-size: 0.85rem;
}

.quota-name-link {
	color: var(--primary-button-bg);
	text-decoration: none;
	font-weight: 500;
	
	&:hover {
		text-decoration: underline;
	}
}

.partition-id {
	font-size: 0.9rem;
}

.last-updated {
	margin-top: 16px;
	font-size: 0.85rem;
	color: #666;
	text-align: right;
}

.auto-refresh-indicator {
	color: #4caf50;
	margin-left: 8px;
}
</style>
