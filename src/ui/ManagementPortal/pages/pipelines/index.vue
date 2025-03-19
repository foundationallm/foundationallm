<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Pipelines</h2>
				<div class="page-subheader">The following pipelines are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/pipelines/create" tabindex="-1">
					<Button aria-label="Create Pipeline">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Pipeline
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:value="pipelines"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #empty>
					<div role="alert" aria-live="polite">No vector stores found.</div>
				</template>

				<template #loading>Loading pipelines. Please wait.</template>

				<!-- Name -->
				<Column
					field="resource.name"
					header="Name"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Description -->
				<Column
					field="resource.description"
					header="Description"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Trigger -->
				<Column
					field="resource.trigger_type"
					header="Trigger"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Active -->
				<Column
					field="resource.active"
					header="Active"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- View -->
				<Column
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
						<NuxtLink :to="`/pipelines/edit/${data.resource.name}`" tabindex="-1">
							<Button link label="View" :aria-label="`View ${data.resource.name}`" />
						</NuxtLink>
					</template>
				</Column>

				<!-- Run -->
				<Column
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
							link
							label="Run"
							:aria-label="`Delete ${data.resource.name}`"
							@click="vectorStoreToDelete = data.resource"
						/>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- View Pipeline -->
		<Dialog :visible="pipelineToView !== null" modal :header="pipelineToView?.name">
			<div class="dialog-content">
				<div class="details-grid">
					<div class="detail-item">
						<h4>Description</h4>
						<p>{{ pipelineToView?.description || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Data Source</h4>
						<p>{{ pipelineToView?.data_source || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Trigger</h4>
						<p>{{ pipelineToView?.trigger_type || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Embedding</h4>
						<p>{{ pipelineToView?.embedding || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Active</h4>
						<p>{{ pipelineToView?.active ? 'Yes' : 'No' }}</p>
					</div>
					<div class="detail-item">
						<h4>Vector Store</h4>
						<p>{{ pipelineToView?.vector_store || '-' }}</p>
					</div>
				</div>

				<h3>Pipeline Runs</h3>
				<table class="pipeline-runs-table">
					<thead>
						<tr>
							<th>Start</th>
							<th>End</th>
							<th>Status</th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						<tr v-for="run in pipelineToView?.runs || []" :key="run.id">
							<td>{{ run.start }}</td>
							<td>{{ run.end }}</td>
							<td>{{ run.status }}</td>
							<td>
								<button class="view-button" @click="viewRun(run)">View</button>
							</td>
						</tr>
					</tbody>
				</table>
			</div>
		</Dialog>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { /* DataSource, */ ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'Pipelines',

	data() {
		return {
			// dataSources: [] as ResourceProviderGetResult<DataSource>[],
			pipelines: [] as ResourceProviderGetResult<Pipeline>[],
			pipelineToView: null,
			// vectorStores: [] as [],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			// dataSourceToDelete: null as DataSource | null,
			// vectorStoreToDelete: null,
		};
	},

	watch: {
		pipelineToView(newValue) {
			if (newValue) {
				// this.getPipelineRuns(newValue.name);
				this.getPipeline(newValue.name);
			}
		},
	},

	async created() {
		// await this.getAgentDataSources();
		// await this.getVectorStores();
		await this.getPipelines();
	},

	methods: {
		// async getAgentDataSources() {
		// 	this.loading = true;
		// 	try {
		// 		this.dataSources = await api.getAgentDataSources();
		// 	} catch (error) {
		// 		this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}
		// 	this.loading = false;
		// },

		async getPipelines() {
			this.loading = true;
			try {
				this.pipelines = await api.getPipelines();
				console.log(this.pipelines);
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async getPipeline(pipelineName: string) {
			try {
				const pipeline = await api.getPipeline(pipelineName);
				console.log(pipeline);
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},

		// async getPipelineRuns(pipelineName: string) {
		//     try {
		//         const runs = await api.getPipelineRuns(pipelineName);
		//         console.log(runs);
		//     } catch (error) {
		//         this.$toast.add({
		//             severity: 'error',
		//             detail: error?.response?._data || error,
		//             life: 5000,
		//         });
		//     }
		// },

		// async getVectorStores() {
		// 	this.loading = true;
		// 	try {
		// 		this.vectorStores = await api.getIndexingProfiles();
		// 	} catch (error) {
		// 		this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}
		// 	this.loading = false;
		// },

		// async handleDeleteDataSource() {
		// 	try {
		// 		await api.deleteDataSource(this.dataSourceToDelete!.name);
		// 		this.dataSourceToDelete = null;
		// 	} catch (error) {
		// 		return this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}

		// 	await this.getAgentDataSources();
		// },

		// async handleDeleteVectorStore() {
		// 	try {
		// 		await api.deleteIndexingProfile(this.vectorStoreToDelete!.name);
		// 		this.vectorStoreToDelete = null;
		// 	} catch (error) {
		// 		return this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}

		// 	await this.getVectorStores();
		// },
	},
};
</script>

<style lang="scss">
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
