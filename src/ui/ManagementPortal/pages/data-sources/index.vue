<template>
	<div>
		<div style="display: flex;">
			<div style="flex: 1">
				<h2 class="page-header">Data Sources</h2>
				<div class="page-subheader">The following data sources are available.</div>
			</div>

			<div style="display: flex; align-items: center;">
				<NuxtLink to="/data-sources/create">
					<Button>
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px;"></i>
						Create Data Source
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable :value="dataSources" stripedRows scrollable tableStyle="max-width: 100%" size="small">
				<template #empty>
					No data sources found. Please use the menu on the left to create a new data source.</template
				>
				<template #loading>Loading data sources. Please wait.</template>

				<!-- Name -->
				<Column field="name" header="Name" sortable style="min-width: 200px" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, sortIcon: { style: { color: 'var(--primary-text)' } } }"></Column>

				<!-- Type -->
				<Column field="type" header="Source Type" sortable style="min-width: 200px" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, sortIcon: { style: { color: 'var(--primary-text)' } } }"></Column>

				<!-- Edit -->
				<Column header="Edit" headerStyle="width:6rem" style="text-align: center" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, headerContent: { style: { justifyContent: 'center' } } }">
					<template #body="{ data }">
						<NuxtLink :to="'/data-sources/edit/' + data.name" class="table__button">
							<Button link>
								<i class="pi pi-cog" style="font-size: 1.2rem"></i>
							</Button>
						</NuxtLink>
					</template>
				</Column>

				<!-- Delete -->
				<Column header="Delete" headerStyle="width:6rem" style="text-align: center" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, headerContent: { style: { justifyContent: 'center' } } }">
					<template #body="{ data }">
						<Button link @click="dataSourceToDelete = data">
							<i class="pi pi-trash" style="font-size: 1.2rem; color: var(--red-400);"></i>
						</Button>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Delete agent dialog -->
		<Dialog
			:visible="dataSourceToDelete !== null"
			modal
			header="Delete Data Source"
			:closable="false"
		>
			<p>Do you want to delete the data source "{{ dataSourceToDelete.name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="dataSourceToDelete = null" />
				<Button label="Delete" severity="danger" @click="handleDeleteDataSource" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Agent } from '@/js/types';

export default {
	name: 'PublicAgents',

	data() {
		return {
			dataSources: [] as Agent,
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			dataSourceToDelete: null as Agent | null,
		};
	},

	async created() {
		await this.getAgentDataSources();
	},

	methods: {
		async getAgentDataSources() {
			this.loading = true;
			try {
				this.dataSources = await api.getAgentDataSources();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteDataSource() {
			try {
				await api.deleteDataSource(this.dataSourceToDelete!.name);
				this.dataSourceToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getAgents();
		},
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
	position: absolute;
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
</style>
