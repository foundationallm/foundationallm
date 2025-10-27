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
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:value="pipelineRuns"
				striped-rows
				scrollable
				:sort-field="'created_on'"
				:sort-order="-1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex gap-4">
						<!-- <TableSearch v-model="filters" placeholder="Search by name" /> -->
                        <Dropdown
                            v-model="selectedPipelineName"
                            :options="pipelineNames"
                            option-label="value"
                            option-value="value"
                            placeholder="--Select Data Pipeline--"
                            @change="getPipelineRuns"
					    />
                        <div class="flex align-items-center justify-content-center gap-2" style="padding-top: 0.75rem;">
                            <Checkbox
                                inputId="allCB"
                                v-model="showAll"
                                binary
                                size="large"
                                @change="getPipelineRuns"
                            />
                            <label for="allCB">All</label>
                            <Checkbox
                                inputId="completedCB"
                                v-model="completed"
                                binary
                                size="large"
                                :disabled="showAll"
                                @change="getPipelineRuns"
                            />
                            <label for="completedCB">Completed</label>
                            <Checkbox
                                inputId="successfulCB"
                                v-model="successful"
                                binary
                                size="large"
                                :disabled="showAll"
                                @change="getPipelineRuns"
                            />
                            <label for="successfulCB">Successful</label>
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

				<!-- Name -->
				<!-- <Column
					field="name"
					header="Name"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column> -->

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
					header="Completed"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Successful -->
				<Column
					field="successful"
					header="Successful"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

                <!-- Active Stages -->
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
		</div>

        <!-- Trigger Pipeline Modal -->
		<Dialog 
            :visible="showParametersDialog" 
            modal
            closable
            :header="`Data Pipeline Run`"
            @update:visible="closeParametersDialog"
            >
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

                <h4>Parameters</h4>
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

                <h4>Stage Metrics</h4>

                <DataTable :value="stage_metrics_formatted_data">
                    <Column field="stage" header="Stage"></Column>
                    <Column field="work_items_count" header="Work Items"></Column>
                    <Column field="completed_work_items_count" header="Completed"></Column>
                    <Column field="successful_work_items_count" header="Successful"></Column>
                    <Column field="start_timestamp" header="Start Time"></Column>
                    <Column field="last_update_timestamp" header="End Time"></Column>
                    <Column field="duration" header="Duration (mm:ss.ms)"></Column>
                </DataTable>

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
			pipelineRuns: [],
            pipelineNames: [],
            selectedRun: null,
            stage_metrics_formatted_data: [],
            selectedPipelineName: null as string | null,
            showParametersDialog: false as boolean,
            showAll: true as boolean,
            completed: true as boolean,
            successful: true as boolean,
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			}
		};
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
				var pipelines = await api.getPipelines();
                this.pipelineNames = pipelines.map((pipeline) => ({
                    value: pipeline.resource.name
                }));
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async getPipelineRuns() {
			this.loading = true;
			try {

                const payload = {
                    data_pipeline_name_filter: this.selectedPipelineName,
                    completed: this.showAll ? null : this.completed,
                    successful: this.showAll ? null : this.successful,
                    //start_time: "2025-06-26",
			    };

                var response = await api.getPipelineRuns(this.selectedPipelineName || '', payload);
				this.pipelineRuns = response.resource.resources;

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
</style>
