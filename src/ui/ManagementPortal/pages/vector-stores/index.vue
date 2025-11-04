<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Vector Stores</h2>
				<div class="page-subheader">The following vector stores are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/vector-stores/create" tabindex="-1">
					<Button aria-label="Create vector store">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Vector Store
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading vector stores">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:globalFilterFields="['resource.name']"
				v-model:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:value="vectorStores"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search vector stores" />
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="getVectorStores"
						/>
					</div>
				</template>

				<template #empty>
					<div role="alert" aria-live="polite">No vector stores found.</div>
				</template>

				<template #loading>Loading vector stores. Please wait.</template>

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

				<!-- Type -->
				<Column
					field="resource.indexer"
					header="Source Indexer"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Edit -->
				<Column
					header="Edit"
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
						<NuxtLink
							:to="'/vector-stores/edit/' + data.resource.name"
							class="table__button"
							tabindex="-1"
						>
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button link :aria-label="`Edit ${data.resource.name}`">
									<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
								</Button>
								<template #popper
									><div role="tooltip">Edit {{ data.resource.name }}</div></template
								>
							</VTooltip>
						</NuxtLink>
					</template>
				</Column>

				<!-- Delete -->
				<Column
					header="Delete"
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
						<VTooltip :auto-hide="false" :popper-triggers="['hover']">
							<Button
								link
								:aria-label="`Delete ${data.resource.name}`"
								@click="vectorStoreToDelete = data.resource"
							>
								<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
							<template #popper
								><div role="tooltip">Delete {{ data.resource.name }}</div></template
							>
						</VTooltip>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Delete agent dialog -->
		<ConfirmationDialog
			v-if="vectorStoreToDelete !== null"
			:visible="vectorStoreToDelete !== null"
			header="Delete Vector Store"
			confirm-text="Yes"
			cancel-text="Cancel"
			confirm-button-severity="danger"
			@confirm="handleDeleteVectorStore"
			@cancel="vectorStoreToDelete = null"
			@update:visible="vectorStoreToDelete = null"
		>
			Do you want to delete the vector store "{{ vectorStoreToDelete!.name }}"?
		</ConfirmationDialog>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { DataSource, ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'PublicDataSources',

	data() {
		return {
			dataSources: [] as ResourceProviderGetResult<DataSource>[],
			vectorStores: [] as [],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
			dataSourceToDelete: null as DataSource | null,
			vectorStoreToDelete: null,
		};
	},

	async created() {
		await this.getAgentDataSources();
		await this.getVectorStores();
	},

	beforeUnmount() {
		// Clear filters when leaving the component
		this.filters.global.value = null;
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

		async getVectorStores() {
			this.loading = true;
			try {
				this.vectorStores = await api.getIndexingProfiles();
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

			await this.getAgentDataSources();
		},

		async handleDeleteVectorStore() {
			try {
				await api.deleteIndexingProfile(this.vectorStoreToDelete!.name);
				this.vectorStoreToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getVectorStores();
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
</style>
