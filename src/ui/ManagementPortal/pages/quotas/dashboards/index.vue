<template>
	<div>
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Quota Dashboards</h2>
				<div class="page-subheader">Monitor real-time quota usage and enforcement status.</div>
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
					@click="refreshMetrics"
				/>
			</div>
		</div>

		<!-- Summary Cards -->
		<div class="summary-cards">
			<div class="summary-card">
				<div class="summary-card__icon">
					<i class="pi pi-chart-bar"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ totalQuotas }}</div>
					<div class="summary-card__label">Active Quotas</div>
				</div>
			</div>

			<div class="summary-card summary-card--warning">
				<div class="summary-card__icon">
					<i class="pi pi-lock"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ lockedOutCount }}</div>
					<div class="summary-card__label">In Lockout</div>
				</div>
			</div>

			<div class="summary-card summary-card--info">
				<div class="summary-card__icon">
					<i class="pi pi-percentage"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ averageUtilization }}%</div>
					<div class="summary-card__label">Avg Utilization</div>
				</div>
			</div>

			<div class="summary-card summary-card--danger">
				<div class="summary-card__icon">
					<i class="pi pi-exclamation-triangle"></i>
				</div>
				<div class="summary-card__content">
					<div class="summary-card__value">{{ highUtilizationCount }}</div>
					<div class="summary-card__label">High Usage (&gt;80%)</div>
				</div>
			</div>
		</div>

		<!-- Filters -->
		<div class="filters-section">
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
			<div class="filter-item">
				<label>Status</label>
				<Dropdown
					v-model="selectedStatus"
					:options="statusFilterOptions"
					option-label="label"
					option-value="value"
					placeholder="All Status"
					class="w-full"
					@change="applyFilters"
				/>
			</div>
		</div>

		<!-- Loading overlay -->
		<div :class="{ 'grid--loading': loading }">
			<template v-if="loading && metrics.length === 0">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading metrics">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Metrics Table -->
			<DataTable
				:value="filteredMetrics"
				striped-rows
				scrollable
				:sort-field="'quota_name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
				paginator
				:rows="20"
				:rowsPerPageOptions="[10, 20, 50, 100]"
			>
				<template #empty>
					No quota metrics found. Make sure quotas are configured and the API is running.
				</template>

				<!-- Quota Name -->
				<Column
					field="quota_name"
					header="Quota Name"
					sortable
					style="min-width: 150px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Context -->
				<Column
					field="quota_context"
					header="Context"
					sortable
					style="min-width: 180px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<code class="context-code">{{ data.quota_context }}</code>
					</template>
				</Column>

				<!-- Partition -->
				<Column
					field="partition_id"
					header="Partition"
					sortable
					style="min-width: 140px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<span :title="data.partition_id" class="partition-id">
							{{ truncatePartition(data.partition_id) }}
						</span>
					</template>
				</Column>

				<!-- Usage -->
				<Column
					field="current_count"
					header="Usage"
					sortable
					style="min-width: 100px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						{{ data.current_count }} / {{ data.limit }}
					</template>
				</Column>

				<!-- Utilization -->
				<Column
					field="utilization_percentage"
					header="Utilization"
					sortable
					style="min-width: 180px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<div class="utilization-bar">
							<div
								class="utilization-bar__fill"
								:class="getUtilizationClass(data.utilization_percentage)"
								:style="{ width: Math.min(data.utilization_percentage, 100) + '%' }"
							></div>
						</div>
						<span class="utilization-text">{{ data.utilization_percentage.toFixed(1) }}%</span>
					</template>
				</Column>

				<!-- Status -->
				<Column
					field="lockout_active"
					header="Status"
					sortable
					style="min-width: 120px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<Tag v-if="data.lockout_active" severity="danger">
							<i class="pi pi-lock mr-1"></i>
							Lockout
						</Tag>
						<Tag v-else severity="success">
							<i class="pi pi-check mr-1"></i>
							Active
						</Tag>
					</template>
				</Column>

				<!-- Lockout Remaining -->
				<Column
					field="lockout_remaining_seconds"
					header="Lockout Remaining"
					sortable
					style="min-width: 140px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<span v-if="data.lockout_active" class="lockout-countdown">
							{{ formatTime(data.lockout_remaining_seconds) }}
						</span>
						<span v-else class="text-muted">—</span>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Last Updated -->
		<div class="last-updated" v-if="lastUpdated">
			Last updated: {{ lastUpdated.toLocaleTimeString() }}
			<span v-if="autoRefresh" class="auto-refresh-indicator">
				(auto-refreshing every 5s)
			</span>
		</div>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { QuotaUsageMetrics } from '@/js/types';

export default {
	name: 'QuotaDashboards',

	data() {
		return {
			metrics: [] as QuotaUsageMetrics[],
			loading: false as boolean,
			loadingStatusText: 'Loading metrics...' as string,
			autoRefresh: true as boolean,
			refreshInterval: null as number | null,
			lastUpdated: null as Date | null,

			selectedQuota: '' as string,
			selectedStatus: '' as string,

			statusFilterOptions: [
				{ label: 'All Status', value: '' },
				{ label: 'Active', value: 'active' },
				{ label: 'Lockout', value: 'lockout' },
			],
		};
	},

	computed: {
		quotaFilterOptions(): { label: string; value: string }[] {
			const quotaNames = [...new Set(this.metrics.map(m => m.quota_name))];
			return [
				{ label: 'All Quotas', value: '' },
				...quotaNames.map(name => ({ label: name, value: name })),
			];
		},

		filteredMetrics(): QuotaUsageMetrics[] {
			let result = this.metrics;

			if (this.selectedQuota) {
				result = result.filter(m => m.quota_name === this.selectedQuota);
			}

			if (this.selectedStatus === 'active') {
				result = result.filter(m => !m.lockout_active);
			} else if (this.selectedStatus === 'lockout') {
				result = result.filter(m => m.lockout_active);
			}

			return result;
		},

		totalQuotas(): number {
			return [...new Set(this.metrics.map(m => m.quota_name))].length;
		},

		lockedOutCount(): number {
			return this.metrics.filter(m => m.lockout_active).length;
		},

		averageUtilization(): string {
			if (this.metrics.length === 0) return '0';
			const avg = this.metrics.reduce((sum, m) => sum + m.utilization_percentage, 0) / this.metrics.length;
			return avg.toFixed(1);
		},

		highUtilizationCount(): number {
			return this.metrics.filter(m => m.utilization_percentage > 80).length;
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
		await this.refreshMetrics();
		if (this.autoRefresh) {
			this.startAutoRefresh();
		}
	},

	beforeUnmount() {
		this.stopAutoRefresh();
	},

	methods: {
		async refreshMetrics() {
			this.loading = true;
			try {
				this.metrics = await api.getQuotaMetrics();
				this.lastUpdated = new Date();
			} catch (error) {
				// Only show error toast if not auto-refreshing to avoid spam
				if (!this.autoRefresh) {
					this.$toast.add({
						severity: 'error',
						detail: error?.response?._data || error,
						life: 5000,
					});
				}
				console.error('Failed to fetch quota metrics:', error);
			}
			this.loading = false;
		},

		applyFilters() {
			// Filters are computed, no action needed
		},

		startAutoRefresh() {
			this.refreshInterval = window.setInterval(() => {
				this.refreshMetrics();
			}, 5000);
		},

		stopAutoRefresh() {
			if (this.refreshInterval) {
				clearInterval(this.refreshInterval);
				this.refreshInterval = null;
			}
		},

		truncatePartition(partition: string): string {
			if (partition.length <= 25) return partition;
			return partition.substring(0, 22) + '...';
		},

		formatTime(seconds: number): string {
			if (seconds <= 0) return '0s';
			const mins = Math.floor(seconds / 60);
			const secs = seconds % 60;
			if (mins > 0) {
				return `${mins}m ${secs}s`;
			}
			return `${secs}s`;
		},

		getUtilizationClass(percentage: number): string {
			if (percentage >= 90) return 'utilization-bar__fill--danger';
			if (percentage >= 70) return 'utilization-bar__fill--warning';
			return 'utilization-bar__fill--success';
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
	margin-top: 16px;
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

	&--warning {
		border-left-color: #dc3545;
	}

	&--info {
		border-left-color: #17a2b8;
	}

	&--danger {
		border-left-color: #ffc107;
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

.grid--loading {
	pointer-events: none;
	opacity: 0.7;
}

.grid__loading-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 16px;
	z-index: 10;
	background-color: rgba(255, 255, 255, 0.9);
	pointer-events: none;
}

.context-code {
	background-color: #f5f5f5;
	padding: 2px 6px;
	border-radius: 0;
	font-family: monospace;
	font-size: 0.85rem;
}

.partition-id {
	font-size: 0.9rem;
}

.utilization-bar {
	display: inline-block;
	width: 100px;
	height: 8px;
	background-color: #e0e0e0;
	border-radius: 0;
	overflow: hidden;
	margin-right: 8px;

	&__fill {
		height: 100%;
		border-radius: 0;
		transition: width 0.3s ease;

		&--success {
			background-color: #28a745;
		}

		&--warning {
			background-color: #ffc107;
		}

		&--danger {
			background-color: #dc3545;
		}
	}
}

.utilization-text {
	font-weight: 600;
	font-size: 0.9rem;
}

.lockout-countdown {
	color: #f44336;
	font-weight: 600;
	font-family: monospace;
}

.text-muted {
	color: #999;
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

.mr-1 {
	margin-right: 4px;
}
</style>
