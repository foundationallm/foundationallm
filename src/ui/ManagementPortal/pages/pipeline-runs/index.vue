<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Data Pipeline runs</h2>
				<div class="page-subheader"></div>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading pipeline runs">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:globalFilterFields="['created_on']"
				v-model:filters="filters"
				filterDisplay="menu"
				:value="filteredPipelineRuns"
				striped-rows
				scrollable
				:sort-field="'created_on'"
				:sort-order="-1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="filters">
						<div class="filter-group">
							<label for="pipelineFilter">Item</label>
							<Dropdown
								id="pipelineFilter"
								v-model="selectedPipelineName"
								:options="pipelineNames"
								option-label="label"
								option-value="value"
								placeholder="Select"
								@change="handleItemFilterChange"
							/>
						</div>

						<div class="filter-group">
							<label for="statusFilter">Status</label>
							<Dropdown
								id="statusFilter"
								v-model="statusFilter"
								:options="statusOptions"
								option-label="label"
								option-value="value"
								@change="handleStatusFilterChange"
							/>
						</div>

						<div class="filter-group">
							<label for="successFilter">Success</label>
							<Dropdown
								id="successFilter"
								v-model="successFilter"
								:options="successOptions"
								option-label="label"
								option-value="value"
								@change="handleSuccessFilterChange"
							/>
						</div>

						<div class="filter-group">
							<label for="timeFilter">Start time</label>
							<Dropdown
								id="timeFilter"
								v-model="timeFilter"
								:options="timeOptions"
								option-label="label"
								option-value="value"
								@change="handleTimeFilterChange"
							/>
						</div>

						<div class="filter-group">
							<label for="startFrom">Start time from</label>
							<Calendar
								id="startFrom"
								v-model="startFrom"
								show-icon
								show-button-bar
								show-time
								hour-format="24"
								date-format="yy-mm-dd"
								class="datetime-input"
								:disabled="timeFilter !== 'custom'"
								@update:model-value="handleTimeFilterChange"
							/>
						</div>

						<div class="filter-group">
							<label for="startTo">Start time to</label>
							<Calendar
								id="startTo"
								v-model="startTo"
								show-icon
								show-button-bar
								show-time
								hour-format="24"
								date-format="yy-mm-dd"
								class="datetime-input"
								:disabled="timeFilter !== 'custom'"
								@update:model-value="handleTimeFilterChange"
							/>
						</div>

						<div class="filter-actions">
							<Button
								type="button"
								label="Clear Filters"
								@click="clearFilters"
							/>
						</div>
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="getPipelineRuns"
						/>
					</div>
				</template>

				<template #empty>
					<div role="alert" aria-live="polite">No data pipeline runs found.</div>
				</template>

				<template #loading>Loading data pipeline runs. Please wait.</template>

				<Column
					header="Item"
					style="min-width: 160px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="slotProps">
						{{ getPipelineNameFromObjectId(slotProps.data.object_id) }}
					</template>
				</Column>

                <!-- Start Time -->
				<Column
					field="created_on"
					header="Start Time"
					sortable
					style="min-width: 50px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}">
                <template #body="slotProps">
                    {{ getFormattedDate(slotProps.data.created_on) }}
                </template>
				</Column>

                <!-- Completed -->
				<Column
					field="completed"
					header="Status"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="slotProps">
						<span
							class="status-pill"
							:class="slotProps.data.completed ? 'status-pill--completed' : 'status-pill--inprogress'"
						>
							{{ slotProps.data.completed ? 'Completed' : 'In progress' }}
						</span>
					</template>
				</Column>

				<!-- Successful -->
				<Column
					field="successful"
					header="Success"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="slotProps">
						<span
							class="success-pill"
							:class="slotProps.data.successful ? 'success-pill--true' : 'success-pill--false'"
						>
							{{ slotProps.data.successful ? 'Success' : 'Failure' }}
						</span>
					</template>
				</Column>

                <!-- Active Stages 
				<Column
					field="active_stages"
					header="Active Stages"
					style="min-width: 50px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}">
                <template #body="slotProps">
                    {{ slotProps.data.active_stages.join(', ') }}
                </template>
				</Column>
				-->
                <!-- Completed Stages -->
				<Column
					field="completed_stages"
					header="Completed Stages"
					style="min-width: 50px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}">
                <template #body="slotProps">
                    {{ slotProps.data.completed_stages.join(', ') }}
                </template>
				</Column>

                <!-- Failed Stages -->
				<Column
					field="failed_stages"
					header="Failed Stages"
					style="min-width: 50px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}">
                <template #body="slotProps">
                    {{ slotProps.data.failed_stages.join(', ') }}
                </template>
				</Column>

                <!-- Parameters -->
				<Column
					header="Parameters"
					header-style="width:6rem"
					style="text-align: center"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						headerContent: { style: { justifyContent: 'center' } },
					}"
				>
					<template #body="{ data }">
						<Button
							:aria-label="`Parameters`"
							@click="openParameters(data)"
						>
							<i class="pi pi-info-circle" style="font-size: 1.2rem" aria-hidden="true"></i>
						</Button>
					</template>
				</Column>

			</DataTable>
            <div class="flex justify-content-center mt-3" v-if="continuationToken">
                <Button label="Load More" icon="pi pi-refresh" @click="getPipelineRuns(true)" :loading="loading" />
            </div>
		</div>

        <!-- Trigger Pipeline Modal -->
		<Dialog 
            :visible="showParametersDialog" 
            modal
            closable
            :header="`Data Pipeline Run`"
            @update:visible="closeParametersDialog"
            >
                <label style="font-weight: bold;">Stage Metrics</label>
                <DataTable :value="stage_metrics_formatted_data">
                    <Column field="stage" header="Stage"></Column>
                    <Column field="work_items_count" header="Work Items"></Column>
                    <Column field="completed_work_items_count" header="Completed"></Column>
                    <Column field="successful_work_items_count" header="Successful"></Column>
                    <Column field="start_timestamp" header="Start Time"></Column>
                    <Column field="last_update_timestamp" header="End Time"></Column>
                    <Column field="duration" header="Duration (mm:ss.ms)"></Column>
                </DataTable>
                <br>
                <label style="font-weight: bold;">Identifier</label>
				<div style="margin-bottom: 1rem">
					<InputText
                        :readonly="true"
                        type="text"
						v-model="selectedRun.name"
						class="w-full"
					/>
				</div>

                <label style="font-weight: bold;">Canonical Id</label>
                <div style="margin-bottom: 1rem">
                    <InputText
                        :readonly="true"
                        type="text"
                        v-model="selectedRun.canonical_run_id"
                        class="w-full"
                    />
                </div>

                <label style="font-weight: bold;">Processor</label>
				<div style="margin-bottom: 1rem">
					<InputText
                        :readonly="true"
                        type="text"
						v-model="selectedRun.processor"
						class="w-full"
					/>
				</div>

                <label style="font-weight: bold;">Parameters</label>
				<div v-for="(value, key) in selectedRun.trigger_parameter_values" :key="key" class="form-group">
					<div style="margin-bottom: 1rem">
						<label :for="key">{{ key }}</label>
						<InputText
                            :readonly="true"
							type="text"
							:id="key"
							v-model="selectedRun.trigger_parameter_values[key]"
							class="w-full"
						/>
					</div>
				</div>

				<div>
					<Button
						class="sidebar-dialog__button"
						label="Close"
						text
						@click="closeParametersDialog"
					/>
				</div>
		</Dialog>

	</main>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
	name: 'DataPipelineRuns',

	data() {
		return {
			pipelineRuns: [] as any[],
			pipelineNames: [],
			selectedRun: null,
			stage_metrics_formatted_data: [],
			// When null, no item is selected and the dropdown shows the placeholder ("Select").
			selectedPipelineName: null as string | null,
			showParametersDialog: false as boolean,
			showAll: true as boolean,
			completed: true as boolean,
			successful: true as boolean,
			statusFilter: 'all' as 'all' | 'completed' | 'in-progress',
			successFilter: 'all' as 'all' | 'true' | 'false',
			timeFilter: 'all' as 'all' | 'last-1h' | 'last-24h' | 'last-7d' | 'last-30d' | 'custom',
			startFrom: null as Date | null,
			startTo: null as Date | null,
			statusOptions: [
				{ label: 'All', value: 'all' },
				{ label: 'Completed', value: 'completed' },
				{ label: 'In progress', value: 'in-progress' },
			],
			successOptions: [
				{ label: 'All', value: 'all' },
				{ label: 'Success', value: 'true' },
				{ label: 'Failure', value: 'false' },
			],
			timeOptions: [
				{ label: 'All time', value: 'all' },
				{ label: 'Last 1 hour', value: 'last-1h' },
				{ label: 'Last 24 hours', value: 'last-24h' },
				{ label: 'Last 7 days', value: 'last-7d' },
				{ label: 'Last 30 days', value: 'last-30d' },
				{ label: 'Custom range', value: 'custom' },
			],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			continuationToken: null as string | null,
			filters: {
				global: { value: null, matchMode: 'contains' },
			},
		};
	},

	computed: {
		filteredPipelineRuns(): any[] {
			let runs = this.pipelineRuns || [];

			if (this.statusFilter === 'completed') {
				runs = runs.filter((run: any) => run.completed === true);
			} else if (this.statusFilter === 'in-progress') {
				runs = runs.filter((run: any) => run.completed === false);
			}

			if (this.successFilter === 'true') {
				runs = runs.filter((run: any) => run.successful === true);
			} else if (this.successFilter === 'false') {
				runs = runs.filter((run: any) => run.successful === false);
			}

			const { from, to } = this.getTimeRange();

			if (from || to) {
				runs = runs.filter((run: any) => {
					const runDate = new Date(run.created_on);
					if (from && runDate < from) {
						return false;
					}
					if (to && runDate > to) {
						return false;
					}
					return true;
				});
			}

			return runs;
		},
	},

	async created() {
		await this.getPipelines();
	},

	beforeUnmount() {
		// Clear filters when leaving the component
		this.filters.global.value = null;
	},

	methods: {
        async getPipelines() {
			this.loading = true;
			try {
				const pipelines = await api.getPipelines();
				this.pipelineNames = [
					{ label: 'All', value: 'all' },
					...pipelines.map((pipeline: any) => ({
						label: pipeline.resource.name,
						value: pipeline.resource.name,
					})),
				];
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async getPipelineRuns(append = false) {
			this.loading = true;
			try {
                if (!append) {
                    this.continuationToken = null;
                }
				const isAll = !this.selectedPipelineName || this.selectedPipelineName === 'all';
				const mainResourceId = isAll ? 'all' : this.selectedPipelineName;

				const { from, to } = this.getTimeRange();

                const payload = {
                    data_pipeline_name_filter: isAll ? null : this.selectedPipelineName,
                    completed: this.showAll ? null : this.completed,
                    successful: this.showAll ? null : this.successful,
                    continuation_token: this.continuationToken,
					start_time: from ? from.toISOString() : null,
					end_time: to ? to.toISOString() : null,
			    };

                const response = await api.getPipelineRuns(mainResourceId || 'all', payload);
                
                if (append) {
                    this.pipelineRuns = [...this.pipelineRuns, ...response.resource.resources];
                } else {
                    this.pipelineRuns = response.resource.resources;
                }
                
                this.continuationToken = response.resource.continuation_token;

			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

        getFormattedDate(date) {
            return date.toString().replace('T', ' ').replace('Z', '').slice(0, 19);
        },

        formatDuration(ms) {
            if (ms === null || ms === undefined) return '-'
            const totalMilliseconds = Number(ms)
            const hours = Math.floor(totalMilliseconds / 3600000)
            const minutes = Math.floor((totalMilliseconds % 3600000) / 60000)
            const seconds = Math.floor((totalMilliseconds % 60000) / 1000)
            const milliseconds = totalMilliseconds % 1000
            if (hours > 0) {
                return `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(seconds)}.${String(milliseconds).padStart(3, '0')}`
            }
            return `${this.pad(minutes)}:${this.pad(seconds)}.${String(milliseconds).padStart(3, '0')}`
        },

        pad(number, digits = 2) {
            return String(number).padStart(digits, '0')
        },

		getPipelineNameFromObjectId(objectId: string) {
			if (!objectId) {
				return '-';
			}

			const pipelinesMarker = '/dataPipelines/';
			const runsMarker = '/dataPipelineRuns';

			const startIndex = objectId.indexOf(pipelinesMarker);
			if (startIndex === -1) {
				return '-';
			}

			const afterPipelines = objectId.substring(startIndex + pipelinesMarker.length);
			const endIndex = afterPipelines.indexOf(runsMarker);

			const name =
				endIndex === -1 ? afterPipelines : afterPipelines.substring(0, endIndex);

			return name || '-';
		},

		getTimeRange() {
			const now = new Date();
			let from: Date | null = null;
			let to: Date | null = null;

			switch (this.timeFilter) {
				case 'last-1h':
					from = new Date(now.getTime() - 1 * 60 * 60 * 1000);
					to = now;
					break;
				case 'last-24h':
					from = new Date(now.getTime() - 24 * 60 * 60 * 1000);
					to = now;
					break;
				case 'last-7d':
					from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
					to = now;
					break;
				case 'last-30d':
					from = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
					to = now;
					break;
				case 'custom':
					from = this.startFrom ? new Date(this.startFrom) : null;
					to = this.startTo ? new Date(this.startTo) : null;
					break;
				case 'all':
				default:
					from = null;
					to = null;
					break;
			}

			return { from, to };
		},

		syncBooleanFiltersFromDropdowns() {
			this.showAll = this.statusFilter === 'all' && this.successFilter === 'all';

			if (this.statusFilter === 'completed') {
				this.completed = true;
			} else if (this.statusFilter === 'in-progress') {
				this.completed = false;
			}

			if (this.successFilter === 'true') {
				this.successful = true;
			} else if (this.successFilter === 'false') {
				this.successful = false;
			}
		},

		handleItemFilterChange() {
			this.getPipelineRuns(false);
		},

		handleStatusFilterChange() {
			this.syncBooleanFiltersFromDropdowns();
			this.getPipelineRuns(false);
		},

		handleSuccessFilterChange() {
			this.syncBooleanFiltersFromDropdowns();
			this.getPipelineRuns(false);
		},

		handleTimeFilterChange() {
			if (this.timeFilter !== 'custom') {
				this.startFrom = '';
				this.startTo = '';
				this.getPipelineRuns(false);
				return;
			}
			if (this.startFrom || this.startTo) {
				this.getPipelineRuns(false);
			}
		},

		clearFilters() {
			this.statusFilter = 'all';
			this.successFilter = 'all';
			this.timeFilter = 'all';
			this.startFrom = null;
			this.startTo = null;
			this.selectedPipelineName = null;
			this.showAll = true;
			this.completed = true;
			this.successful = true;
			this.getPipelineRuns(false);
		},

        openParameters(pipelineRun) {
            this.showParametersDialog = true;
            this.selectedRun = pipelineRun;

            this.stage_metrics_formatted_data = Object.entries(pipelineRun.stages_metrics).map(([key, value]) => ({
                stage: key,
                work_items_count: value.work_items_count,
                completed_work_items_count: value.completed_work_items_count,
                successful_work_items_count: value.successful_work_items_count,
                start_timestamp: this.getFormattedDate(value.start_timestamp),
                last_update_timestamp: this.getFormattedDate(value.last_update_timestamp),
                duration: this.formatDuration(Math.max(0, new Date(value.last_update_timestamp).getTime() - new Date(value.start_timestamp).getTime()))
            }));
        },

        closeParametersDialog() {
            this.stage_metrics_formatted_data = [];
            this.showParametersDialog = false;
            this.selectedRun = null;
        },
	},
};
</script>

<style lang="scss" scoped>
.table__button {
	color: var(--primary-button-bg);
}

.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.grid--loading {
	pointer-events: none;
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

.dialog-content {
	display: flex;
	flex-direction: column;
	gap: 1.5rem;
}

.details-grid {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 1rem;
}

.detail-item h4 {
	margin: 0 0 0.5rem;
}

.pipeline-runs-table {
	width: 100%;
	border-collapse: collapse;
	margin-top: 1rem;
}

.pipeline-runs-table th,
.pipeline-runs-table td {
	padding: 0.5rem;
	text-align: left;
}

.status-pill,
.success-pill {
	display: inline-block;
	padding: 0.25rem 0.75rem;
	border-radius: 999px;
	font-size: 0.85rem;
	font-weight: 600;
}

.status-pill--completed {
	background-color: #e6f4ea;
	color: #137333;
}

.status-pill--inprogress {
	background-color: #e8f0fe;
	color: #174ea6;
}

.success-pill--true {
	background-color: #e6f4ea;
	color: #137333;
}

.success-pill--false {
	background-color: #fce8e6;
	color: #c5221f;
}

.filters {
	display: flex;
	flex-wrap: wrap;
	gap: 0.75rem;
	padding: 0.75rem 1rem;
	margin-bottom: 0.75rem;
	align-items: flex-end;
	background-color: #ffffff;
	border-radius: 8px;
	box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.filter-group {
	display: flex;
	flex-direction: column;
	font-size: 0.875rem;
}

.filter-group label {
	margin-bottom: 0.25rem;
	font-weight: 600;
}

.filter-actions {
	margin-left: auto;
	display: flex;
	gap: 0.5rem;
	align-items: flex-end;
}

.datetime-input {
	border-radius: 4px;
	max-width: 250px;
}
</style>
